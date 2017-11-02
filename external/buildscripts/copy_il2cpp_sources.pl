#
# Use this script to copy the Mono file used by IL2CPP.
# perl external\buildscripts\copy_il2cpp_sources.pl --monoroot=<Mono root clone> --sourcesroot=<Unity root clone>\External\MonoBleedingEdge\builds\source
#

use File::Basename;
use File::Copy;
use File::Path;
use Getopt::Long;

my $monoroot ="";
my $sourcesroot = "";

GetOptions(
    'monoroot=s'=>\$monoroot,
    'sourcesroot=s'=>\$sourcesroot,
) or die ("illegal cmdline options");

my $sourcesFile = "$monoroot/external/buildscripts/sources.txt";
open(SOURCE_FILE, $sourcesFile) or die "failed opening $sourcesFile\n";
my @listOfSourceFilesLines = <SOURCE_FILE>;
close(SOURCE_FILE);
chomp(@listOfSourceFilesLines);

my $isPrivateFile = 0;
foreach my $sourcesLine(@listOfSourceFilesLines)
{
    if($sourcesLine =~ /#.*/)
    {
        next;
    }
    elsif($sourcesLine =~ /SOURCES:/ or $sourcesLine =~ /HEADERS:/ or $sourcesLine =~ /METADATA:/)
    {
        $isPrivateFile = 0;
        next;
    }
    elsif($sourcesLine =~ /PRIVATE:/)
    {
        $isPrivateFile = 1;
        next;
    }

    $fileToCopy = "$monoroot/$sourcesLine";
    $destFile = "$sourcesroot/$sourcesLine";
    if($isPrivateFile)
    {
        $destFile =~ s/(.*)\/(.*\.c)/$1\/private\/$2/g;
    }

    $destDir = dirname("$destFile");
    if (!-d $destDir)
    {
        mkpath($destDir) or die "failed making directory $destDir\n";
    }

    if (-e $fileToCopy)
    {
        copy($fileToCopy, $destFile) or die "failed to copy $fileToCopy to $destFile\n";
    }
}
