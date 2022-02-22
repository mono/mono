use lib ('external/buildscripts/perl_lib');
use Cwd 'abs_path';
use File::Basename;
use File::Copy::Recursive qw(dircopy rmove);
use File::Path;
use Tools qw(InstallNameTool);


my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);

my $path = "incomingbuilds/";

if ($^O ne "linux" && $^O ne "darwin")
{
	die("Unsupported platform to create stevedore artifact.")
}

rmtree("collectedbuilds");
mkpath("collectedbuilds");

my @folders = ();
opendir(DIR, $path) or die "cant find $path: $!";
# Sort the directories alphabetically so that classlibs comes before the
# OSX universal runtime (in the osx-i386 directory). Both builds produce the same
# files in some cases (notably libMonoPosixHelper.dylib), and we need the 
# universal runtime build to be second, since it produces a universal binary
# and the classlibs build produces a 32-bit binary only.  
my @files = sort readdir(DIR);
while (defined(my $file = shift @files)) {

	next if $file =~ /^\.\.?$/;
	if (-d "$path$file"){
		if (-f "$path$file/versions.txt") {
			system("cat $path$file/versions.txt >> collectedbuilds/versions-aggregated.txt");
		}
		dircopy("$path$file","collectedbuilds/") or die ("failed copying $path$file");
		push @folders,"$path$file";
	}
}
closedir(DIR);

system("find collectedbuilds -type f -name mono -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");
system("find collectedbuilds -type f -name mono-sgen -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");
system("find collectedbuilds -type f -name pedump -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");
system("find collectedbuilds -type f -name cli -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");
system("find collectedbuilds -type f -name cli_x86 -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");
system("find collectedbuilds -type f -name ilasm -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");
system("find collectedbuilds -type f -name mcs -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");
system("find collectedbuilds -type f -name mono-env -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");
system("find collectedbuilds -type f -name monolinker -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");
system("find collectedbuilds -type f -name nunit-console -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");
system("find collectedbuilds -type f -name nunit-console2 -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");
system("find collectedbuilds -type f -name resgen2 -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");
system("find collectedbuilds -type f -name xbuild -exec chmod +x {} \\;") eq 0 or die("Failed chmodding");

chdir("collectedbuilds");

system("cp embedruntimes/linux64/libmono-btls-shared.so monodistribution/lib/libmono-btls-shared.so");
system("cp embedruntimes/linux64/libmono-native.so monodistribution/lib/libmono-native.so");
system("cp embedruntimes/linux64/libMonoPosixHelper.so monodistribution/lib/libMonoPosixHelper.so");

rmove('versions-aggregated.txt', 'versions.txt');

print(">>> Create stevedore artifact $monoroot/stevedore/MonoBleedingEdge.7z");

rmtree("../stevedore");
my $stevedoreMbePath = "../stevedore/MonoBleedingEdge";
my $stevedoreMbe7z = "../stevedore/MonoBleedingEdge.7z";
my $stevedoreMbeArtifactID = "../stevedore/artifactid.txt";

system("mkdir -p $stevedoreMbePath") eq 0 or die("failed to mkdir $stevedoreMbePath");
system("cp -r * $stevedoreMbePath/") eq 0 or die ("failed copying builds to $stevedoreMbePath\n");

# Use our 7za fork with zstd support, installed in the PATH somewhere.
# See .yamato/scripts/collate_builds.sh for more details.
system("7za a $stevedoreMbe7z -m0=zstd -mx22 $stevedoreMbePath/* -sdel") eq 0 or die("failed 7z up $stevedoreMbePath");
system("rm -rf $stevedoreMbePath") eq 0 or die("failed to delete $stevedoreMbePath");

# Write stevedore artifact ID to file
my $revision = `git rev-parse --short HEAD`;
system("mono ../external/buildscripts/bee.exe steve new $stevedoreMbe7z MonoBleedingEdge $revision") eq 0 or die("failed running bee");
open (my $file, '>', $stevedoreMbeArtifactID);
my $artifactID = `mono ../external/buildscripts/bee.exe steve new $stevedoreMbe7z MonoBleedingEdge $revision`;
print $file $artifactID; 
print (">>> MonoBleedingEdge stevedore artifact ID: $artifactID\n");

print(">>> Done creating stevedore artifact $monoroot/MonoBleedingEdge.7z");

system("zip -r builds.zip *") eq 0 or die("failed zipping up builds");

system("7za a builds.7z -m0=zstd -mx22 * -x!builds.zip") eq 0 or die("failed 7z up builds");

