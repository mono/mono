#!/usr/bin/perl -w
use Cwd;
use File::Basename;
use File::Find;
use Getopt::Long;


my $Compiler = "mbas";
my $Runtime;
my $CompilerFlags = "";

my $Execute = 1;
my $CompileCmd;
my $RunCmd;
my $RetVal=-1;

my $VBFile="";
my $VBExeFile;
my $VBLogFile;
my $TestResultsFile = "TestResults.log";
my $ExpectedResult="SUCCESS";
my @ActualResults = ();


my $VerboseMode=1;
my $FilePattern = "*.vb"; 
my $PrintHelp;




sub ParseTestFile
{
    my $testAnnotation;

    my $lineNo;
    my $fileName;
    my $expectedError;

    my $target = "";
    my $compilerOptions = "";

    my $cmdLine;

    open(VB_FILE, $VBFile);
    while(<VB_FILE>) 
    {
	next unless length;

	if(/^\s*REM(.*)/) {       
	    $testAnnotation = $1;
	} 
	else {
	    last;
	}

	if($testAnnotation =~ /\s*LineNo\s*:\s*(\d+)/) {
	    if(!defined $lineNo) {
		$lineNo = $1;
	    }
	    else {
		last;
	    }
	}
	elsif($testAnnotation =~ /\s*ExpectedError\s*:\s*(BC\d+)/) {
	    $expectedError = $1;
	}
	elsif($testAnnotation =~ /\s*Target\s*:\s*(.*)/) {
	    $target = $1;

	    if($target =~ /library/ || $target =~ /module/) {
		$Execute = 0;
	    }
	    else {
		$Execute = 1;
	    }

	    if($target ne "") {
		$target = "/target:" . $target;
	    }
	}
	elsif($testAnnotation =~ /\s*CompilerOptions\s*:\s*(.*)/) {
	    $compilerOptions = $1;
	}
    }
    close(VB_FILE);

    $cmdLine = $Compiler . " " . $CompilerFlags . " " . $target . " " . $compilerOptions . " " . $VBFile;

    if(defined $expectedError)
    {
	$ExpectedResult = {
	    FILE => $VBFile,
	    LINENO => $lineNo,
	    ERRORNO => $expectedError,
	};
    }
    else 
    {
	$ExpectedResult = "SUCCESS";
    }

    return $cmdLine;
}

sub Command
{
    my $retVal;
    my $cmdLine = shift(@_);

    open SAVEOUT, ">&STDOUT";
    open SAVEERR, ">&STDERR";

    print SAVEOUT "";
    print SAVEERR "";

    open STDOUT, ">>$VBLogFile";
    open STDERR, ">&STDOUT";

    print "\n";
    print $cmdLine . "\n";
    system($cmdLine);

    $retVal = $?;

    close STDOUT;
    close STDERR;

    open STDOUT, ">&SAVEOUT";
    open STDERR, ">&SAVEERR";

    return $retVal;
}

sub ExtractResults
{
    @ActualResults = ();

    open(VBLOGFILE, $VBLogFile);
    while(<VBLOGFILE>)
    {
	if($_ =~ /\s*((.*)\((\d+).*\)\s*)?:?\s*(error|warning)\s*(BC\d+)\s*:\s*(.*)/)
	{
	    my $actualResult = {
		FILE => $2,
		LINENO => $3,
		ERRORNO => $5,
	    };
	    push @ActualResults, $actualResult;
	}
    }
    close(VBLOGFILE);
}

sub ValidateResults
{
    my $retval = 0;
    my @matching = ();

    if($ExpectedResult eq  "SUCCESS")
    {
	if(!@ActualResults) {
	    return 0;
	}
	else {
	    return -1;
	}
    }

    @matching = grep {$_->{ERRORNO} eq $ExpectedResult->{ERRORNO}} @ActualResults;

    if(@matching) {
	return 0;
    }
    else {
	return -1;
    }
}

sub LogResults
{
    my($retVal, $msg) = @_;

    LogMessage("========================");
    if($retVal == 0) {
	LogMessage($VBFile . ": OK");
    } 
    else {
	LogMessage($VBFile . ": FAILED " . $msg);
    }
}



sub LogMessage
{
    my $msg = shift(@_);

    if($VerboseMode) {
	print $msg . "\n";
    }

    print TEST_RESULTS_FILE $msg . "\n";
}

sub PrintUsage() {
    print "\nUsage: test-mbas.pl [options]";
    print "\nTypical Usage: test-mbas.pl --p=Accessibility*.vb --res=test-results.log";
    print "\n\noptions:";
    print "\n\t--help\t\tprints this help";
    print "\n\t--verbose\tprints the results on the screen";
    print "\n\t--compiler\tspecify mbas or vbc";
    print "\n\t--compilerflags\tuse this to pass additional flags to the compiler";
    print "\n\t--runtime\tspecify mono or dotnet";
    print "\n\t--results\tname of the logfile";
    print "\n\t--pattern\tshell pattern for test case files\n\n";
}

# Process the command line

$result = GetOptions( "verbose!"=>\$VerboseMode,
		      "help"=>\$PrintHelp,
            "compiler:s"=>\$Compiler,
	    "compilerflags:s"=>\$CompilerFlags,
	    "runtime:s"=>\$Runtime,
	    "results:s"=>\$TestResultsFile,
	    "pattern:s"=>\$FilePattern);

if(!$result || $PrintHelp) {
    PrintUsage();
    exit 1;
}

# Build the list of tests to run

open(TEST_RESULTS_FILE, ">$TestResultsFile");
while(defined ($vbFile = glob($FilePattern))) {
    $VBFile = $vbFile;
    $VBLogFile = $VBFile . ".log";

    $CompileCmd = ParseTestFile();


    $RetVal = Command($CompileCmd);
    if($ExpectedResult eq "SUCCESS") {
	if($RetVal != 0) {
	    LogResults($RetVal, "COMPILATION");
	    next;
	}
	else {
	    if($Execute == 1) {
		$VBExeFile = $VBFile;
		$VBExeFile =~ s/\.vb$/\.exe/;
		$RunCmd = "$Runtime ./$VBExeFile";
		$RetVal = Command($RunCmd);
		LogResults($RetVal, "EXECUTION");
	    } 
	    else {
		LogResults($RetVal, "");
	    }
	}
    }
    else {
	ExtractResults();
	$RetVal = ValidateResults();
	LogResults($RetVal, "");
    }

    if($RetVal == 0) {
	unlink($VBLogFile);
    }
}
close TEST_RESULTS_FILE;



