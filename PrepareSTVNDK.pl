#!/usr/bin/perl

package PrepareSTVNDK;

use File::Spec;
use File::Basename qw(dirname basename);
use lib File::Spec->rel2abs(dirname(__FILE__));
use PrepareSTVNDK;

if ($#ARGV + 1 == 0)
{
	print "Usage:\n";
	print "\t" . basename(__FILE__) . " -ndk=<rX>\n";
	print "\ti.e. \"" . basename(__FILE__) . " -ndk=r01 -env=setupenv.sh && source setupenv.sh\"\n";
	print "\n";
}

my ($sdk, $tools, $ndk, $env);
Getopt::Long::GetOptions("ndk=s"=>\$ndk, "env=s"=>\$setenv) or die ("Illegal cmdline options");
PrepareSTVNDK::GetSTVNDK($ndk, $setenv);
