@echo off
setlocal

pushd "%~dp0"
dotnet restore -r win-x64 --configfile "%~dp0NuGet.Config"
if errorlevel 1 (
    popd
    exit /b %errorlevel%
)

dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true --no-restore
if errorlevel 1 (
    popd
    exit /b %errorlevel%
)

"%~dp0bin\Release\net10.0-windows\win-x64\publish\PhysX.exe" %*
set APP_EXIT=%errorlevel%
popd
exit /b %APP_EXIT%
