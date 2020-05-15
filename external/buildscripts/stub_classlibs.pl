use Cwd;
use Cwd 'abs_path';
use File::Basename;
use File::Path;

if($^O ne "darwin")
{
	print ">>> The ProfileStubber is only built and run in the class library build on macOS\n";
	exit
}

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $extraBuildTools = "$monoroot/external/buildscripts/artifacts/Stevedore/mono-build-tools-extra";

my $profileRoot = "tmp/lib/mono";
my $referenceProfile = "$profileRoot/4.7.1-api";

print ">>> Modifying the unityjit-macos profile to match the .NET 4.7.1 API\n";

$result = system("mono",
					"$extraBuildTools/ProfileStubber.exe",
					"--reference-profile=$referenceProfile",
					"--stub-profile=$profileRoot/unityjit-macos");

if ($result ne 0)
{
	die("Failed to stub the unityjit-macos profile\n");
}

print ">>> Modifying the unityjit-win32 profile to match the .NET 4.7.1 API\n";

$result = system("mono",
					"$extraBuildTools/ProfileStubber.exe",
					"--reference-profile=$referenceProfile",
					"--stub-profile=$profileRoot/unityjit-win32");

if ($result ne 0)
{
	die("Failed to stub the unityjit-win32 profile\n");
}

print ">>> Modifying the unityjit-linux profile to match the .NET 4.7.1 API\n";

$result = system("mono",
					"$extraBuildTools/ProfileStubber.exe",
					"--reference-profile=$referenceProfile",
					"--stub-profile=$profileRoot/unityjit-linux");

if ($result ne 0)
{
	die("Failed to stub the unityjit-linux profile\n");
}

print ">>> Modifying the unityaot profile to match the .NET 4.7.1 API\n";

$result = system("mono",
					"$extraBuildTools/ProfileStubber.exe",
					"--reference-profile=$referenceProfile",
					"--stub-profile=$profileRoot/unityaot");

if ($result ne 0)
{
	die("Failed to stub the unityaot profile\n");
}