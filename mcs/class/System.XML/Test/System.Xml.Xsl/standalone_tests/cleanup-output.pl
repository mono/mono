#!/usr/bin/perl

&main;

sub main ()
{
	print "deleting file.\n";
	open (fd, "output.lst") or die;
	foreach $fn (<fd>) {
		chomp $fn;
		if (-e $fn) {
			unlink $fn;
		} else {
			print "File : " , $fn; 
		}
	}
}
