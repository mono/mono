use File::Basename;
use File::Copy;
use File::Compare;

my $Compiler = "mbas";
my $VBFile = "PreProcessorDirective.vb";
my $LogFile = "Results.log";
my $ExpectedResult;
my @ActualResults = ();

# build the command line

ParseTestFile();
Compile();
ExtractResults();
$RetVal = ValidateResults();

if($RetVal == 0) {
    print "\n\nTEST PASSED";
}
else {
    print "\n\nTEST FAILED";
}




sub ParseTestFile
{
    my $lineNo;
    my $fileName;
    my $expectedError;

    my $target;
    my $compilerOptions;

    open(VB_TEST_FILE, $VBFile) or
	print $VBFile . "not found";

    while(<VB_TEST_FILE>) 
    {
	if($_ !~ /^\s*REM(.*)/) {
	    break;
	}

	if($1 =~ /\s*LineNo\s*:\s*(\d+)/) {
	    $lineNo = $1;
	}
	elsif($1 =~ /\s*ExpectedError\s*:\s*(BC\d+)/) {
	    $expectedError = $1;
	}
	elsif($1 =~ /\s*Target\s*:\s*(.*)/) {
	    if($1 ne "")
	    {
		$target = "/target:" . $1;
	    }
	}
	elsif($1 =~ /\s*CompilerOptions\s*:\s*(.*)/) {
	    $compilerOptions = $1;
	}
    }

    $CmdLine = $Compiler . " " . $target . " " . $compilerOptions . " " . $VBFile;
    print "\n\n$CmdLine";

    if(defined $expectedError)
    {
	print "Expected Error is defined";

	$ExpectedResult = {
	    FILE => $VBFile,
	    LINENO => $lineNo,
	    ERRORNO => $expectedError,
	};

	print "\n\nEXPECTING ";
	print "\n\tFile:\t\t" . $ExpectedResult->{FILE};
	print "\n\tLine:\t\t" . $ExpectedResult->{LINENO};
	print "\n\tErrNo:\t\t" . $ExpectedResult->{ERRORNO};
    }
}

sub Compile
{
    my $retVal;

    open SAVEOUT, ">&STDOUT";
    open SAVERR, ">&SAVEERR";

    open STDOUT, ">$LogFile";
    open STDERR, ">&STDOUT";

    select STDERR; $| = 1;
    select STDOUT; $| = 1;

    system($CmdLine);

    $retVal = $?;

    close STDOUT;
    close STDERR;

    open STDOUT, ">&SAVEOUT";
    open STDERR, ">&SAVERR";

    return $retval;
}

sub ExtractResults
{
    @ActualResults = ();

    open(LOGFILE, $LogFile);
    while(<LOGFILE>)
    {
	if($_ =~ /\s*((.*)\((\d+).*\)\s*)?:?\s*(error|warning)\s*(BC\d+)\s*:\s*(.*)/)
	{
	    my $actualResult = {
		FILE => $2,
		LINENO => $3,
		ERRORNO => $5,
	    };

	    print "\n\nACTUAL ";
	    print "\n\tFile:\t\t" . $actualResult->{FILE};
	    print "\n\tLine:\t\t" . $actualResult->{LINENO};
	    print "\n\tErrNo:\t\t" . $actualResult->{ERRORNO};

	    push @ActualResults, $actualResult;
	}
    }
    close(LOGFILE);
}

sub ValidateResults
{
    my $retval = 0;
    my @matching = ();

    if(!defined $ExpectedResult)
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





