use lib ('.', 'perl_lib', 'external/buildscripts/perl_lib');
use Cwd;
use File::Path;

use File::Copy::Recursive qw(dircopy);
use Getopt::Long;
use File::Basename;

my $root = getcwd();

my $monodistro = "$root/builds/monodistribution";
my $libmono = "$monodistro/lib/mono";

sub AddDotNetFolderToPath() {

	my @netFrameworkLocations = (
		$ENV{"SYSTEMROOT"}."/Microsoft.NET/Framework/v4.0.30319",
		$ENV{"SYSTEMROOT"}."/Microsoft.NET/Framework/v3.5"
	);

	my $netFrameworkLocation = "";
	my $checkedLocations = "";

	find_framework:
	foreach my $current (@netFrameworkLocations)
	{		
		if (-e $current) {
			$netFrameworkLocation = $current;
			last find_framework;
		}

		$checkedLocations = $checkedLocations . ", " . $current;
	}

	if ($netFrameworkLocation eq '') {
    	die("Could not find dotnet framework folder. Checked: $checkedLocations");
    }

	print("Using .Net framework: $netFrameworkLocation");
	$ENV{PATH} = "$ENV{PATH};$netFrameworkLocation";
}

AddDotNetFolderToPath();

my $output = Win32::GetLongPathName("$ENV{TEMP}") . "/output/BareMinimum";

print("\nEnvironment Path: $ENV{PATH}\n");

my $booCheckout = "$root/../../boo/build";
my $usCheckout = "$root/../../unityscript/build";

my $skipbuild=0;
GetOptions(
   "skipbuild=i"=>\$skipbuild,
) or die ("illegal cmdline options");

my $monodistroLibMono = "$monodistro/lib/mono";
my $monodistroUnity = "$monodistroLibMono/unity";

sub UnityBooc
{
	my $commandLine = shift;		
	system("$output/wsa/booc -debug- $commandLine") eq 0 or die("booc failed to execute: $commandLine");
}

sub BuildUnityScriptForUnity
{	
	# Build system is handling this
	if (!$ENV{UNITY_THISISABUILDMACHINE}) {
		GitClone("git://github.com/Unity-Technologies/boo.git", $booCheckout);
	}

	my $commonDefines = "NO_SERIALIZATION_INFO,NO_SYSTEM_PROCESS,NO_ICLONEABLE,MSBUILD,IGNOREKEYFILE";

	Build("$booCheckout/src/booc/Booc.csproj", undef, "/property:TargetFrameworkVersion=4.0 /property:DefineConstants=\"" . $commonDefines . "\" /property:OutputPath=$output/wp8");
	Build("$booCheckout/src/booc/Booc.csproj", undef, "/property:TargetFrameworkVersion=4.0 /property:DefineConstants=\"" . $commonDefines . ",NO_SYSTEM_REFLECTION_EMIT\" /property:OutputPath=$output/wsa");
	
	if (!$ENV{UNITY_THISISABUILDMACHINE}) {
		GitClone("git://github.com/Unity-Technologies/unityscript.git", $usCheckout);
	}
	
	UnityBooc("-out:$output/wsa/Boo.Lang.Extensions.dll -srcdir:$booCheckout/src/Boo.Lang.Extensions -r:$output/wsa/Boo.Lang.dll -r:$output/wsa/Boo.Lang.Compiler.dll");
	UnityBooc("-out:$output/wsa/Boo.Lang.Useful.dll -srcdir:$booCheckout/src/Boo.Lang.Useful -r:$output/wsa/Boo.Lang.Parser");
	UnityBooc("-out:$output/wsa/Boo.Lang.PatternMatching.dll -srcdir:$booCheckout/src/Boo.Lang.PatternMatching");

	my $UnityScriptLangDLL = "$output/UnityScript.Lang.dll";
	UnityBooc("-out:$UnityScriptLangDLL -srcdir:$usCheckout/src/UnityScript.Lang -r:$output/wsa/Boo.Lang.Extensions.dll");
}

sub Build
{
	my $projectFile = shift;
	
	my $optionalConfiguration = shift; 
	my $configuration = defined($optionalConfiguration) ? $optionalConfiguration : "Release";
	
	my $optionalCustomArguments = shift;
	my $customArguments = defined($optionalCustomArguments) ? $optionalCustomArguments : "";

	my $target = "Rebuild";
	my $commandLine = "MSBuild $projectFile /p:AssemblyOriginatorKeyFile= /p:SignAssembly=false /p:MonoTouch=True /t:$target /p:Configuration=$configuration $customArguments";
	
	system($commandLine) eq 0 or die("Failed to xbuild '$projectFile' for unity");
}

sub GitClone
{
	my $repo = shift;
	my $localFolder = shift;
	my $branch = shift;
	$branch = defined($branch)?$branch:master;

	if (-d $localFolder) {
		return;
	}
	system("git clone --branch $branch $repo $localFolder") eq 0 or die("git clone $repo $localFolder failed!");
}

sub NormalizePath {
	my $path = shift;
	$path =~ s/\//\\/g;

	return $path;
}

sub cp
{
	my $cmdLine = shift;
	$cmdLine = NormalizePath($cmdLine);

	system("xcopy $cmdLine /s /y") eq 0 or die("failed to copy '$cmdLine'");	
	print "Copied: $cmdLine\n";
}

rmtree("$root/builds");
rmtree("$output");

BuildUnityScriptForUnity();

cp("$output/wsa/Boo.Lang.dll $libmono/bare-minimum/wsa/Boo.Lang.dll*");
cp("$output/wsa/Boo.Lang.pdb $libmono/bare-minimum/wsa/Boo.Lang.pdb*");
cp("$output/wp8/Boo.Lang.dll $libmono/bare-minimum/wp8/Boo.Lang.dll*");
cp("$output/wp8/Boo.Lang.pdb $libmono/bare-minimum/wp8/Boo.Lang.pdb*");
cp("$output/UnityScript.Lang.* $libmono/bare-minimum/UnityScript.Lang.*");

if($ENV{UNITY_THISISABUILDMACHINE})
{
	my %checkouts = (
		'mono-classlibs' => 'BUILD_VCS_NUMBER_Mono____Mono2_6_x_Unity3_x',
		'boo' => 'BUILD_VCS_NUMBER_Boo',
		'unityscript' => 'BUILD_VCS_NUMBER_UnityScript',
		'cecil' => 'BUILD_VCS_NUMBER_Cecil'
	);

	system("echo '' > $root/builds/versions.txt");
	for my $key (keys %checkouts) {
		system("echo \"$key = $ENV{$checkouts{$key}}\" >> $root/builds/versions.txt");
	}
}
