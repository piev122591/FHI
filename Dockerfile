# syntax=docker/dockerfile:1

# ---- Stage 1: compile the Tailwind CSS bundle ----
FROM node:20-alpine AS css-build
WORKDIR /src/FHP.Web
COPY FHP.Web/package.json FHP.Web/package-lock.json* ./
RUN npm install
COPY FHP.Web/tailwind.config.js FHP.Web/postcss.config.js ./
COPY FHP.Web/Content ./Content
COPY FHP.Web/Pages ./Pages
COPY FHP.Web/wwwroot/js ./wwwroot/js
RUN npm run build

# ---- Stage 2: restore, build, and publish the .NET app ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build
WORKDIR /src
COPY FHP.Core/FHP.Core.csproj FHP.Core/
COPY FHP.Web/FHP.Web.csproj FHP.Web/
RUN dotnet restore FHP.Web/FHP.Web.csproj
COPY FHP.Core/ FHP.Core/
COPY FHP.Web/ FHP.Web/
COPY --from=css-build /src/FHP.Web/wwwroot/css/site.css FHP.Web/wwwroot/css/site.css
RUN dotnet publish FHP.Web/FHP.Web.csproj -c Release -o /app/publish --no-restore

# ---- Stage 3: runtime image ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=dotnet-build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "FHP.Web.dll"]
