#!/usr/bin/perl
#
# Make Valgrind output for Mono applications readable.
#
# 8 March 2005
#
# Nat Friedman <nat@novell.com>
# 
# Usage:
#     valgrind --leak-check=full mono -v foo.exe > mono-v.out 2> valgrind.out
#     valgrind-monofunc.pl mono-v.out valgrind.out > valgrind-monofunc.out
#

my $path = $ARGV[0];

if (! open (FILE, $path)) {
    die ("Could not open $path");
}

my @map = ();
my $i = 0;
while (<FILE>) {

    $_ =~ s,\n,,g;
    $_ =~ s,\r,,g;

    if ($_ =~ /^Method/) {

	$method = $ip1 = $ip2 = $_;

	$method =~ s,^Method (.*) emitted at.*$,\1,;
	$ip1 =~ s,^.*emitted at (0x[a-f0-9]*).*$,\1,;
	$ip2 =~ s,^.*to (0x[a-f0-9]*).*$,\1,g;

	my %entry = ( "method" => $method,
		      "ip1" => $ip1,
		      "ip2" => $ip2 );

	push (@map, \%entry);
	$i ++;
    }
}

print "Loaded $i method VMAs from $path\n";

#foreach my $foo (@map) {
#    print $$foo{"ip1"} . " - " . $$foo{"ip2"} . " " . $$foo{"method"} . "\n\n";
#}

my $path2 = $ARGV[1];

if (! open (FILE2, $path2)) {
    die ("Could not open $path");
}

while (<FILE2>) {
    if ($_ =~ /\?\?\?/) {
	$ip = $_;
	$ip =~ s,^.*by (0x[A-Fa-f0-9]*): \?\?\?.*$,\1,g;
	$ip = lc ($ip);
	$ip =~ s,\n,,g;
	$ip =~ s,\r,,g;

	my $last = "UNKNOWN";
	foreach my $m (@map) {
#	    print "Comparing $ip to " . $$m{"ip1"} . "(" . $$m{"method"} . ") ...\n";
	    if (hex ($ip) < hex ($$m{"ip1"})) {
		$_ =~ s,\?\?\?,$last,g;
		break;
	    }
	    $last = $$m{"method"};
	}
    }

    print $_;
}
