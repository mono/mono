#!/usr/bin/perl

# PrepareAndroidSDK.pl -sdk=android-4 -env=setupenv.sh && source setupenv.sh

package PrepareAndroidSDK;

use File::Spec;
use File::Basename qw(dirname basename);
use lib File::Spec->rel2abs(dirname(__FILE__));
use PrepareAndroidSDK;

if ($#ARGV + 1 == 0)
{
	print "Usage:\n";
	print "\t" . basename(__FILE__) . " -sdk=<android-X> -tools=<N> -ndk=<rX> -env=<envsetup.sh/.bat>\n";
	print "\ti.e. \"" . basename(__FILE__) . " -sdk=android-8 -tools=17 -ndk=r5c -env=setupenv.sh && source setupenv.sh\"\n";
	print "\n";
}

my ($sdk, $tools, $ndk, $env);
Getopt::Long::GetOptions("sdk=s"=>\$sdk, "tools=s"=>\$tools, "ndk=s"=>\$ndk, "env=s"=>\$setenv) or die ("Illegal cmdline options");
PrepareAndroidSDK::GetAndroidSDK($sdk, $tools, $ndk, $setenv);
