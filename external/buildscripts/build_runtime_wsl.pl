use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildScriptsRoot = "$monoroot/external/buildscripts";

#Windows Subsystem for Linux currently does not support 32-bit, so the option is 64-bit only

system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=1", "--test=1", "--artifact=1", "--arch32=0", "--classlibtests=0", "--forcedefaultbuilddeps=1", "--windowssubsystemforlinux=1") eq 0 or die ("Failed building mono\n");
