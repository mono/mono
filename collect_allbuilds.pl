use lib ('./perl_lib');
use File::Copy::Recursive qw(dircopy rmove);
use File::Path;
use Tools qw(InstallNameTool);

my $path = "incomingbuilds/";

rmtree("collectedbuilds");
mkpath("collectedbuilds");

my @folders = ();
opendir(DIR, $path) or die "cant find $path: $!";
while (defined(my $file = readdir(DIR))) {
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

#both the ppc and i386 runtime builds output their runtime in monodistribution/bin/osx/mono.  as we collect both, we want
#to merge them into one file using the lipo tool.
system("lipo -create incomingbuilds/osx-i386/monodistribution/bin/mono incomingbuilds/osx-ppc/monodistribution/bin/mono -output collectedbuilds/monodistribution/bin/mono") && die("failed running lipo on osx runtimes");
system("chmod +x collectedbuilds/monodistribution/bin/mono") eq 0 or die("Failed chmodding");

system("lipo -create incomingbuilds/osx-i386/embedruntimes/osx/MonoBundleBinary incomingbuilds/osx-ppc/embedruntimes/osx/MonoBundleBinary -output collectedbuilds/embedruntimes/osx/MonoBundleBinary") && die("failed running lipo on osx MonoBundleBinary");
system("lipo -create incomingbuilds/osx-i386/embedruntimes/osx/libmono.0.dylib incomingbuilds/osx-ppc/embedruntimes/osx/libmono.0.dylib -output collectedbuilds/embedruntimes/osx/libmono.0.dylib") && die("failed running lipo on libmono.0.dylib");
system("lipo -create incomingbuilds/osx-i386/embedruntimes/osx/libmono.a incomingbuilds/osx-ppc/embedruntimes/osx/libmono.a -output collectedbuilds/embedruntimes/osx/libmono.a") && die("failed running lipo on libmono.a");

InstallNameTool("collectedbuilds/embedruntimes/osx/libmono.0.dylib", "\@executable_path/../Frameworks/MonoEmbedRuntime/osx/libmono.0.dylib");

chdir("collectedbuilds");

rmove('versions-aggregated.txt', 'versions.txt');

open(MYFILE,">built_by_teamcity.txt");
print MYFILE "These builds were created by teamcity from svn revision $ENV{BUILD_VCS_NUMBER}\n";
print MYFILE "TC projectname was: $ENV{TEAMCITY_PROJECT_NAME}\n";
print MYFILE "TC buildconfigname was: $ENV{TEAMCITY_BUILDCONF_NAME}\n";
close(MYFILE);

system("zip -r builds.zip *") eq 0 or die("failed zipping up builds");
