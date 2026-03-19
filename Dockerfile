# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG GITHUB_TOKEN
WORKDIR /src

# Copy solution and project files
COPY ["PayrollEngine.McpServer.sln", "./"]
COPY ["McpServer/PayrollEngine.McpServer.csproj", "McpServer/"]
COPY ["Tools/PayrollEngine.McpServer.Tools.csproj", "Tools/"]
COPY ["Tests/PayrollEngine.McpServer.Tests.csproj", "Tests/"]
COPY ["Directory.Build.props", "./"]

# Configure GitHub Packages NuGet source
RUN dotnet nuget add source "https://nuget.pkg.github.com/Payroll-Engine/index.json" \
    --name github \
    --username github-actions \
    --password ${GITHUB_TOKEN} \
    --store-password-in-clear-text

# Restore dependencies (cached layer)
RUN dotnet restore "PayrollEngine.McpServer.sln"

# Copy remaining source files and publish
COPY . .
WORKDIR "/src/McpServer"
RUN dotnet publish "PayrollEngine.McpServer.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PayrollEngine.McpServer.dll"]
