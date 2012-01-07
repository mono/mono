use Cwd;
use Getopt::Long;

system("source","~/.profile");
print "My Path: $ENV{PATH}\n";

my $root = getcwd();
my $teamcity = 0;

if ($ENV{UNITY_THISISABUILDMACHINE}) {
	$teamcity = 1;
}

#do build
chdir("$root/mono/tests") eq 1 or die("failed to chdir tests");
if ($teamcity) {
	print("##teamcity[testSuiteStarted name='mono runtime tests']\n");
}
my $result = 0;
if($^O eq 'MSWin32') {
	$result = system("msbuild build.proj /t:Test");
} else {
	$result = system("make test");
}
if ($teamcity) {
	print("##teamcity[testSuiteFinished name='mono runtime tests']\n");
}
$result eq 0 or die ("Failed running mono runtime tests");
