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
	print(">>> Copying libMonoPosixHelper.dylib to lib directory\n");

	my $libDir = "$buildsroot/monodistribution/lib";
	if (!(-d $libDir))
	{
		system("mkdir -p $libDir");
	}

	system("cp","$buildsroot/embedruntimes/osx/libMonoPosixHelper.dylib", "$libDir/")
}
