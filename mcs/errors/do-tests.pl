#!/usr/bin/perl -w

unless ($#ARGV == 2) {
    print STDERR "Usage: $0 profile compiler glob-pattern\n";
    exit 1;
}

#
# Expected value constants
#
my $EXPECTING_WRONG_ERROR = 1;
my $EXPECTING_NO_ERROR    = 2;
my %expecting_map = ();
my %ignore_map = ();

my $profile = $ARGV [0];
my $compile = $ARGV [1];
my $files   = $ARGV [2];

if (open (EXPECT_WRONG, "<$profile-expect-wrong-error")) {
	$expecting_map{$_} = $EXPECTING_WRONG_ERROR 
	foreach map {
		chomp,                     # remove trailing \n
		s/\#.*//g,                 # remove # style comments
		s/\s//g;                    # remove whitespace
		$_ eq "" ? () : $_;        # now copy over non empty stuff
	} <EXPECT_WRONG>;
	
	close EXPECT_WRONG;
}

if (open (EXPECT_NO, "<$profile-expect-no-error")) {
	$expecting_map{$_} = $EXPECTING_NO_ERROR 
	foreach map {
		chomp,                     # remove trailing \n
		s/\#.*//g,                 # remove # style comments
		s/\s//g;                    # remove whitespace
		$_ eq "" ? () : $_;        # now copy over non empty stuff
	} <EXPECT_NO>;
        
	close EXPECT_NO;
}

if (open (IGNORE, "<$profile-ignore-tests")) {
	$ignore_map{$_} = 1
	foreach map {
		chomp,                     # remove trailing \n
		s/\#.*//g,                 # remove # style comments
		s/\s//g;                    # remove whitespace
		$_ eq "" ? () : $_;        # now copy over non empty stuff
	} <IGNORE>;
	
	close IGNORE;
}

my $RESULT_UNEXPECTED_CORRECT_ERROR     = 1;
my $RESULT_CORRECT_ERROR                = 2;
my $RESULT_UNEXPECTED_INCORRECT_ERROR   = 3;
my $RESULT_EXPECTED_INCORRECT_ERROR     = 4;
my $RESULT_UNEXPECTED_NO_ERROR          = 5;
my $RESULT_EXPECTED_NO_ERROR            = 6;
my $RESULT_UNEXPECTED_CRASH		= 7;

my @statuses = (
	"UNEXPECTED SUCCESS",
	"SUCCESS",
	"UNEXPECTED INCORRECT ERROR",
	"INCORRECT ERROR",
	"UNEXPECTED NO ERROR",
	"NO ERROR",
	"UNEXPECTED CRASH"
);

my @status_items = (
	[],
	[],
	[],
	[],
	[],
	[],
	[],
);

my %results_map = ();
my $total = 0;

foreach (glob ($files)) {
        print "$_";
	my ($error_number) = (/[a-z]*(\d+)(-\d+)?\.cs/);
	my $options = `sed -n 's,^// Compiler options:,,p' $_`;
	chomp $options;
	print "...";

	if (exists $ignore_map {$_}) {
	    print "IGNORED\n";
	    next;
	}

        $total++;
	my $testlogfile="$profile-$_.log";
	system "$compile --expect-error $error_number $options -out:$profile-$_.junk $_ > $testlogfile 2>&1";
	
	exit 1 if $? & 127;
	
	my $exit_value = $? >> 8;

	my $status;
	
	if ($exit_value > 2) {
		if (exists $expecting_map {$_} and $expecting_map {$_} == $EXPECTING_WRONG_ERROR) {
			$status = $RESULT_EXPECTED_INCORRECT_ERROR;
		} else {
			$status = $RESULT_UNEXPECTED_CRASH;
		}
	}
        
	if ($exit_value == 0) {
                system "rm -f $testlogfile";
		$status = $RESULT_UNEXPECTED_CORRECT_ERROR     if     exists $expecting_map {$_};
		$status = $RESULT_CORRECT_ERROR                unless exists $expecting_map {$_};
	}
	
	if ($exit_value == 1) {
		$status = $RESULT_UNEXPECTED_INCORRECT_ERROR   unless exists $expecting_map {$_} and $expecting_map {$_} == $EXPECTING_WRONG_ERROR;
		$status = $RESULT_EXPECTED_INCORRECT_ERROR     if     exists $expecting_map {$_} and $expecting_map {$_} == $EXPECTING_WRONG_ERROR;
	}
	
	if ($exit_value == 2) {
		$status = $RESULT_UNEXPECTED_NO_ERROR          unless exists $expecting_map {$_} and $expecting_map {$_} == $EXPECTING_NO_ERROR;
		$status = $RESULT_EXPECTED_NO_ERROR            if     exists $expecting_map {$_} and $expecting_map {$_} == $EXPECTING_NO_ERROR;
	}
	

	push @{@status_items [($status - 1)]}, $_;
	print "@statuses[($status - 1)]\n";
	$results_map{$_} = $status;
}
print "\n";
my $correct = scalar @{@status_items [($RESULT_CORRECT_ERROR              - 1)]};
print $correct, " correctly detected errors (", sprintf("%.2f",($correct / $total) * 100), " %) \n\n";

if (scalar @{@status_items [($RESULT_UNEXPECTED_CRASH - 1)]} > 0) {
    print scalar @{@status_items [($RESULT_UNEXPECTED_CRASH - 1)]}, " compiler crashes\n";
    print, print "\n" foreach @{@status_items [($RESULT_UNEXPECTED_CRASH - 1)]};
}

if (scalar @{@status_items [($RESULT_UNEXPECTED_CORRECT_ERROR - 1)]} > 0) {
    print scalar @{@status_items [($RESULT_UNEXPECTED_CORRECT_ERROR   - 1)]}, " fixed error report(s), remove it from expect-wrong-error or expect-no-error !\n";
    print, print "\n" foreach @{@status_items [($RESULT_UNEXPECTED_CORRECT_ERROR - 1)]};
}

if (scalar @{@status_items [($RESULT_UNEXPECTED_INCORRECT_ERROR - 1)]} > 0) {
    print scalar @{@status_items [($RESULT_UNEXPECTED_INCORRECT_ERROR - 1)]}, " new incorrect error report(s) !\n";
    print, print "\n" foreach @{@status_items [($RESULT_UNEXPECTED_INCORRECT_ERROR - 1)]};
}

if (scalar @{@status_items [($RESULT_UNEXPECTED_NO_ERROR - 1)]} > 0) {
    print scalar @{@status_items [($RESULT_UNEXPECTED_NO_ERROR        - 1)]}, " new missing error report(s) !\n";
    print, print "\n" foreach @{@status_items [($RESULT_UNEXPECTED_NO_ERROR - 1)]};
}

exit (
	scalar @{@status_items [($RESULT_UNEXPECTED_INCORRECT_ERROR - 1)]} +
	scalar @{@status_items [($RESULT_UNEXPECTED_NO_ERROR        - 1)]} +
	scalar @{@status_items [($RESULT_UNEXPECTED_CRASH           - 1)]}
) == 0 ? 0 : 1;
