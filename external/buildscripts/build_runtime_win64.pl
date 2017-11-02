use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildScriptsRoot = "$monoroot/external/buildscripts";

# Note : Ideally we can switch back to this build approach once the random cygwin hangs on the build machines are sorted out
#system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=1", "--test=1", "--artifact=1", "--classlibtests=0", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono\n");

system("perl", "$buildScriptsRoot/build_win_no_cygwin.pl", "--build=1", "--clean=1", "--artifact=1", "--arch32=0", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono\n");
