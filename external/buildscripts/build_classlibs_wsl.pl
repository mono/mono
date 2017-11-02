use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildScriptsRoot = "$monoroot/external/buildscripts";

my $build = 1;
my $clean = 1;
my $mcsOnly = 0;
my $skipMonoMake = 0;

# Handy troubleshooting/niche options

# The prefix hack probably isn't needed anymore.  Let's disable it by default and see how things go
my $shortPrefix = 1;

# This script should not be ran on windows, if it is, kindly switch over to wsl
if ($^O eq "MSWin32")
{
	print(">>> Called from Windows.  Switching over to wsl\n");
	my $monoRootInBash = `bash -c pwd`;
	chomp $monoRootInBash;
	print(">>> monoRootInBash = $monoRootInBash\n");
	my $cmdForBash = "$monoRootInBash/external/buildscripts/build_classlibs_wsl.pl @ARGV";
	system("bash", "-c", "\"perl $cmdForBash\"") eq 0 or die("\n");
	exit 0;
}

GetOptions(
   "build=i"=>\$build,
   "clean=i"=>\$clean,
   "mcsOnly=i"=>\$mcsOnly,
   'skipmonomake=i'=>\$skipMonoMake,
   'shortprefix=i'=>\$shortPrefix,
) or die ("illegal cmdline options");

system(
	"perl",
	"$buildScriptsRoot/build.pl",
	"--build=$build",
	"--clean=$clean",
	"--mcsonly=$mcsOnly",
	"--skipmonomake=$skipMonoMake",
	"--artifact=1",
	"--artifactscommon=1",
	"--buildusandboo=1",
	"--forcedefaultbuilddeps=1",
	"--windowssubsystemforlinux=1",
	"--shortprefix=$shortPrefix") eq 0 or die ("Failed building mono\n");
