#!/usr/bin/perl -w

use strict;
use Carp;

my @alltests;
my @allsuites;

while (defined ($_ = <>)) {
    next unless /^\s*suite\.AddTest\s*\((.*)\.(.*?)\.Suite\)/;

    push @alltests, [$1,$2];
}

print qq[using NUnit.Framework;\n];
print qq[using System;\n];
print qq[using System.Threading;\n];
print qq[using System.Globalization;\n\n];


my $alltest;
foreach $alltest (@alltests) {

    my @suites;

    my $testname = $alltest->[0];
    my $filename = $alltest->[0]."/".$alltest->[1].".cs";

    open ALLTEST, $filename or croak "open ($filename): $!";
    while (defined ($_ = <ALLTEST>)) {
	next unless /^\s*suite\.AddTest\s*\((.*)\.Suite\)/;
	next if $1 eq 'HashtableTest'; # broken
	next if $1 eq 'PathTest'; # broken
	push @suites, $1;
	push @allsuites, qq[$testname.Run$1];
    }
    close ALLTEST;

    print qq[namespace MonoTests.$testname\n\{\n];

    my $suite;
    foreach $suite (@suites) {

	my @tests;

	open SUITE, qq[$testname/$suite.cs] or
	    croak "open ($testname/$suite.cs): $!";
	while (defined ($_ = <SUITE>)) {
	    next unless /^\s*public\s+void\s+(Test.*?)\s*\(\s*\)/;
	    push @tests, $1;
	}
	close SUITE;

	print qq[\tpublic class Run$suite : $suite\n\t\{\n];
	print qq[\t\tprotected override void RunTest ()\n\t\t\{\n];
	foreach (@tests) {
	    print qq[\t\t\t$_ ();\n];
	}
	print qq[\t\t\}\n\t\}\n];
    }
    print qq[\}\n\n];
}

print qq[namespace MonoTests\n\{\n];
print qq[\tpublic class RunAllTests\n\{\n];
print qq[\t\tpublic static void AddAllTests (TestSuite suite)\n];
print qq[\t\t\{\n];

my $suite;
foreach $suite (@allsuites) {
    print qq[\t\t\tsuite.AddTest (new MonoTests.$suite ());\n];
}

print qq[\t\t\}\n\t\}\n\}\n];

