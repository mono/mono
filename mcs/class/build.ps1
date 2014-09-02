$ErrorActionPreference = "Stop"

Import-Module .\Invoke-MsBuild.psm1

$MSBUILD="C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe"
$RELEASE_ANY="/property:Configuration=Release;Platform=AnyCPU"
$RELEASE_ARM="/property:Configuration=Release;Platform=ARM"
$RELEASE_X86="/property:Configuration=Release;Platform=x86"


# Functions

Function Build()
{
    Write-Host "Building Mono.Data.Sqlite for Windows Store Apps..." -ForegroundColor Green
    
    Write-Host "Building projects for Windows Store (Windows 8)..." -ForegroundColor Green
    Write-Host "Building System.Transactions..." -ForegroundColor Cyan
    Run-Build -project System.Transactions/System.Transactions-netcore.csproj -target build -parameters $RELEASE_ANY
    Write-Host "Building System.Data..." -ForegroundColor Cyan
    Run-Build -project System.Data/System.Data-netcore.csproj -target build -parameters $RELEASE_ANY
    Write-Host "Building Mono.Data.Sqlite..." -ForegroundColor Cyan
    Run-Build -project Mono.Data.Sqlite/Mono.Data.Sqlite-netcore.csproj -target build -parameters $RELEASE_ARM
    Run-Build -project Mono.Data.Sqlite/Mono.Data.Sqlite-netcore.csproj -target build -parameters $RELEASE_X86
 
    Write-Host "Building projects for Windows Store (Silverlight for Windows Phone)..." -ForegroundColor Green
    Write-Host "Building System.Transactions..." -ForegroundColor Cyan
    Run-Build -project System.Transactions/System.Transactions-wp8.csproj -target build -parameters $RELEASE_ANY
    Write-Host "Building System.Data..." -ForegroundColor Cyan
    Run-Build -project System.Data/System.Data-wp8.csproj -target build -parameters $RELEASE_ANY
    Write-Host "Building Mono.Data.Sqlite..." -ForegroundColor Cyan
    Run-Build -project Mono.Data.Sqlite/MonoDataSqliteDllImport.vcxproj -target build -parameters $RELEASE_ARM
    Run-Build -project Mono.Data.Sqlite/Mono.Data.Sqlite-wp8.csproj -target build -parameters $RELEASE_ARM
    Run-Build -project Mono.Data.Sqlite/MonoDataSqliteDllImport.vcxproj -target build -parameters $RELEASE_X86
    Run-Build -project Mono.Data.Sqlite/Mono.Data.Sqlite-wp8.csproj -target build -parameters $RELEASE_X86
    
    Write-Host "Building projects for Windows Store (WinRT for Windows Phone)..." -ForegroundColor Green
    Write-Host "Building System.Transactions..." -ForegroundColor Cyan
    Run-Build -project System.Transactions/System.Transactions-wpa81.csproj -target build -parameters $RELEASE_ANY
    Write-Host "Building System.Data..." -ForegroundColor Cyan
    Run-Build -project System.Data/System.Data-wpa81.csproj -target build -parameters $RELEASE_ANY
    Write-Host "Building Mono.Data.Sqlite..." -ForegroundColor Cyan
    Run-Build -project Mono.Data.Sqlite/Mono.Data.Sqlite-wpa81.csproj -target build -parameters $RELEASE_ARM
    Run-Build -project Mono.Data.Sqlite/Mono.Data.Sqlite-wpa81.csproj -target build -parameters $RELEASE_X86
    
    Write-Host "Build completed." -ForegroundColor Green
}

