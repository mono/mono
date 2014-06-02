#!/usr/bin/env perl
# This script simply executes the Team City Cleaner script.  The location of
# the cleaner is defaulted for every OS, but it can be overridden by setting
# the UNITY_TC_CLEANER environment variable.

my $cleanerLocation = "";

if ($^O eq 'MSWin32') {
	$cleanerLocation = "C:/Users/builduser/TeamCityCleaner.pl";
} elsif ($^O eq 'darwin') {
	$cleanerLocation = "/Users/builduser/TeamCityCleaner.pl";
} elsif ($^O eq 'linux') {
	$cleanerLocation = "/home/builduser/TeamCityCleaner.pl";
}
if (not ( $ENV{'UNITY_TC_CLEANER'} eq "" ))
{
	printf("Detected that location of TeamCity Cleaner is overridden by UNITY_TC_CLEANER variable...\n");
	$cleanerLocation = $ENV{'UNITY_TC_CLEANER'};
}
printf("Running TeamCity Cleaner script in: $cleanerLocation\n");
system("$cleanerLocation");
