#se lib ('.', "perl_lib");
use Cwd;
#use File::Path;
#use File::Copy::Recursive qw(dircopy);
use Getopt::Long;
#use File::Basename;

system("source","~/.profile");
print "My Path: $ENV{PATH}\n";

my $root = getcwd();
my $teamcity = 0;

if ($ENV{UNITY_THISISABUILDMACHINE}) {
	$teamcity = 1;
	print "Hello build machine\n"
} else {
	print "Hello user machine\n"
}

#do build
chdir("$root/mono/tests") eq 1 or die("failed to chdir tests");
if ($teamcity) {
	print("##teamcity[testSuiteStarted name='mono runtime tests']\n");
}
my $result = system("make test");
if ($teamcity) {
	print("##teamcity[testSuiteFinished name='mono runtime tests']\n");
}
$result eq 0 or die ("Failed running mono runtime tests");
