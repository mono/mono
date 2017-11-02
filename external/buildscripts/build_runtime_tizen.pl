use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildScriptsRoot = "$monoroot/external/buildscripts";

my $clean = 1;

GetOptions(
   "clean=i"=>\$clean,
) or die ("illegal cmdline options");

system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=1", "--artifact=1", "--arch32=1", "--tizen=1", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono for tizen\n");
system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=1", "--artifact=1", "--arch32=1", "--tizen=1", "--tizenemulator=1", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono for tizen emulator\n");
