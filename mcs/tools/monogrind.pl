#!/usr/bin/perl
#
# Valgrind a Mono-based app.
#
# 8 March 2005
#
# Nat Friedman <nat@novell.com>
# 
# Usage:
#     monogrind [valgrind options] foo.exe [foo.exe options]
#

use IPC::Open3;

my $valgrind_options = "";
my $exe_options = "";
my $exe = "";
my $got_exe = 0;

foreach my $arg (@ARGV) {
    if ($arg =~ /.*\.exe$/) {
	$exe = $arg;
	$got_exe = 1;
    } elsif ($got_exe == 1) {
	$exe_options .= " $arg";
    } else {
	$valgrind_options .= " $arg";
    }
}

my $cmd = "valgrind $valgrind_options mono -v $exe $exe_options";

my ($wtr, $rdr, $err);
$pid = open3 ($wtr, $rdr, $err, $cmd);

# Where we hold the IP/Method mappings
my @map = ();

# Build up all the stderr stuff and process it en masse at the end
$valgrind_output = "";

while (<$rdr>) {
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
    } elsif ($_ =~ /^==/)  {
	$valgrind_output .= "$_\n";
    } else {
	print "$_\n";
    }
}

# Read the rest of stderr
while (<$err>) {
    $valgrind_output .= "$_\n";
}

my @valgrind_lines = split (/\n/, $valgrind_output);
foreach my $val_line (@valgrind_lines) {
    $_ = $val_line;
    if ($_ =~ /\?\?\?/) {
	$ip = $_;
	$ip =~ s,^.*by (0x[A-Fa-f0-9]*): \?\?\?.*$,\1,g;
	$ip = lc ($ip);
	$ip =~ s,\n,,g;
	$ip =~ s,\r,,g;

	my $last = "UNKNOWN";
	foreach my $m (@map) {
	    if (hex ($ip) < hex ($$m{"ip1"})) {
		$_ =~ s,\?\?\?,$last,g;
		break;
	    }
	    $last = $$m{"method"};
	}
    }

    print "$_\n";
}
