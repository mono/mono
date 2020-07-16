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

my @thisScriptArgs = ();
my @passAlongArgs = ();
foreach my $arg (@ARGV)
{
	# Filter out --clean if someone uses it.  We have to clean since we are doing two builds
	if (not $arg =~ /^--clean=/)
	{
		# We don't need common artifacts from both, so filter out temporarily and we'll
		# only pass it to the second build
		if ($arg =~ /^--artifactscommon=/)
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
);

print(">>> Building x86_64\n");
system("perl", "$buildscriptsdir/build.pl", "--clean=1", "--classlibtests=0", "--targetarch=x86_64", @passAlongArgs) eq 0 or die ('failing building x86_64');

#print(">>> Building ARM64\n");
#system("perl", "$buildscriptsdir/build.pl", "--clean=1", "--classlibtests=0", "--targetarch=arm64", @passAlongArgs) eq 0 or die ("failing building ARM64");

if ($artifact)
{
	print(">>> Moving built binaries to final output directories\n");

	# Copy stuff in the embedruntimes directory
	my $embedDirRoot = "$buildsroot/embedruntimes";
	my $embedDirSourceX64 = "$embedDirRoot/osx-tmp-x86_64";
	#my $embedDirSourceARM64 = "$embedDirRoot/osx-tmp-arm64";

	CopyEmbedRuntimeBinaries($embedDirSourceX64, "$embedDirRoot/osx");
	#CopyEmbedRuntimeBinaries($embedDirSourceARM64, "$embedDirRoot/osx-arm64");

	# Merge stuff in the monodistribution directory
	my $distDirRoot = "$buildsroot/monodistribution";
	my $distDirDestinationBin = "$buildsroot/monodistribution/bin";
	my $distDirDestinationLib = "$buildsroot/monodistribution/lib";
	my $distDirSourceBinX64 = "$distDirRoot/bin-osx-tmp-x86_64";
	#my $distDirSourceBinARM64 = "$distDirRoot/bin-osx-tmp-arm64";

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

	if (!(-d $distDirSourceBinX64))
	{
		die("Expected source directory not found : $distDirSourceBinX64\n");
	}

	#if (!(-d $distDirSourceBinARM64))
	#{
		#die("Expected source directory not found : $distDirSourceBinARM64\n");
	#}

	for my $file ('mono')
	{
		#MergeIntoFatBinary("$distDirSourceBinX64/$file", "$distDirSourceBinARM64/$file", "$distDirDestinationBin/$file");
		system ('mv', "$distDirSourceBinX64/$file", "$distDirDestinationBin/$file");
	}

	for my $file ('pedump')
	{
		# pedump doens't get cross-compiled
		system ('mv', "$distDirSourceBinX64/$file", "$distDirDestinationBin/$file") eq 0 or die ("Failed to move '$distDirSourceBinX64/$file' to '$distDirDestinationBin/$file'.");
	}

	for my $file ('libMonoPosixHelper.dylib', 'libmono-native.dylib')
	{
		#MergeIntoFatBinary("$embedDirSourceX64/$file", "$embedDirSourceARM64/$file", "$distDirDestinationLib/$file");
		print(">>> cp $embedDirSourceX64/$file $distDirDestinationLib/$file\n\n");
		system ('cp', "$embedDirSourceX64/$file", "$distDirDestinationLib/$file");
	}

	if ($buildMachine)
	{
		print(">>> Clean up temporary arch specific build directories\n");

		rmtree("$distDirSourceBinX64");
		#rmtree("$distDirSourceBinARM64");
		rmtree("$embedDirSourceX64");
		#rmtree("$embedDirSourceARM64");
	}
}

sub CopyEmbedRuntimeBinaries
{
	my ($embedDirSource, $embedDirDestination) = @_;

	system("mkdir -p $embedDirDestination");

	if (!(-d $embedDirSource))
	{
		die("Expected source directory not found : $embedDirSource\n");
	}

	for my $file ('libmonobdwgc-2.0.dylib','libmonosgen-2.0.dylib','libMonoPosixHelper.dylib', 'libmono-native.dylib')
	{
		print(">>> cp $embedDirSource/$file $embedDirDestination/$file\n\n");
		system('cp', "$embedDirSource/$file", "$embedDirDestination/$file") eq 0 or die("Failed to copy '$embedDirSource/$file' to '$embedDirDestination/$file'.");
	}

	if (not $buildMachine)
	{
		print(">>> Doing non-build machine stuff...\n");
		for my $file ('libmonobdwgc-2.0.dylib','libmonosgen-2.0.dylib','libMonoPosixHelper.dylib', 'libmono-native.dylib')
		{
			print(">>> Removing $embedDirDestination/$file.dSYM\n");
			rmtree("$embedDirDestination/$file.dSYM");
			print(">>> 'dsymutil $embedDirDestination/$file\n");
			system('dsymutil', "$embedDirDestination/$file") eq 0 or warn("Failed creating $embedDirDestination/$file.dSYM");
		}

		print(">>> Done with non-build machine stuff\n");
	}
}

sub MergeIntoFatBinary
{
	my ($binary1, $binary2, $binaryOutput) = @_;

	print(">>> Merging '$binary1' and '$binary2' into '$binaryOutput'\n\n");
	system('lipo', "$binary1", "$binary2", "-create", "-output", "$binaryOutput") eq 0 or die("Failed to run lipo!");
}