#!/usr/bin/perl -w

use strict;
use IPC::Open3;

# Setting $strict to 1 enables line number checks, setting it to 2 makes
# line number mismatches fatal.
my $strict = 2;
my $failures = 0;
my $mcs = (defined $ENV{MCS}) ? $ENV{MCS} : 'mcs';

unless ($#ARGV == 0) {
    print STDERR "Usage: $0 testcase.cs\n";
    exit 1;
}

my %errors = ();
my %warnings = ();
my %lines = ();

my $filename = $ARGV [0];
my $input;

my $line = 0;

open (INPUT, "<$filename") or die
    "Can't open testcase: $!";
while (defined ($input = <INPUT>)) {
    ++$line;
    chop $input;
    next unless $input =~ m,^\s*//\s*(error|warning)?\s*(CS\d+),;

    if ((defined $1) and ($1 eq 'warning')) {
	++$warnings{$2};
    } else {
	++$errors{$2};
    }

    $lines{$line+1} = $2;
}
close INPUT;

open (MCS, "$mcs $filename|") or die
    "Can't open mcs pipe: $!";

while (defined ($input = <MCS>)) {
    chop $input;
    next unless $input =~ m,\((\d+)\)\s+(warning|error)\s+(CS\d+):,;

    if ($2 eq 'warning') {
	--$warnings{$3};
    } else {
	--$errors{$3};
    }

    next unless $strict;

    if (!defined $lines{$1}) {
	print "Didn't expect any warnings or errors in line $1, but got $2 $3.\n";
	$failures++ if $strict == 2;
    } elsif ($lines{$1} ne $3) {
	print "Expected to find ".$lines{$1}." on line $1, but got $3.\n";
	$failures++ if $strict == 2;
    }
}

close MCS;

foreach my $error (keys %errors) {
    my $times = $errors{$error};

    if ($times == -1) {
	print "Unexpected error $error.\n";
    } elsif ($times < 0) {
	print "Unexpected error $error (reported ".(-$times)." times).\n";
    } elsif ($times == 1) {
	print "Failed to report error $error.\n";
    } elsif ($times > 0) {
	print "Failed to report error $error $times times.\n";
    }

    $failures++ unless $times == 0;    
}

foreach my $warning (keys %warnings) {
    my $times = $warnings{$warning};

    if ($times == -1) {
	print "Unexpected warning $warning.\n";
    } elsif ($times < 0) {
	print "Unexpected warning $warning (reported ".(-$times)." times).\n";
    } elsif ($times == 1) {
	print "Failed to report warning $warning.\n";
    } elsif ($times > 0) {
	print "Failed to report warning $warning $times times.\n";
    }

    $failures++ unless $times == 0;    
}

if ($failures == 0) {
    exit 0;
} else {
    exit 1;
}
