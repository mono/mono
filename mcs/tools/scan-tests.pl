#!/usr/bin/perl -w

use strict;
use Carp;

my @alltests;
my @allsuites;

my @badtests = qw[System.Collections.HastableTest System.Collections.StackTest System.IO.PathTest];

die "Usage: $0 input output" unless $#ARGV == 1;

open INPUT, $ARGV[0] or croak "open ($ARGV[0]): $!";
while (defined ($_ = <INPUT>)) {
    next unless /^\s*suite\.AddTest\s*\((.*)\.(.*?)\.Suite\)/;

    push @alltests, [$1,$2];
}
close INPUT;

open OUTPUT, "> $ARGV[1]" or croak "open (> $ARGV[1]): $!";
select OUTPUT;

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
	my $name = $1;

	next if grep $name, @badtests;

	push @suites, $name;
	push @allsuites, qq[$testname.Run$name];
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
print qq[\tpublic class RunAllTests\n\t\{\n];
print qq[\t\tpublic static void AddAllTests (TestSuite suite)\n];
print qq[\t\t\{\n];

my $suite;
foreach $suite (@allsuites) {
    print qq[\t\t\tsuite.AddTest (new MonoTests.$suite ());\n];
}

print qq[\t\t\}\n\t\}\n\}\n\n];

print qq[class MainApp\n\{\n];
print qq[\tpublic static void Main()\n\t\{\n];
print qq[\t\tThread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");\n\n];
print qq[\t\tTestResult result = new TestResult ();\n];
print qq[\t\tTestSuite suite = new TestSuite ();\n];
print qq[\t\tMonoTests.RunAllTests.AddAllTests (suite);\n];
print qq[\t\tsuite.Run (result);\n];
print qq[\t\tMonoTests.MyTestRunner.Print (result);\n];
print qq[\t\}\n\}\n\n];

close OUTPUT;

