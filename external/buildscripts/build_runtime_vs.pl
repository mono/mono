use strict;
use warnings;
sub CompileVCProj;
use Cwd 'abs_path';
use Getopt::Long;
use File::Spec;
use File::Basename;
use File::Copy;
use File::Path;

print ">>> PATH in Build VS = $ENV{PATH}\n\n";

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
$monoroot = abs_path($monoroot);
my $buildsroot = "$monoroot/builds";
my $buildMachine = $ENV{UNITY_THISISABUILDMACHINE};

my $build = 0;
my $clean = 0;
my $targetArch = "";
my $debug = 0;
my $gc = "bdwgc";

GetOptions(
	'build=i'=>\$build,
	'clean=i'=>\$clean,
	'targetarch=s'=>\$targetArch,
	'debug=i'=>\$debug,
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
	my $vsInstallRoot = $ENV{"ProgramFiles(x86)"} . "/Microsoft Visual Studio";

	my $msbuild = "$vsInstallRoot/2019/Professional/MSBuild/Current/Bin/MSBuild.exe";

	if (!(-e -x $msbuild))
	{
		print (">>> Unable to find executable MSBuild for vs19 at: $msbuild\nFalling back to vs17\n");
		$msbuild = "$vsInstallRoot/2017/Professional/MSBuild/15.0/Bin/MSBuild.exe";
	}

    $config = $debug ? "Debug" : "Release";
	my $arch = $targetArch;

	my $target = $clean ? "/t:Clean,Build" :"/t:Build";
	my $properties = "/p:Configuration=$config;Platform=$arch;MONO_TARGET_GC=$gc;MONO_USE_STATIC_C_RUNTIME=true";

	print (">>> $msbuild $properties $target $sln\n\n");
	system($msbuild, $properties, $target, $sln) eq 0
			or die("MSBuild failed to build $sln\n");
}
