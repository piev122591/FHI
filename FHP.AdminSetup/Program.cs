using System;
using System.IO;
using FHP.Core.Models;
using FHP.Core.Services;

namespace FHP.AdminSetup
{
    /// <summary>
    /// One-time (or as-needed) console tool for creating an Admin user directly in
    /// App_Data/users.json. This exists so the web app never needs a public
    /// "create the first admin" page, and so no plaintext password ever has to be
    /// hand-edited into the JSON file.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("FHP Admin Setup");
            Console.WriteLine("===============");
            Console.WriteLine();

            string usersFilePath = ResolveUsersFilePath(args);
            Console.WriteLine($"Using data file: {usersFilePath}");
            Console.WriteLine();

            var repository = new JsonUserRepository(usersFilePath);

            string fullName = PromptRequired("Full name");
            string username = PromptRequired("Username");
            string email = PromptRequired("Email");

            if (repository.ExistsByUsernameOrEmail(username, email))
            {
                Console.WriteLine();
                Console.WriteLine("A user with that username or email already exists. Aborting.");
                Exit(1);
                return;
            }

            string password = PromptPassword();

            var user = new User
            {
                FullName = fullName,
                Username = username,
                Email = email,
                PasswordHash = PasswordHasher.Hash(password),
                Role = UserRoles.Admin,
                Status = UserStatuses.Active,
                CreatedDate = DateTime.Now
            };

            var created = repository.Add(user);

            Console.WriteLine();
            Console.WriteLine($"Admin user '{created.Username}' created with Id {created.Id}.");
            Console.WriteLine("You can now log in with this account from the Login page.");
            Exit(0);
        }

        private static string ResolveUsersFilePath(string[] args)
        {
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
                return Path.GetFullPath(args[0]);

            // Walk upward from the exe location looking for FHP.Web\App_Data so this
            // works whether we're run from bin\Debug\net48 or bin\Release\net48.
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; i < 8 && dir != null; i++)
            {
                string candidate = Path.Combine(dir.FullName, "FHP.Web", "App_Data");
                if (Directory.Exists(candidate))
                    return Path.Combine(candidate, "users.json");

                dir = dir.Parent;
            }

            Console.WriteLine("Could not auto-locate FHP.Web\\App_Data.");
            return PromptRequired("Full path to users.json");
        }

        private static string PromptRequired(string label)
        {
            string value;
            do
            {
                Console.Write($"{label}: ");
                value = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(value));

            return value.Trim();
        }

        private static string PromptPassword()
        {
            while (true)
            {
                string password = ReadMasked("Password: ");
                if (string.IsNullOrEmpty(password) || password.Length < 8)
                {
                    Console.WriteLine("Password must be at least 8 characters.");
                    continue;
                }

                string confirm = ReadMasked("Confirm password: ");
                if (password != confirm)
                {
                    Console.WriteLine("Passwords do not match. Try again.");
                    continue;
                }

                return password;
            }
        }

        private static string ReadMasked(string prompt)
        {
            Console.Write(prompt);

            // Console.ReadKey requires a real interactive console. Fall back to a
            // plain (unmasked) ReadLine when input is redirected, e.g. piped input
            // during scripted/automated runs.
            if (Console.IsInputRedirected)
            {
                return Console.ReadLine()?.Trim() ?? string.Empty;
            }

            var chars = new System.Collections.Generic.List<char>();

            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (chars.Count > 0)
                    {
                        chars.RemoveAt(chars.Count - 1);
                        Console.Write("\b \b");
                    }
                    continue;
                }

                if (key.KeyChar != '\0')
                {
                    chars.Add(key.KeyChar);
                    Console.Write('*');
                }
            }

            Console.WriteLine();
            return new string(chars.ToArray());
        }

        private static void Exit(int code)
        {
            if (!Console.IsInputRedirected)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
            }
            Environment.Exit(code);
        }
    }
}
