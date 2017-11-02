use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;

my $currentdir = getcwd();

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildscriptsdir = "$monoroot/external/buildscripts";

my @passAlongArgs = ();
foreach my $arg (@ARGV)
{
	# Filter out --clean if someone uses it.  We have to clean since we are doing two builds
	if (not $arg =~ /^--clean=/)
	{
		push @passAlongArgs, $arg;
	}
}

print(">>> Building i386\n");
system("perl", "$buildscriptsdir/build.pl", "--arch32=1", "--clean=1", "--classlibtests=0", @passAlongArgs) eq 0 or die ('failing building i386');

print(">>> Building x86_64\n");
system("perl", "$buildscriptsdir/build.pl", "--clean=1", "--classlibtests=0", @passAlongArgs) eq 0 or die ('failing building x86_64');