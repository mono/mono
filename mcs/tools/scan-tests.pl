#!/usr/bin/perl -w

use strict;
use Carp;

my @allfiles;

my @badsuites = qw[System\.Collections/HashtableTest System\.Collections/StackTest System\.Collections\.Specialized\.BasicOperationsTest];
my @badtests = qw[PathTest:TestGetTempFileName XmlTextReaderTests:TestIsNameChar XmlTextReaderTests:TestIsFirstNameChar ByteTest:TestParse];
my @mapfiles = ('s,^MonoTests\.(.*)/,$1/,',
		's,^Ximian\.Mono\.Tests(.*)/,,',
		's,^System\.Net/,,',
		's,^Collections\.Specialized\.,,',
		's,^Text\.RegularExpressions\.,,'
		);
my @maptests = ();
my @mapnamespace = ();

die "Usage: $0 input output" unless $#ARGV == 1;

my $namespace = 'MonoTests';

sub parse_test {
    my ($filename, $namespace, $testname, $suite) = @_;

    foreach (@badsuites) {
	return if $filename =~ /$_/;
    }

    my $map;
    foreach $map (@mapfiles) {
	eval "\$filename =~ $map";
    }

    foreach (@allfiles) {
	return if $filename eq $_->[0];
    }

    # print STDERR "PARSE: |$filename|\n";

    push @allfiles, [$filename,$namespace,$testname,$suite,[]];

    my $INPUT;
    open $INPUT, $filename or croak "open ($filename): $!";
    while (defined ($_ = <$INPUT>)) {
	if (/^\s*namespace\s*([\w\.]+?)\s*$/) {
	    $namespace = $1;
	    next;
	}
	if (/^\s*suite\.AddTest\s*\((.*)\.(.*?)\.Suite\)/) {
	    my $filename = (defined $namespace) ? qq[$namespace.$1/$2.cs] : qq[$1/$2.cs];
	    my $nsprefix = (defined $namespace) ? qq[$namespace.$1] : qq[MonoTests.$1];
	    parse_test ($filename, $nsprefix, $1, $2);
	    next;
	}
	if (/^\s*suite\.AddTest\s*\((.*?)\.Suite\)/) {
	    my $filename = (defined $namespace) ? qq[$namespace/$1.cs] : qq[$1.cs];
	    parse_test ($filename, $namespace, '', $1);
	    next;
	}
	if (/^\s*suite\.AddTest\s*\(\s*new\s+TestSuite\s*\(\s*typeof\(\s*(.*)\s*\)\s*\)\s*\);/) {
	    my $filename = (defined $namespace) ? qq[$namespace/$1.cs] : qq[$1.cs];
	    parse_test ($filename, $namespace, '', $1);
	    next;
	}
    }
    close $INPUT;
}

parse_test ($ARGV[0], undef, '', '');

my $file;
foreach $file (@allfiles) {
    my ($filename,$namespace,$testname,$suite) = @$file;

    open SUITE, $filename or croak "open ($filename): $!";
    while (defined ($_ = <SUITE>)) {
	next unless /^\s*public\s+void\s+(Test.*?)\s*\(\s*\)/;
	push @{$file->[4]}, $1;
    }
    close SUITE;
}

open OUTPUT, "> $ARGV[1]" or croak "open (> $ARGV[1]): $!";
select OUTPUT;

print qq[using NUnit.Framework;\n];
print qq[using System;\n];
print qq[using System.Threading;\n];
print qq[using System.Globalization;\n\n];


my $alltest;
foreach $alltest (@allfiles) {

    my ($filename,$namespace,$testname,$suite,$tests) = @$alltest;
    my @tests = @$tests;

    next unless defined $namespace;
    next unless $#tests >= 0;

    # print STDERR "DOING TEST: |$testname|$filename|\n";

    $namespace .= ".$testname" unless $testname eq '';

    print qq[namespace $namespace\n\{\n];
    print qq[\tpublic class Run$suite : $suite\n\t\{\n];
    print qq[\t\tprotected override void RunTest ()\n\t\t\{\n];
#    print qq[\t\t\tbool errorThrown = false;\n\n];
    my $test;
  testloop:
    foreach $test (@tests) {
	my $badtest;
	$filename =~ s/\.cs$//;
	my $fullname = qq[$filename:$test];
	# print STDERR "TEST: |$fullname|\n";
	foreach $badtest (@badtests) {
	    next testloop if $fullname =~ /$badtest/;
	}
#	print qq[\t\t\ttry \{\n\t\t\t\t$test ();\n\t\t\t\} catch \{\n];
#	print qq[\t\t\t\tConsole.WriteLine ("$namespace:$suite:$test failed");\n];
#	print qq[\t\t\t\terrorThrown = true;\n];
#	print qq[\t\t\t\}\n];
	print qq[\t\t\t$test ();\n];
#	print qq[\t\t\tConsole.WriteLine ("$namespace:$suite:$test DONE");\n];
    }
    print qq[\n];
#    print qq[\t\t\tif (errorThrown)\n\t\t\t\tthrow new ArgumentException ();\n];
    print qq[\t\t\}\n\t\}\n];
    print qq[\}\n\n];
}

print qq[namespace $namespace\n\{\n];
print qq[\tpublic class RunAllTests\n\t\{\n];
print qq[\t\tpublic static void AddAllTests (TestSuite suite)\n];
print qq[\t\t\{\n];

foreach $alltest (@allfiles) {
    my ($filename,$namespace,$testname,$suite,$tests) = @$alltest;
    my @tests = @$tests;

    next unless defined $namespace;
    next unless $#tests >= 0;

    $namespace .= ".$testname" unless $testname eq '';

    print qq[\t\t\tsuite.AddTest (new $namespace.Run$suite ());\n];
}

print qq[\t\t\}\n\t\}\n\}\n\n];

print qq[class MainApp\n\{\n];
print qq[\tpublic static void Main()\n\t\{\n];
print qq[\t\tThread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");\n\n];
print qq[\t\tTestResult result = new TestResult ();\n];
print qq[\t\tTestSuite suite = new TestSuite ();\n];
print qq[\t\t$namespace.RunAllTests.AddAllTests (suite);\n];
print qq[\t\tsuite.Run (result);\n];
print qq[\t\tMonoTests.MyTestRunner.Print (result);\n];
print qq[\t\}\n\}\n\n];

close OUTPUT;

