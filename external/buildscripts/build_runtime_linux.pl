use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildScriptsRoot = "$monoroot/external/buildscripts";

my $build64 = 0;

GetOptions(
   "build64=i"=>\$build64,
) or die ("illegal cmdline options");

my $arch32 = 1;
if ($build64)
{
	$arch32 = 0;
}

system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=1", "--test=1", "--artifact=1", "--arch32=$arch32", "--classlibtests=0", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono\n");
