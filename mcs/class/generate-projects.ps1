$ErrorActionPreference = "Stop"

Import-Module .\Invoke-MsBuild.psm1
$MSBUILD="C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe"

# powershell references
[Reflection.Assembly]::LoadWithPartialName("System.Linq") | Out-Null
[Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | Out-Null

# constants and helpers
$MSBUILDNS="http://schemas.microsoft.com/developer/msbuild/2003"
$MONO_VERSION="3.99.0.0" # this is obtained from somewhere

# directories
$TESTTEMPLATE_DIR=".\..\mstest"
$OUTPUT_DIR='.\..\lib'
$INTERMEDIATE_DIR='.\..\obj'
$COMMON_DIR='.\..\build\common'
$JAY_DIR=".\..\jay"

# generation set up
$TEST_PREFIX="tests-";
$PROJECTS=("System.Transactions", "System.Data", "Mono.Data.Sqlite")
$FRAMEWORKS=("wp8", "netcore", "store", "$($TEST_PREFIX)wp8", "$($TEST_PREFIX)netcore") # TODO: add "tests-store"

$CURRENT_DIR=(Get-Item -Path ".\" -Verbose).FullName

$REFS=@{
    "wp8"=@{
        "Reference"=@{
            "Mono.Data.Sqlite"=@(
                "$OUTPUT_DIR\wp8\$('$(Platform)')\Mono.Data.Sqlite.DllImport.winmd",
                "$OUTPUT_DIR\wp8\AnyCPU\System.Data.dll",
                "$OUTPUT_DIR\wp8\AnyCPU\System.Transactions.dll"
            );
            "System.Data"=@(
                "$OUTPUT_DIR\wp8\AnyCPU\System.Transactions.dll"
            );
        };
        "Constants"=@{
            "Mono.Data.Sqlite"="SQLITE_STANDARD";
            "System.Data"="INCLUDE_MONO_XML_SCHEMA"
        };
    };
    "netcore"=@{
        "Reference"=@{
            "Mono.Data.Sqlite"=@(
                "$OUTPUT_DIR\netcore\AnyCPU\System.Data.dll",
                "$OUTPUT_DIR\netcore\AnyCPU\System.Transactions.dll"
            );
            "System.Data"=@(
                "$OUTPUT_DIR\netcore\AnyCPU\System.Transactions.dll"
            );
        };
        "Constants"=@{
            "Mono.Data.Sqlite"="SQLITE_STANDARD";
            "System.Data"="INCLUDE_MONO_XML_SCHEMA"
        };
    };
    "store"=@{
        "Reference"=@{
            "Mono.Data.Sqlite"=@(
                "$OUTPUT_DIR\store\AnyCPU\System.Data.dll",
                "$OUTPUT_DIR\store\AnyCPU\System.Transactions.dll"
            );
            "System.Data"=@(
                "$OUTPUT_DIR\store\AnyCPU\System.Transactions.dll"
            );
        };
        "Constants"=@{
            "Mono.Data.Sqlite"="SQLITE_STANDARD";
            "System.Data"="INCLUDE_MONO_XML_SCHEMA"
        };
    };
    "$($TEST_PREFIX)wp8"=@{
        "Reference"=@{
            "Mono.Data.Sqlite"=@(
                "$OUTPUT_DIR\wp8\$('$(Platform)')\Mono.Data.Sqlite.DllImport.winmd",
                "$OUTPUT_DIR\wp8\$('$(Platform)')\Mono.Data.Sqlite.dll",
                "$OUTPUT_DIR\wp8\AnyCPU\System.Data.dll",
                "$OUTPUT_DIR\wp8\AnyCPU\System.Transactions.dll"
            );
            "System.Data"=@(
                "$OUTPUT_DIR\wp8\AnyCPU\System.Data.dll",
                "$OUTPUT_DIR\wp8\AnyCPU\System.Transactions.dll"
            );
            "System.Transactions"=@(
                "$OUTPUT_DIR\wp8\AnyCPU\System.Transactions.dll"
            );
        };
        "SDKReference"=@{
            "Mono.Data.Sqlite"=@(
                "SQLite.WP80, Version=3.8.7.3"
            );
        };
    };
    "$($TEST_PREFIX)netcore"=@{
        "Reference"=@{
            "Mono.Data.Sqlite"=@(
                "$OUTPUT_DIR\netcore\$('$(Platform)')\Mono.Data.Sqlite.dll",
                "$OUTPUT_DIR\netcore\AnyCPU\System.Data.dll",
                "$OUTPUT_DIR\netcore\AnyCPU\System.Transactions.dll"
            );
            "System.Data"=@(
                "$OUTPUT_DIR\netcore\AnyCPU\System.Data.dll",
                "$OUTPUT_DIR\netcore\AnyCPU\System.Transactions.dll"
            );
            "System.Transactions"=@(
                "$OUTPUT_DIR\netcore\AnyCPU\System.Transactions.dll"
            );
        };
        "SDKReference"=@{
            "Mono.Data.Sqlite"=@(
                "SQLite.WinRT, Version=3.8.4.3",
                "Microsoft.VCLibs, version=11.0"
            );
        };
    };
}


# FUNCTIONS

Function GetSourceFilesRecursive($path)
{
    $content = Get-Content -path $path

    $files = @()
    $prefix = ""
    If ($framework.StartsWith($TEST_PREFIX)) {
        $prefix = "Test\"
    }

    $include = "#include "
    $content | Where { $_ -and ($_.StartsWith($include) -or -not $_.StartsWith("#")) } | foreach {
        If ($_.StartsWith($include)) {
            $next = $_.SubString($include.Length)
            $specific = [System.IO.Path]::Combine([System.IO.Path]::GetDirectoryName($path), $next)
            $files = $files + (GetSourceFilesRecursive -path $specific)
        } Else {
            $files = $files + ("$prefix$($_.Replace('/', '\'))")
        }
    }

    return $files;
}
Function GetSourceFiles($path, $project, $framework)
{
    $suffix = ".dll.sources"
    If ($framework.StartsWith($TEST_PREFIX)) {
        $suffix = "_test" + $suffix
    }
    $specific = [System.IO.Path]::Combine($path, ("$framework".Replace($TEST_PREFIX,"")+"_"+"$project$suffix"))
    If (Test-Path($specific)) {
        return GetSourceFilesRecursive -path $specific -framework $framework
    }
    $general = [System.IO.Path]::Combine($path, "$project$suffix");
    return GetSourceFilesRecursive -path $general -framework $framework
}

Function GetContentFiles($path, $subfolder)
{
    return Get-ChildItem ([System.IO.Path]::Combine($path, $subfolder)) -Include *.xsd,*.xsc,*.xss,*.xml -Recurse -File | foreach {
        $tmp = Get-Location
        Set-Location $path
        $rel = Resolve-Path -relative $_
        Set-Location $tmp
        return $rel
    }
}

# main function that generates the 
Function GenerateProjectFile($project, $framework, $references)
{
    $isTest = $framework.StartsWith($TEST_PREFIX);
    $destination = ".\$project"

    If ($isTest) {
        $realFramework = $framework.Substring($TEST_PREFIX.Length)
        $templatePath = "$TESTTEMPLATE_DIR\$realFramework"
        Copy-Item -Path "$templatePath\*" -Destination $destination -Exclude "UnitTestApp.csproj" -Recurse -Force
        $templatePath = "$templatePath\UnitTestApp.csproj"
    } Else {
        $realFramework = $framework
        $templatePath = "project_template_$framework.txt"
    }

    $xDoc = [System.Xml.Linq.XDocument]::Load("$CURRENT_DIR\$templatePath")
    $xRoot = $xDoc.Element("{$MSBUILDNS}Project")
    $xInsertPoint=$xRoot.Element("{$MSBUILDNS}InsertReferencesHere")
    $xDefineConstants=$xRoot.Elements("{$MSBUILDNS}PropertyGroup").Elements("{$MSBUILDNS}DefineConstants")
    

    # Global Properties
    $xPropGrp=$xRoot.Element("{$MSBUILDNS}PropertyGroup")
    If ($isTest) {
        $name = $project+"_"+$framework.Replace("-", "_")
        $xPropGrp.Add((New-Object System.Xml.Linq.XElement (("{$MSBUILDNS}AssemblyName"), "$name")))
        $xPropGrp.Add((New-Object System.Xml.Linq.XElement (("{$MSBUILDNS}RootNamespace"), "$name")))
    } Else {
        $xPropGrp.Add((New-Object System.Xml.Linq.XElement (("{$MSBUILDNS}RootNamespace"), "")))
        $xPropGrp.Add((New-Object System.Xml.Linq.XElement (("{$MSBUILDNS}AssemblyName"), "$project")))
    }
    $xPropGrp.Add((New-Object System.Xml.Linq.XElement (("{$MSBUILDNS}OutputPath"), "$OUTPUT_DIR\$framework\$('$(Platform)')")))
    $xPropGrp.Add((New-Object System.Xml.Linq.XElement (("{$MSBUILDNS}BaseIntermediateOutputPath"), "$INTERMEDIATE_DIR\$('$(AssemblyName)')\$framework\$('$(Platform)')")))


    # References
    $references.Keys | foreach {
        $refType = $_;
        $refs = $references.Item("$refType").Item("$project")

        If ($refs) {
            # add assembly references
            If ($refType -eq "Reference") {
                $xInsertPoint.AddBeforeSelf((New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}ItemGroup", [System.Xml.Linq.XObject[]](
                    $refs | foreach {
                    New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}Reference", [System.Xml.Linq.XObject[]](
                        (New-Object System.Xml.Linq.XAttribute ("Include", ([system.io.path]::GetFileNameWithoutExtension($_)))),
                        (New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}HintPath", $_))
                    ))
                }))))
            } 
            # add extensions references
            If ($refType -eq "SDKReference") {
                $xInsertPoint.AddBeforeSelf((New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}ItemGroup", [System.Xml.Linq.XObject[]](
                    $refs | foreach {
                    New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}SDKReference", [System.Xml.Linq.XObject[]](
                        (New-Object System.Xml.Linq.XAttribute ("Include", $_))
                    ))
                }))))
            } 
            # add the values to <DefineConstants> for each <PropertyGroup>
            If ($refType -eq "Constants") {
                $xDefineConstants | foreach {
                    $_.Value = "$($_.Value);$refs"
                }
            }
        }
    }
    

    # Source Files
    $sources=GetSourceFiles -path $destination -framework $framework -project $project
    $xInsertPoint.AddBeforeSelf((New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}ItemGroup", [System.Xml.Linq.XObject[]]($sources | foreach {
        (New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}Compile", 
            (New-Object System.Xml.Linq.XAttribute ("Include", $_))))
    }))))
    

    # Content Files (for unit tests)
    If ($isTest) {
        $content=GetContentFiles -path $destination -subfolder "Test"
        $xInsertPoint.AddBeforeSelf((New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}ItemGroup", [System.Xml.Linq.XObject[]]($content | foreach {
            (New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}Content", 
                (New-Object System.Xml.Linq.XAttribute ("Include", $_))))
        }))))
    }
    

    $xInsertPoint.Remove();
    $xDoc.Save("$CURRENT_DIR\$destination\$project-$framework.csproj");

    Write-Host "Created $destination\$project-$framework.csproj"
}

# make sure that jay exists
If (-Not (Test-Path "$JAY_DIR\jay.exe")) 
{
    Write-Host "Building JAY..."
    Run-Build -project ".$JAY_DIR\jay.vcxproj" -target build -parameters ""
    Write-Host "Building JAY complete."
}

# build the Parser.cs file from the .jay
Get-Content "$JAY_DIR\skeleton.cs" | 
    & "$JAY_DIR\jay.exe" -vct .\System.Data\Mono.Data.SqlExpressions\Parser.jay | 
    Set-Content ".\System.Data\Mono.Data.SqlExpressions\Parser.cs"

Write-Host "Created .\System.Data\Mono.Data.SqlExpressions\Parser.cs"

# copy the consts.cs file, replacing @MONO_VERSION@
(Get-Content "$COMMON_DIR\Consts.cs.in") | 
    foreach {$_ -replace "@MONO_VERSION@", "$MONO_VERSION"} | 
    Set-Content "$COMMON_DIR\Consts.cs" -Force

# start the main build
$PROJECTS | foreach { $p = $_
    $FRAMEWORKS | foreach { $f = $_
        GenerateProjectFile -project $p -framework $f -references $REFS.Item($f)
    }
}
