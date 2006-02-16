#!/usr/bin/perl -w
use strict;

foreach my $file (@ARGV) {
    open FILE,"+<",$file;
    my $contents = "";
    while (defined ($_ = <FILE>)) {
	$contents .= $_;
    }
    truncate FILE, 0;
    seek FILE, 0, 0;
    print FILE "#if NET_2_0\n";
    print FILE $contents . "\n";
    print FILE "#endif\n";
    close FILE;
}
