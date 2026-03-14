@echo off
rem publish tools assembly
dotnet publish ..\Tools\PayrollEngine.McpServer.Tools.csproj -c Release -o ..\publish

rem build docfx and start server
docfx docfx.json --serve --port 5866

rem open in browser
start "" http://localhost:5866/
