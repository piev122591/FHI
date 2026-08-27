# FHP Admin (ASP.NET Core Razor Pages + Tailwind CSS)

A small admin dashboard built with ASP.NET Core Razor Pages (.NET 10), cookie
authentication, and Tailwind CSS. User data is stored in a local JSON file
(`FHP.Web/App_Data/users.json`) behind a repository interface so it can be swapped
for a SQL Server-backed implementation later without touching the UI.

This is a port of the original classic ASP.NET Web Forms version of this app.
The Web Forms edition needed a Visual Studio install with the legacy "Web
Application Project" system (part of the ASP.NET and web development workload)
to open in Solution Explorer, and newer major Visual Studio releases have
dropped that project type entirely — opening it fails with "The application
which this project type is based on was not found." This version uses a plain
SDK-style project (`Microsoft.NET.Sdk.Web`), so it opens and runs in any current
Visual Studio, VS Code, or `dotnet` CLI with no extra workload required.

## Solution layout

- **FHP.Core** — class library: `User` model, `IUserRepository` /
  `JsonUserRepository`, `PasswordHasher`. No dependency on ASP.NET, so it's
  shared as-is by both the web app and the console tool below.
- **FHP.AdminSetup** — console app for creating an Admin user directly in
  `users.json` (see "Create the first admin account" below).
- **FHP.Web** — the Razor Pages application (`Pages/Login`, `Pages/Dashboard`,
  `Pages/Users`, `Pages/UserForm`, shared layout in `Pages/Shared/_Layout.cshtml`).

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/) 8 or later (developed against .NET 10).
- Visual Studio 2022/2026 with the **ASP.NET and web development** workload, VS
  Code with the C# extension, or just the `dotnet` CLI — any of these can open
  and run this solution.
- [Node.js](https://nodejs.org/) (for the Tailwind CLI build only — not required
  at runtime; `wwwroot/css/site.css` is already checked in so the app has styles
  even without running it).

## First-time setup

1. Open `FHP.slnx` in Visual Studio, or run `dotnet build` from this folder.
2. (Optional) Rebuild the Tailwind CSS if you change markup/classes:
   ```bash
   cd FHP.Web
   npm install
   npm run build
   ```
   Use `npm run dev` instead while actively iterating — it rebuilds
   `wwwroot/css/site.css` on every save.
3. Create the first admin account (see below).
4. Set `FHP.Web` as the startup project and press F5 (Visual Studio launches it
   under Kestrel), or run it from the command line:
   ```bash
   cd FHP.Web
   dotnet run
   ```

## Create the first admin account

There's no public registration page — on purpose, since this is the only account
that can manage every other user. Instead, run the console tool once:

```bash
cd FHP.AdminSetup
dotnet run
```

It will prompt for a full name, username, email, and password (masked input,
never echoed or logged), hash the password with PBKDF2, and append the new Admin
user to `FHP.Web\App_Data\users.json`. You can run it again any time you need to
create another admin account — it checks for duplicate usernames/emails first.

## Notes on the JSON data store

- All reads/writes go through `JsonUserRepository`, which serializes access with a
  lock and writes via a temp-file-then-replace swap to avoid a corrupted file if
  the process is interrupted mid-write.
- This is safe for a single running instance of the app. It is **not** safe for
  multiple instances/processes writing concurrently — that's one of the reasons
  this is meant to be replaced by SQL Server later.
- `App_Data` lives outside `wwwroot`, so ASP.NET Core's static file middleware
  never serves it over HTTP — no extra configuration needed to keep it private.

## Migrating to SQL Server later

Add a `SqlUserRepository : IUserRepository` in `FHP.Core.Services`, then change the
`IUserRepository` registration in `FHP.Web/Program.cs` (currently
`new JsonUserRepository(...)`) to construct `SqlUserRepository` instead. No page
or PageModel needs to change, since they all depend on `IUserRepository`, not the
JSON implementation.

## Deploying to Railway

The repo root has a `Dockerfile` (multi-stage: builds the Tailwind CSS bundle
with Node, then builds/publishes `FHP.Web` with the .NET SDK) and a
`railway.json` that pins Railway to that Dockerfile instead of its Nixpacks
auto-detect builder — required since Nixpacks doesn't reliably support a
mixed Node + very-recent-.NET build like this one.

1. **Attach a persistent Volume.** All app data (`users.json`, `userGroups.json`,
   `dashboardGroups.json`, Data Protection keys) lives on local disk by default,
   which Railway wipes on every redeploy. In the Railway dashboard, add a Volume
   to the service and mount it at, e.g., `/data`, then set the environment
   variable `DATA_DIR=/data`. `Program.cs` reads `DATA_DIR` and falls back to
   `App_Data` if it isn't set (so local dev is unaffected).
2. **Set the first-admin environment variables**: `ADMIN_USERNAME`,
   `ADMIN_EMAIL`, `ADMIN_PASSWORD` (and optionally `ADMIN_FULLNAME`). On
   startup, if no users exist yet in `DATA_DIR`, the app creates one Admin
   account from these — that's the only way to bootstrap access on a host
   where you can't run `FHP.AdminSetup` interactively. Safe to leave set after
   the first deploy; it only acts when the user store is empty.
3. **Port and HTTPS are already handled** — `Program.cs` binds Kestrel to
   Railway's injected `$PORT`, and trusts Railway's `X-Forwarded-Proto` header
   instead of redirecting every request to HTTPS itself (which would otherwise
   loop, since Railway terminates TLS at its edge).
4. **Keep it single-instance.** The JSON repositories use in-process file locks
   — safe for one running container, not safe for multiple replicas writing
   the same volume concurrently. Don't turn on horizontal autoscaling for this
   service without first migrating to a real database (see below).

## Security notes for this prototype

- Passwords are hashed with PBKDF2-HMACSHA256 (100,000 iterations, random salt per
  user) — never stored or logged in plain text.
- Sessions use ASP.NET Core cookie authentication; every page under
  `SecurePageModel` (`Dashboard`, `Users`, `UserForm`) requires `[Authorize]`,
  enforced by the framework before the page's code runs.
- Every POST in the app (login, save, delete, logout) is protected by Razor
  Pages' built-in antiforgery (CSRF) tokens — this is stronger than the original
  Web Forms version, which had no CSRF protection at all.
- Data Protection keys (used to encrypt auth cookies and antiforgery tokens) are
  persisted to `App_Data/keys` so logins survive an app restart during
  development. Treat that folder as sensitive; it's excluded from git.
- All user-supplied values are HTML-encoded automatically by Razor.
