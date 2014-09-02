$ErrorActionPreference = "Stop"

# powershell references
[Reflection.Assembly]::LoadWithPartialName("System.Linq") | Out-Null
[Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | Out-Null

# constants and helpers
$MSBUILDNS="http://schemas.microsoft.com/developer/msbuild/2003"

# directories
$TESTTEMPLATE_DIR=".\..\mstest"
$OUTPUT_DIR='.\..\lib'
$INTERMEDIATE_DIR='.\..\obj'

# generation set up
$TEST_PREFIX="tests-";
$PROJECTS=("System.Transactions", "System.Data", "Mono.Data.Sqlite")
$FRAMEWORKS=("wp8", "netcore", "store", "$($TEST_PREFIX)wp8", "$($TEST_PREFIX)netcore") # TODO: add "tests-store"

# any references to add:
# specific project: @{ platform=@{ task=@( @{ project=path } ) } }
# all projects:     @{ platform=@{ task=@( path ) } }
$REFS=@{
    "wp8"=@{
        "Reference"=@(
            @{"Mono.Data.Sqlite"="$OUTPUT_DIR\wp8\$('$(Platform)')\Mono.Data.Sqlite.DllImport.winmd"}
        );
    };
    "netcore"=@{
    };
    "store"=@{
    };
    "$($TEST_PREFIX)wp8"=@{
        "Reference"=@(
            @{"Mono.Data.Sqlite"="$OUTPUT_DIR\wp8\$('$(Platform)')\Mono.Data.Sqlite.DllImport.winmd"}
        );
    };
    "$($TEST_PREFIX)netcore"=@{
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
    $content | Where {$_} | foreach {
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
    $specific = [System.IO.Path]::Combine($path, ("$framework"+"_"+"$project$suffix"))
    If (Test-Path($specific)) {
        return GetSourceFilesRecursive -path $specific -framework $framework
    }
    return GetSourceFilesRecursive -path ([System.IO.Path]::Combine($path, "$project$suffix")) -framework $framework
}

Function GetContentFiles($path, $subfolder)
{
    return Get-ChildItem ([System.IO.Path]::Combine($path, $subfolder)) -Include *.xsd,*.xsc,*.xss,*.xml -Recurse | foreach {
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

    If ($isTest) {
        $realFramework = $framework.Substring($TEST_PREFIX.Length)
        $templatePath = "$TESTTEMPLATE_DIR\$realFramework"
        Copy-Item -Path "$templatePath\*" -Destination $destination -Exclude "UnitTestApp.csproj" -Recurse -Force
        $templatePath = "$templatePath\UnitTestApp.csproj"
    } Else {
        $realFramework = $framework
        $templatePath = "project_template_$framework.txt"
    }
    $destination = ".\$project"

    $xDoc = [System.Xml.Linq.XDocument]::Load("$templatePath")
    $xRoot = $xDoc.Element("{$MSBUILDNS}Project")
    $xInsertPoint=$xRoot.Element("{$MSBUILDNS}InsertReferencesHere")
    

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
        $xInsertPoint.AddBeforeSelf((New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}ItemGroup", [System.Xml.Linq.XObject[]]($references.Item($refType) | foreach {
            If ($_.GetType() -eq "".GetType()) {
                # plain string references
                New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}$refType", 
                    (New-Object System.Xml.Linq.XAttribute ("Include", $_)))
            } Else {
                If ($_.Keys[0] -eq $project) {
                    # project-specific references
                    $fname = $_.Values[0]
                    New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}$refType", [System.Xml.Linq.XObject[]](
                        (New-Object System.Xml.Linq.XAttribute ("Include", ([system.io.path]::GetFileNameWithoutExtension($fname)))),
                        (New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}HintPath", $fname))
                    ))
                }
            }
        }))))
    }
    
    
    # Dependencies
    $refRoot = New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}ItemGroup", $null)
    $xInsertPoint.AddBeforeSelf($refRoot)
    foreach ($proj in $PROJECTS | Where { -not $_.StartsWith($TEST_PREFIX) }) {
        If (($proj -eq $project) -and (-not $isTest)) {
            Break
        }
        $refRoot.Add((New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}Reference", [System.Xml.Linq.XObject[]](
            (New-Object System.Xml.Linq.XAttribute ("Include", $proj)),
            (New-Object System.Xml.Linq.XElement ("{$MSBUILDNS}HintPath", "$OUTPUT_DIR\$realFramework\$('$(Platform)')\$proj.dll"))
        ))))
        If ($proj -eq $project) {
            Break
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
    $xDoc.Save("$destination\$project-$framework.csproj")
}

$PROJECTS | foreach { $p = $_
    $FRAMEWORKS | foreach { $f = $_
        GenerateProjectFile -project $p -framework $f -references $REFS.Item($f)
    }
}
