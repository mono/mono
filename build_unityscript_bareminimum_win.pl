use lib ('.', "perl_lib");
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

my $output = "$ENV{TEMP}/output/BareMinimum";

print("Environment Path: $ENV{PATH}\n");

my $dependencyBranchToUse = "unity3.0";

my $booCheckout = "$root/external/boo";
my $usCheckout = "$root/external/unityscript";

my $skipbuild=0;
GetOptions(
   "skipbuild=i"=>\$skipbuild,
) or die ("illegal cmdline options");

my $monodistroLibMono = "$monodistro/lib/mono";
my $monodistroUnity = "$monodistroLibMono/unity";

sub UnityBooc
{
	my $commandLine = shift;		
	system("$output/booc -debug- $commandLine") eq 0 or die("booc failed to execute: $commandLine");
}

sub BuildUnityScriptForUnity
{	
	# Build system is handling this
	if (!$ENV{UNITY_THISISABUILDMACHINE}) {
		GitClone("git://github.com/Unity-Technologies/boo.git", $booCheckout);
	}

	Build("$booCheckout/src/booc/Booc.csproj", undef, "/property:TargetFrameworkVersion=4.0 /property:DefineConstants=\"NO_SERIALIZATION_INFO,NO_SYSTEM_PROCESS,NO_ICLONEABLE,NO_SYSTEM_REFLECTION_EMIT,MSBUILD\" /property:OutputPath=$output");
	
	if (!$ENV{UNITY_THISISABUILDMACHINE}) {
		GitClone("git://github.com/Unity-Technologies/unityscript.git", $usCheckout);
	}
	
	UnityBooc("-out:$output/Boo.Lang.Extensions.dll -srcdir:$booCheckout/src/Boo.Lang.Extensions -r:$output/Boo.Lang.dll -r:$output/Boo.Lang.Compiler.dll");
	UnityBooc("-out:$output/Boo.Lang.Useful.dll -srcdir:$booCheckout/src/Boo.Lang.Useful -r:$output/Boo.Lang.Parser");
	UnityBooc("-out:$output/Boo.Lang.PatternMatching.dll -srcdir:$booCheckout/src/Boo.Lang.PatternMatching");

	my $UnityScriptLangDLL = "$output/UnityScript.Lang.dll";
	UnityBooc("-out:$UnityScriptLangDLL -srcdir:$usCheckout/src/UnityScript.Lang -r:$output/Boo.Lang.Extensions.dll");
}

sub Build
{
	my $projectFile = shift;
	
	my $optionalConfiguration = shift; 
	my $configuration = defined($optionalConfiguration) ? $optionalConfiguration : "Release";
	
	my $optionalCustomArguments = shift;
	my $customArguments = defined($optionalCustomArguments) ? $optionalCustomArguments : "";

	my $target = "Rebuild";
	my $commandLine = "MSBuild $projectFile /p:MonoTouch=True /t:$target /p:Configuration=$configuration $customArguments";
	
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

sub cp
{
	my $cmdLine = shift;
	$cmdLine =~ s/\//\\/g;

	system("xcopy $cmdLine /s /y") eq 0 or die("failed to copy '$cmdLine'");	
	print "Copied: $cmdLine\n";
}

rmtree("$root/builds");
rmtree("$output");

BuildUnityScriptForUnity();

cp("$output/Boo.Lang.dll $libmono/bare-minimum/Boo.Lang.dll*");
cp("$output/Boo.Lang.pdb $libmono/bare-minimum/Boo.Lang.pdb*");
cp("$output/UnityScript.Lang.* $libmono/bare-minimum/UnityScript.Lang.*");
cp("$booCheckout/src/Boo.sn* $libmono/bare-minimum/Boo.sn*");

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