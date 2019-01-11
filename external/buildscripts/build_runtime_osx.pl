use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildScriptsRoot = "$monoroot/external/buildscripts";
my $stevedoreBuildDeps=1;

GetOptions(
   'stevedorebuilddeps=i'=>\$stevedoreBuildDeps,
) or die ("illegal cmdline options");

system("perl", "$buildScriptsRoot/build_all_osx.pl", "--build=1", "--artifact=1", "--test=1", "--forcedefaultbuilddeps=1", "--stevedorebuilddeps=$stevedoreBuildDeps") eq 0 or die ("Failed building mono\n");
