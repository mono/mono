#!/usr/bin/perl -w

use strict;
use IPC::Open3;

sub remove ($$) {
    my ($val, $arr) = @_;
    my $had = 0;
    my @hasnt = ();
    
    for my $v (@$arr) {
	if ($v == $val) { $had = 1; }
	else { push @hasnt, $v; }
    }
    return ($had, \@hasnt);
}

# Setting $strict to 1 enables line number checks, setting it to 2 makes
# line number mismatches fatal.
my $strict = 2;
my $failures = 0;

if ($#ARGV != 1) {
    print STDERR "Usage: $0 testcase.cs testcase.out\n";
    exit 1;
}

my %errors = ();
my %warnings = ();
my %lines = ();

my $csfile = $ARGV [0];
my $mcsout = $ARGV [1];
my $input;

my $line = 0;

open (INPUT, "<$csfile") or die
    "Can't open testcase: $!";
while (defined ($input = <INPUT>)) {
    ++$line;
    chop $input;
    next unless $input =~ m,^\s*//\s*(error|warning)?\s*(CS\d+),;

    if ((defined $1) and ($1 eq 'warning')) {
	push @{$warnings{$2}}, $line+1;
    } else {
	push @{$errors{$2}}, $line+1;
    }

    $lines{$line+1} = $2;
}

close INPUT;

open (MCS, "$mcsout") or die
    "Can't open compiler output file: $!";

while (defined ($input = <MCS>)) {
    chop $input;
    next unless $input =~ m,\((\d+)\)\s+(warning|error)\s+(CS\d+):,;
    my $had;

    if (!defined $lines{$1}) {
	print "Unexpected $2 $3 on line $1.\n";
	$failures++ if $strict == 2;
	next;
    }

    if ($2 eq 'warning') {
	($had, $warnings{$3}) = remove $1, $warnings{$3};
	if ($strict && ! $had) {
	    print "Didn't expect any warnings on line $1, but got $2 $3.\n";
	    $failures++ if $strict == 2;
	}
    } else {
	($had, $errors{$3}) = remove $1, $errors{$3};
	if ($strict && ! $had) {
	    print "Didn't expect any errors on line $1, but got $2 $3.\n";
	    $failures++ if $strict == 2;
	}
    }
    
    print "Expected to find ".$lines{$1}." on line $1, but got $3.\n"
	if $strict and $lines{$1} ne $3;
}

close MCS;

foreach my $error (keys %errors) {
    print "Failed to report error $error on line $_\n"
	for @{$errors{$error}};
    $failures += @{$errors{$error}};
}

foreach my $warning (keys %warnings) {
    print "Failed to report warning $warning on line $_\n"
	for @{$warnings{$warning}};
    $failures += @{$warnings{$warning}};
}

exit ($failures != 0);
