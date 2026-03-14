@echo off
rem publish tools assembly
echo Publishing tools assembly...
dotnet publish ..\Tools\PayrollEngine.McpServer.Tools.csproj -c Release -o ..\publish

rem build static site
echo.
echo Building static HTML reference...
echo.
docfx docfx.json
echo.
