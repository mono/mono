use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildScriptsRoot = "$monoroot/external/buildscripts";

my $clean = 1;
my $runtime = 0;
my $xcomp = 0;
my $simulator = 0;

GetOptions(
   "clean=i"=>\$clean,
   "runtime=i"=>\$runtime,
   "xcomp=i"=>\$xcomp,
   "simulator=i"=>\$simulator,
) or die ("illegal cmdline options");

# Build everything by default
if ($runtime == 0 && $xcomp == 0 && $simulator == 0)
{
	print ">>> All iphone related builds will be ran\n";
	$runtime = 1;
	$xcomp = 1;
	$simulator = 1;
}

if ($runtime)
{
	print ">>> Building iphone runtime\n";
	system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=$clean", "--artifact=1", "--arch32=1", "--iphoneArch=armv7", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono for iphone\n");
}

if ($xcomp)
{
	# TODO : This is a horrible waste of time, and we should fix it, but for now it gets things working.
	# The mono we have in the build deps for bootstrapping doesn't have a 32bit slice, which we need in order to run the MonoAotOffsetsDumper.
	# To get around this for the moment, we'll build the runtime & classlibs first, and then use that to run the MonoAotOffsetsDumper.
	# Once we update the mono in the build deps, we can remove this.
	if(!(-f "$monoroot/builds/monodistribution/bin/mono"))
	{
		print ">>> Building mono to use for bootstrapping.  The version in mono build deps is missing the 32bit slice and we need a 32bit version to run the MonoAotOffsetsDumper\n";
		#system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--arch32=1", "--clean=$clean", "--classlibtests=0", "--artifact=1", "--artifactscommon=1", "--forcedefaultbuilddeps=1") eq 0 or die ("failing building mono 32bit for bootstrapping\n");
		#system("perl", "$buildScriptsRoot/build_all_osx.pl", "--build=1", "--artifact=1", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono\n");
		system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=1", "--artifact=1", "--arch32=1", "--artifactscommon=1", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building mono\n");

		system("cp", "$monoroot/builds/monodistribution/bin-osx-tmp-i386/mono", "$monoroot/builds/monodistribution/bin/.") eq 0 or die ("failed to copy mono over from bin-osx-tmp-i386 to bin\n");
		system("chmod", "+x", "$monoroot/builds/monodistribution/bin/mono") eq 0 or die("Failed to chmod mono\n");

		# Need to clean up the tmp build folder so that we don't pollute the final artifact
		rmtree("$monoroot/builds/monodistribution/bin-osx-tmp-i386");
		rmtree("$monoroot/builds/embedruntimes/osx-tmp-i386");
	}

	print ">>> Building iphone cross compiler\n";
	system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=$clean", "--artifact=1", "--arch32=1", "--iphonecross=1", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building iphone cross compiler\n");
}

if ($simulator)
{
	print ">>> Building iphone simulator\n";
	system("perl", "$buildScriptsRoot/build.pl", "--build=1", "--clean=$clean", "--artifact=1", "--arch32=1", "--iphonesimulator=1", "--forcedefaultbuilddeps=1") eq 0 or die ("Failed building iphone simulator\n");
}