Function Clean()
{    
    Write-Host "Cleaning Mono.Data.Sqlite for Windows Store Apps..." -ForegroundColor Green
    
    Run-Build -project System.Transactions/System.Transactions-netcore.csproj -target clean -parameters $RELEASE_ANY
    Run-Build -project System.Data/System.Data-netcore.csproj -target clean -parameters $RELEASE_ANY
    Run-Build -project Mono.Data.Sqlite/Mono.Data.Sqlite-netcore.csproj -target clean -parameters $RELEASE_ARM
    Run-Build -project Mono.Data.Sqlite/Mono.Data.Sqlite-netcore.csproj -target clean -parameters $RELEASE_X86
 
    Run-Build -project System.Transactions/System.Transactions-wp8.csproj -target clean -parameters $RELEASE_ANY
    Run-Build -project System.Data/System.Data-wp8.csproj -target clean -parameters $RELEASE_ANY
    Run-Build -project Mono.Data.Sqlite/MonoDataSqliteDllImport.vcxproj -target clean -parameters $RELEASE_ARM
    Run-Build -project Mono.Data.Sqlite/Mono.Data.Sqlite-wp8.csproj -target clean -parameters $RELEASE_ARM
    Run-Build -project Mono.Data.Sqlite/MonoDataSqliteDllImport.vcxproj -target clean -parameters $RELEASE_X86
    Run-Build -project Mono.Data.Sqlite/Mono.Data.Sqlite-wp8.csproj -target clean -parameters $RELEASE_X86
    
    Run-Build -project System.Transactions/System.Transactions-wpa81.csproj -target clean -parameters $RELEASE_ANY
    Run-Build -project System.Data/System.Data-wpa81.csproj -target clean -parameters $RELEASE_ANY
    Run-Build -project Mono.Data.Sqlite/Mono.Data.Sqlite-wpa81.csproj -target clean -parameters $RELEASE_ARM
    Run-Build -project Mono.Data.Sqlite/Mono.Data.Sqlite-wpa81.csproj -target clean -parameters $RELEASE_X86

    Write-Host "Clean completed." -ForegroundColor Green
}

Function Test()
{    
    Write-Host "Runing unit tests for Mono.Data.Sqlite for Windows Store Apps..." -ForegroundColor Green

    Write-Host "Tests to be implemented." -ForegroundColor Yellow

    Write-Host "Unit tests completed." -ForegroundColor Green
}

Function Run-Build($project, $target, $parameters)
{
    $minimal = ($project + ".minimal.log")
    $detailed = ($project + ".detailed.log")
    $success = Invoke-MsBuild -Path $project -MsBuildParameters "/target:$target $parameters /fl1 /fl2 /flp1:Logfile=$minimal;Verbosity=Minimal /flp2:Logfile=$detailed;Verbosity=Detailed" -KeepBuildLogOnSuccessfulBuilds
    $contents = Get-Content $minimal -Delimiter "\n"
    Write-Host $contents
    If (-Not $success)
    {
        Write-Host "The build failed. See '$detailed' for more information." -ForegroundColor Red
        Write-Error "Build failed, aborting." -Category OperationStopped        
    }
}


# Script
If ($args.Count -eq 0)
{
    $args = "clean", "build"
}
foreach ($action in $args) 
{
    If ($action -ieq "build")
    {
        Build
    }
    ElseIf ($action -ieq "clean")
    {
        Clean
    }
    ElseIf ($action -ieq "test")
    {
        Test
    }
    Else
    {
        Write-Error "Unknown command: '$action'." -Category InvalidArgument
    }
}

# CMD WP Unit Test launcher - translate to PS
#   @echo off
#   if "%1" == "" goto error
#   
#   echo Setting up variables...
#   
#   if "%2" == "" (
#   echo 1 %1
#     set platform=emulator
#     set xapfile=%1
#     set testout=%~nx1
#   ) else (
#   echo 2 %2
#     set platform=%1
#     set xapfile=%2
#     set testout=%~nx2
#   )
#   if "%platform%" == "device" (
#     set platform=ARM
#     set device=Device
#   ) else (
#     set platform=x86
#     set device=Emulator WVGA
#   )
#   
#   echo Creating .runsettings...
#   
#   echo ^<?xml version="1.0" encoding="utf-8"?^> > settings.runsettings
#   echo ^<RunSettings^> >> settings.runsettings
#   echo   ^<RunConfiguration^> >> settings.runsettings
#   echo     ^<ResultsDirectory^>.\%testout%.TestResults^</ResultsDirectory^> >> settings.runsettings
#   echo     ^<TargetPlatform^>%platform%^</TargetPlatform^> >> settings.runsettings
#   echo     ^<TargetFrameworkVersion^>Framework45^</TargetFrameworkVersion^> >> settings.runsettings
#   echo   ^</RunConfiguration^> >> settings.runsettings
#   echo   ^<MSPhoneTest^> >> settings.runsettings
#   echo     ^<TargetDevice^>%device%^</TargetDevice^> >> settings.runsettings
#   echo   ^</MSPhoneTest^> >> settings.runsettings
#   echo ^</RunSettings^> >> settings.runsettings
#   echo ^<!-- test path = %xapfile% --^> >> settings.runsettings
#   
#   echo Starting tests...
#   "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" %xapfile% /logger:trx /settings:settings.runsettings /InIsolation
#   goto end
#   
#   :error
#   echo missing xap file argument!
#   echo usage "%0 [device|emulator] XAP_FILE_PATH" ...
#   
#   :end
#   echo.
#   echo Done.
#   
#   