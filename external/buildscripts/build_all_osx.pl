use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;

my $currentdir = getcwd();

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildscriptsdir = "$monoroot/external/buildscripts";
my $buildMachine = $ENV{UNITY_THISISABUILDMACHINE};
my $buildsroot = "$monoroot/builds";

my $artifact=0;
my $artifactsCommon=0;
my $buildUsAndBoo=0;

my @thisScriptArgs = ();
my @passAlongArgs = ();
foreach my $arg (@ARGV)
{
	# Filter out --clean if someone uses it.  We have to clean since we are doing two builds
	if (not $arg =~ /^--clean=/)
	{
		# We don't need common artifacts, us, and boo, from both, so filter out temporarily and we'll
		# only pass it to the second build
		if ($arg =~ /^--artifactscommon=/ || $arg =~ /^--buildusandboo=/)
		{
			push @thisScriptArgs, $arg;
		}
		else
		{
			push @passAlongArgs, $arg;
		}
	}

	if ($arg =~ /^--artifact=/)
	{
		push @thisScriptArgs, $arg;
	}
}

print(">>> This Script Args = @thisScriptArgs\n");
print(">>> Pass Along Args = @passAlongArgs\n");

@ARGV = @thisScriptArgs;
GetOptions(
	'artifact=i'=>\$artifact,
	'artifactscommon=i'=>\$artifactsCommon,
	'buildusandboo=i'=>\$buildUsAndBoo,
);

my $monoArch32Target = "i386";

print(">>> Building $monoArch32Target\n");
system("perl", "$buildscriptsdir/build.pl", "--arch32=1", "--clean=1", "--classlibtests=0", @passAlongArgs) eq 0 or die ("failing building $monoArch32Target");

if ($artifactsCommon)
{
	push @passAlongArgs, "--artifactscommon=1";
}

if ($buildUsAndBoo)
{
	push @passAlongArgs, "--buildusandboo=1";
}

print(">>> Building x86_64\n");
system("perl", "$buildscriptsdir/build.pl", "--clean=1", "--classlibtests=0", @passAlongArgs) eq 0 or die ('failing building x86_64');

if ($artifact)
{
	print(">>> Creating universal binaries\n");
	# Merge stuff in the embedruntimes directory
	my $embedDirRoot = "$buildsroot/embedruntimes";
	my $embedDirDestination = "$embedDirRoot/osx";
	my $embedDirSource32 = "$embedDirRoot/osx-tmp-$monoArch32Target";
	my $embedDirSource64 = "$embedDirRoot/osx-tmp-x86_64";

	system("mkdir -p $embedDirDestination");

	if (!(-d $embedDirSource32))
	{
		die("Expected source directory not found : $embedDirSource32\n");
	}

	if (!(-d $embedDirSource64))
	{
		die("Expected source directory not found : $embedDirSource64\n");
	}

	# Create universal binaries
	for my $file ('libmonobdwgc-2.0.dylib','libmonosgen-2.0.dylib','libMonoPosixHelper.dylib')
	{
		print(">>> lipo $embedDirSource32/$file $embedDirSource64/$file -create -output $embedDirDestination/$file\n\n");
		system ('lipo', "$embedDirSource32/$file", "$embedDirSource64/$file", '-create', '-output', "$embedDirDestination/$file");
	}

	if (not $buildMachine)
	{
		print(">>> Doing non-build machine stuff...\n");
		for my $file ('libmonobdwgc-2.0.dylib','libmonosgen-2.0.dylib','libMonoPosixHelper.dylib')
		{
			print(">>> Removing $embedDirDestination/$file.dSYM\n");
			rmtree ("$embedDirDestination/$file.dSYM");
			print(">>> 'dsymutil $embedDirDestination/$file\n");
			system ('dsymutil', "$embedDirDestination/$file") eq 0 or warn ("Failed creating $embedDirDestination/$file.dSYM");
		}

		print(">>> Done with non-build machine stuff\n");
	}

	# Merge stuff in the monodistribution directory
	my $distDirRoot = "$buildsroot/monodistribution";
	my $distDirDestinationBin = "$buildsroot/monodistribution/bin";
	my $distDirDestinationLib = "$buildsroot/monodistribution/lib";
	my $distDirSourceBin32 = "$distDirRoot/bin-osx-tmp-$monoArch32Target";
	my $distDirSourceBin64 = "$distDirRoot/bin-osx-tmp-x86_64";

	# Should always exist because build_all would have put stuff in it, but in some situations
	# depending on the options it may not.  So create it if it does not exist
	if (!(-d $distDirDestinationBin))
	{
		system("mkdir -p $distDirDestinationBin");
	}

	if (!(-d $distDirDestinationLib))
	{
		system("mkdir -p $distDirDestinationLib");
	}

	if (!(-d $distDirSourceBin32))
	{
		die("Expected source directory not found : $distDirSourceBin32\n");
	}

	if (!(-d $distDirSourceBin64))
	{
		die("Expected source directory not found : $distDirSourceBin64\n");
	}

	for my $file ('mono','pedump')
	{
		print(">>> lipo $distDirSourceBin32/$file $distDirSourceBin64/$file -create -output $distDirDestinationBin/$file\n\n");
		system ('lipo', "$distDirSourceBin32/$file", "$distDirSourceBin64/$file", '-create', '-output', "$distDirDestinationBin/$file");
	}

	#Create universal binaries for stuff is in the embed dir but will end up in the dist dir
	for my $file ('libMonoPosixHelper.dylib')
	{
		print(">>> lipo $embedDirSource32/$file $embedDirSource64/$file -create -output $distDirDestinationLib/$file\n\n");
		system ('lipo', "$embedDirSource32/$file", "$embedDirSource64/$file", '-create', '-output', "$distDirDestinationLib/$file");
	}

	if ($buildMachine)
	{
		print(">>> Clean up temporary arch specific build directories\n");

		rmtree("$distDirSourceBin32");
		rmtree("$distDirSourceBin64");
		rmtree("$embedDirSource32");
		rmtree("$embedDirSource64");
	}
}
