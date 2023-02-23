@echo off

dotnet publish -c Release -r win-x64
dotnet publish -c Release -r win-x86
dotnet publish -c Release -r win-arm64

dotnet publish -c Release -r linux-x64
dotnet publish -c Release -r linux-arm
dotnet publish -c Release -r linux-arm64

dotnet publish -c Release -r osx-x64
