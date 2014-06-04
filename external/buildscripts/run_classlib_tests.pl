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

@testdirs = ('corlib','System', 'System.Xml', 'System.Core');

foreach (@testdirs)
{
	chdir("$root/mcs/class/" . $_) eq 1 or die("failed to chdir " . $_);

	my $result = 0;
	if($^O eq 'MSWin32') {
		$result = system("msbuild build.proj /t:Test");
	} else {
		$result = system("make run-test-local");
	}

	if ($teamcity) {
		print("##teamcity[importData type='nunit' path='mcs/class/". $_ . "/TestResult-net_2_0.xml']\n");
	}
}
#$result eq 0 or die ("Failed running mono classlib tests");

