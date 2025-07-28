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
echo NakuruTool Modules Build Script
echo ===========================================
echo DateTime: !datetime!
echo.

REM Create modules directory structure
echo [0/6] Creating modules directory structure...
if not exist "modules" mkdir "modules"
if not exist "modules\CollectionImporter" mkdir "modules\CollectionImporter"
if not exist "modules\CollectionExporter" mkdir "modules\CollectionExporter"
if not exist "modules\CollectionImporter\backup" mkdir "modules\CollectionImporter\backup"
if not exist "modules\CollectionExporter\output" mkdir "modules\CollectionExporter\output"
echo Directory structure created.
echo.

REM Clean previous builds
echo [1/6] Cleaning previous builds...
cd CollectionImporter
dotnet clean --verbosity quiet
cd ..\CollectionExporter
dotnet clean --verbosity quiet
cd ..
echo Previous builds cleaned.
echo.

REM Build CollectionImporter
echo [2/6] Building CollectionImporter...
cd CollectionImporter
dotnet build --configuration Release --verbosity quiet
if errorlevel 1 (
    echo ERROR: CollectionImporter build failed!
    pause
    exit /b 1
)
cd ..
echo CollectionImporter build completed successfully.
echo.

REM Build CollectionExporter
echo [3/6] Building CollectionExporter...
cd CollectionExporter
dotnet build --configuration Release --verbosity quiet
if errorlevel 1 (
    echo ERROR: CollectionExporter build failed!
    pause
    exit /b 1
)
cd ..
echo CollectionExporter build completed successfully.
echo.

REM Copy CollectionImporter files
echo [4/6] Copying CollectionImporter files...
copy "CollectionImporter\bin\Release\net6.0\CollectionImporter.exe" "modules\CollectionImporter\" >nul
copy "CollectionImporter\bin\Release\net6.0\CollectionImporter.deps.json" "modules\CollectionImporter\" >nul
copy "CollectionImporter\bin\Release\net6.0\CollectionImporter.runtimeconfig.json" "modules\CollectionImporter\" >nul
copy "CollectionImporter\bin\Release\net6.0\*.dll" "modules\CollectionImporter\" >nul
copy "CollectionImporter\config.json" "modules\CollectionImporter\" >nul
copy "CollectionImporter\README.md" "modules\CollectionImporter\" >nul

REM Create input directory and copy sample files if they exist
if not exist "modules\CollectionImporter\input" mkdir "modules\CollectionImporter\input"
if exist "CollectionImporter\bin\Release\net6.0\Input\*.json" (
    copy "CollectionImporter\bin\Release\net6.0\Input\*.json" "modules\CollectionImporter\input\" >nul
)

echo CollectionImporter files copied successfully.
echo.

REM Copy CollectionExporter files
echo [5/6] Copying CollectionExporter files...
copy "CollectionExporter\bin\Release\net6.0\CollectionExporter.exe" "modules\CollectionExporter\" >nul
copy "CollectionExporter\bin\Release\net6.0\CollectionExporter.deps.json" "modules\CollectionExporter\" >nul
copy "CollectionExporter\bin\Release\net6.0\CollectionExporter.runtimeconfig.json" "modules\CollectionExporter\" >nul
copy "CollectionExporter\bin\Release\net6.0\*.dll" "modules\CollectionExporter\" >nul
copy "CollectionExporter\config.json" "modules\CollectionExporter\" >nul
copy "CollectionExporter\README.md" "modules\CollectionExporter\" >nul

echo CollectionExporter files copied successfully.
echo.

REM Create startup scripts
echo [6/6] Creating startup scripts...

REM Create CollectionImporter startup script
echo @echo off > "modules\CollectionImporter\run_importer.bat"
echo chcp 65001 ^>nul >> "modules\CollectionImporter\run_importer.bat"
echo echo === osu! Collection Importer === >> "modules\CollectionImporter\run_importer.bat"
echo echo. >> "modules\CollectionImporter\run_importer.bat"
echo dotnet CollectionImporter.dll >> "modules\CollectionImporter\run_importer.bat"
echo pause >> "modules\CollectionImporter\run_importer.bat"

REM Create CollectionExporter startup script
echo @echo off > "modules\CollectionExporter\run_exporter.bat"
echo chcp 65001 ^>nul >> "modules\CollectionExporter\run_exporter.bat"
echo echo === osu! Collection Exporter === >> "modules\CollectionExporter\run_exporter.bat"
echo echo. >> "modules\CollectionExporter\run_exporter.bat"
echo dotnet CollectionExporter.dll >> "modules\CollectionExporter\run_exporter.bat"
echo pause >> "modules\CollectionExporter\run_exporter.bat"

REM Create unified README
echo # NakuruTool Modules > "modules\README.md"
echo. >> "modules\README.md"
echo このフォルダにはNakuruToolで使用するコンソールアプリケーションの実行環境が含まれています。 >> "modules\README.md"
echo. >> "modules\README.md"
echo ## 含まれるモジュール >> "modules\README.md"
echo. >> "modules\README.md"
echo ### CollectionImporter >> "modules\README.md"
echo - **機能**: JSON形式のコレクションファイルをosu!のcollection.dbにインポート >> "modules\README.md"
echo - **実行方法**: `CollectionImporter\run_importer.bat` をダブルクリック >> "modules\README.md"
echo - **設定ファイル**: `CollectionImporter\config.json` >> "modules\README.md"
echo. >> "modules\README.md"
echo ### CollectionExporter >> "modules\README.md"
echo - **機能**: osu!のcollection.dbをJSON形式でエクスポート >> "modules\README.md"
echo - **実行方法**: `CollectionExporter\run_exporter.bat` をダブルクリック >> "modules\README.md"
echo - **設定ファイル**: `CollectionExporter\config.json` >> "modules\README.md"
echo. >> "modules\README.md"
echo ## ビルド情報 >> "modules\README.md"
echo. >> "modules\README.md"
echo - **ビルド日時**: !datetime! >> "modules\README.md"
echo - **構成**: Release >> "modules\README.md"
echo - **ターゲットフレームワーク**: .NET 6.0 >> "modules\README.md"
echo. >> "modules\README.md"
echo ## 使用上の注意 >> "modules\README.md"
echo. >> "modules\README.md"
echo - このフォルダ全体を移動する場合は、サブフォルダも含めて移動してください >> "modules\README.md"
echo - 実行前に各モジュールのconfig.jsonを適切に設定してください >> "modules\README.md"
echo - バックアップフォルダとoutputフォルダは自動生成されます >> "modules\README.md"

echo Startup scripts and documentation created.
echo.

echo ===========================================
echo Build and copy completed successfully!
echo.
echo Modules directory: modules\
echo.
echo Contents:
echo   CollectionImporter\
echo     - CollectionImporter.exe (実行ファイル)
echo     - run_importer.bat (起動スクリプト)
echo     - config.json (設定ファイル)
echo     - input\ (入力フォルダ)
echo     - backup\ (バックアップフォルダ)
echo.
echo   CollectionExporter\
echo     - CollectionExporter.exe (実行ファイル)
echo     - run_exporter.bat (起動スクリプト)
echo     - config.json (設定ファイル)
echo     - output\ (出力フォルダ)
echo.
echo Build summary:
echo - Built CollectionImporter (Release)
echo - Built CollectionExporter (Release)
echo - Copied all required files
echo - Created startup scripts
echo - Generated documentation
echo ===========================================

pause