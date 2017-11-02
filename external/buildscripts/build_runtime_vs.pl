sub CompileVCProj;
use Cwd 'abs_path';
use Getopt::Long;
use File::Spec;
use File::Basename;
use File::Copy;
use File::Path;

print ">>> PATH in Build VS = $ENV{PATH}\n\n";

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildsroot = "$monoroot/builds";
my $buildMachine = $ENV{UNITY_THISISABUILDMACHINE};

my $build = 0;
my $clean = 0;
my $arch32 = 0;
my $debug = 0;
my $gc = "bdwgc";
my $msBuildVersion = "";

GetOptions(
	'build=i'=>\$build,
	'clean=i'=>\$clean,
	'arch32=i'=>\$arch32,
	'debug=i'=>\$debug,
	'msbuildversion=s'=>\$msBuildVersion,
	'gc=s'=>\$gc,
) or die ("illegal cmdline options");

if ($build)
{
	CompileVCProj("$monoroot/msvc/mono.sln");
}

sub CompileVCProj
{
	my $sln = shift;
	my $config;

	my $msbuild = $ENV{"ProgramFiles(x86)"}."/MSBuild/$msBuildVersion/Bin/MSBuild.exe";

    $config = $debug ? "Debug" : "Release";
	my $arch = $arch32 ? "Win32" : "x64";
	my $target = $clean ? "/t:Clean,Build" :"/t:Build";
	my $properties = "/p:Configuration=$config;Platform=$arch;MONO_TARGET_GC=$gc";

	print ">>> $msbuild $properties $target $sln\n\n";
	system($msbuild, $properties, $target, $sln) eq 0
			or die("MSBuild failed to build $sln\n");
}
