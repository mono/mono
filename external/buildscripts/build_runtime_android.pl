use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildScriptsRoot = "$monoroot/external/buildscripts";

my $androidArch = "";
my $clean = 1;

GetOptions(
   "androidarch=s"=>\$androidArch,
   "clean=i"=>\$clean,
) or die ("illegal cmdline options");

# By default, build runtime for all the variants we need.  But allow something to specify an individual variation to build
if ($androidArch eq "")
{
	system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=1", "--artifact=1", "--arch32=1", "--androidarch=armv5", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono for armv5\n");
	system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=1", "--artifact=1", "--arch32=1", "--androidarch=armv6_vfp", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono for armv6_vfp\n");
	system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=1", "--artifact=1", "--arch32=1", "--androidarch=armv7a", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono for armv7a\n");
	system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=1", "--artifact=1", "--arch32=1", "--androidarch=x86", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono for x86\n");
}
else
{
	system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=$clean", "--artifact=1", "--arch32=1", "--androidarch=$androidArch", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono for $androidArch\n");
}
