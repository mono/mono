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
chdir("$root/mcs/class/corlib") eq 1 or die("failed to chdir corlib");

my $result = 0;
if($^O eq 'MSWin32') {
	$result = system("msbuild build.proj /t:Test");
} else {
	$result = system("make run-test-local");
}

if ($teamcity) {
	print("##teamcity[importData type='nunit' path='mcs/class/corlib/classTestResult-net_2_0.xml']\n");
}

$result eq 0 or die ("Failed running mono classlib tests");
