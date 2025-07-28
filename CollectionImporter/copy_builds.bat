@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

REM Get current date and time
for /f "tokens=2-4 delims=/ " %%a in ('date /t') do (
    set month=%%a
    set day=%%b
    set year=%%c
)

for /f "tokens=1-2 delims=:. " %%a in ('time /t') do (
    set hour=%%a
    set minute=%%b
)

REM Format time to 24-hour format and add zero padding
if "!hour:~0,1!"==" " set hour=0!hour:~1,1!
if "!minute:~0,1!"==" " set minute=0!minute:~1,1!

REM Date and time format (YYYYMMDD_HHMM)
set datetime=!year!!month!!day!_!hour!!minute!

echo ===========================================
echo Collection Importer Build Copy Script
echo ===========================================
echo DateTime: !datetime!
echo.

REM Check if .csproj file exists
if not exist "CollectionImporter.csproj" (
    echo ERROR: CollectionImporter.csproj not found!
    echo Make sure you are running this script from the CollectionImporter project directory.
    pause
    exit /b 1
)

REM Clean previous builds
echo [0/4] Cleaning previous builds...
dotnet clean --verbosity quiet
echo Previous builds cleaned.
echo.

REM Build Debug configuration
echo [1/4] Building Debug configuration...
dotnet build --configuration Debug --verbosity quiet
if errorlevel 1 (
    echo ERROR: Debug build failed!
    pause
    exit /b 1
)
REM Verify Debug build output
if not exist "bin\Debug\net6.0\CollectionImporter.exe" (
    echo ERROR: Debug build output not found!
    pause
    exit /b 1
)
echo Debug build completed successfully.
echo.

REM Build Release configuration
echo [2/4] Building Release configuration...
dotnet build --configuration Release --verbosity quiet
if errorlevel 1 (
    echo ERROR: Release build failed!
    pause
    exit /b 1
)
REM Verify Release build output
if not exist "bin\Release\net6.0\CollectionImporter.exe" (
    echo ERROR: Release build output not found!
    pause
    exit /b 1
)
echo Release build completed successfully.
echo.

REM Copy Debug build
echo [3/4] Copying Debug build...
set debug_folder=!datetime!_debug
if exist "!debug_folder!" (
    echo Removing existing folder: !debug_folder!
    rmdir /s /q "!debug_folder!"
)
mkdir "!debug_folder!"

REM Copy required files for execution
copy "bin\Debug\net6.0\CollectionImporter.exe" "!debug_folder!\" >nul
copy "bin\Debug\net6.0\CollectionImporter.deps.json" "!debug_folder!\" >nul
copy "bin\Debug\net6.0\CollectionImporter.runtimeconfig.json" "!debug_folder!\" >nul

REM Copy all DLLs to same directory as executable
copy "bin\Debug\net6.0\*.dll" "!debug_folder!\" >nul

REM Copy config files and README
copy "config.json" "!debug_folder!\" >nul
copy "README.md" "!debug_folder!\" >nul

REM Copy input folder
xcopy "input" "!debug_folder!\input" /e /i /q >nul

REM Create backup folder
mkdir "!debug_folder!\backup" >nul

echo Debug build completed: !debug_folder!
echo.

REM Copy Release build
echo [4/4] Copying Release build...
set release_folder=!datetime!_release
if exist "!release_folder!" (
    echo Removing existing folder: !release_folder!
    rmdir /s /q "!release_folder!"
)
mkdir "!release_folder!"

REM Copy required files for execution
copy "bin\Release\net6.0\CollectionImporter.exe" "!release_folder!\" >nul
copy "bin\Release\net6.0\CollectionImporter.deps.json" "!release_folder!\" >nul
copy "bin\Release\net6.0\CollectionImporter.runtimeconfig.json" "!release_folder!\" >nul

REM Copy all DLLs to same directory as executable
copy "bin\Release\net6.0\*.dll" "!release_folder!\" >nul

REM Copy config files and README
copy "config.json" "!release_folder!\" >nul
copy "README.md" "!release_folder!\" >nul

REM Copy input folder
xcopy "input" "!release_folder!\input" /e /i /q >nul

REM Create backup folder
mkdir "!release_folder!\backup" >nul

echo Release build completed: !release_folder!
echo.

echo ===========================================
echo Build and copy completed successfully!
echo.
echo Debug build  : !debug_folder!
echo Release build: !release_folder!
echo.
echo Each folder contains:
echo - Executable file (CollectionImporter.exe)
echo - Runtime configuration files
echo - Dependency libraries
echo - Configuration file (config.json)
echo - Documentation (README.md)
echo - Input folder (input/) with sample files
echo - Backup folder (backup/)
echo.
echo Build process summary:
echo - Cleaned previous builds
echo - Built Debug configuration
echo - Built Release configuration  
echo - Copied all required files
echo ===========================================

pause