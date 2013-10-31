use lib ('.', "../../Tools/perl_lib","perl_lib");
use Cwd;
use File::Path;
use Getopt::Long;
use Tools qw(InstallNameTool);

my $root = getcwd();
my $skipbuild=0;
my $debug = 0;
my $minimal = 0;
my $build64 = 0;
my $build_armel = 0;

GetOptions(
   "skipbuild=i"=>\$skipbuild,
   "debug=i"=>\$debug,
   "minimal=i"=>\$minimal,
   "build64=i"=>\$build64,
   "build-armel=i"=>\$build_armel,
) or die ("illegal cmdline options");

die ("illegal cmdline options") if ($build64 and $build_armel);

my $teamcity=0;
if ($ENV{UNITY_THISISABUILDMACHINE})
{
	print "rmtree-ing $root/builds because we're on a buildserver, and want to make sure we don't include old artifacts\n";
	rmtree("$root/builds");
	$teamcity=1;
} else {
	print "not rmtree-ing $root/builds, as we're not on a buildmachine";
	if (($debug==0) && ($skipbuild==0))
	{
		print "\n\nARE YOU SURE YOU DONT WANT TO MAKE A DEBUG BUILD?!?!?!!!!!\n\n\n";
	}
}

my $platform = $build64 ? 'linux64' : $build_armel ? 'linux-armel' : 'linux32' ;
my $bintarget = "$root/builds/monodistribution/bin-$platform";
my $libtarget = "$root/builds/embedruntimes/$platform";
my $etctarget = "$root/builds/monodistribution/etc-$platform";

if ($minimal)
{
	$libtarget = "$root/builds/embedruntimes/$platform-minimal";
}
print("libtarget: $libtarget\n");

system("rm -f $bintarget/mono");
system("rm -f $libtarget/libmono.so");
system("rm -f $libtarget/libMonoPosixHelper.so");
system("rm -f $libtarget/libmono-static.a");

if (not $skipbuild)
{
	#rmtree($bintarget);
	#rmtree($libtarget);

	my $archflags = '';

	if (not $build64 and not $build_armel)
	{
		$archflags = '-m32';
	}
	if ($build_armel)
	{
		$archflags = '-marm -DARM_FPU_NONE';
	}
	if ($debug)
	{
		$ENV{CFLAGS} = "$archflags -g -O0";
	} else
	{
		$ENV{CFLAGS} = "$archflags -Os";  #optimize for size
	}
	$ENV{CXXFLAGS} = $ENV{CFLAGS};
	$ENV{LDFLAGS} = "$archflags";

	#this will fail on a fresh working copy, so don't die on it.
	system("make distclean");
	#were going to tell autogen to use a specific cache file, that we purposely remove before starting.
        #that way, autogen is forced to do all its config stuff again, which should make this buildscript
        #more robust if other targetplatforms have been built from this same workincopy
        system("rm linux.cache");

	chdir("$root/eglib") eq 1 or die ("Failed chdir 1");
	#this will fail on a fresh working copy, so don't die on it.
	system("make distclean");
	system("autoreconf -i") eq 0 or die ("Failed autoreconfing eglib");
	chdir("$root") eq 1 or die ("Failed chdir 2");

	system("autoreconf -i") eq 0 or die ("Failed autoreconfing mono");
	my @autogenparams = ();
	unshift(@autogenparams, "--cache-file=linux.cache");
	unshift(@autogenparams, "--disable-mcs-build");
	unshift(@autogenparams, "--with-glib=embedded");
	unshift(@autogenparams, "--disable-nls");  #this removes the dependency on gettext package
	unshift(@autogenparams, "--disable-parallel-mark");  #this causes crashes
	if(not $build64 and not $build_armel)
	{
		unshift(@autogenparams, "--build=i686-pc-linux-gnu");  #Force x86 build
	}

	# From Massi: I was getting failures in install_name_tool about space
	# for the commands being too small, and adding here things like
	# $ENV{LDFLAGS} = '-headerpad_max_install_names' and
	# $ENV{LDFLAGS} = '-headerpad=0x40000' did not help at all (and also
	# adding them to our final gcc invocation to make the bundle).
	# Lucas noticed that I was lacking a Mono prefix, and having a long
	# one would give us space, so here is this silly looong prefix.
	unshift(@autogenparams, "--prefix=/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890/1234567890");

	if ($minimal)
	{
		unshift(@autogenparams,"--enable-minimal=aot,logging,com,profiler,debug");
	}

	print("\n\n\n\nCalling configure with these parameters: ");
	system("echo", @autogenparams);
	print("\n\n\n\n\n");
	system("calling ./configure",@autogenparams);
	system("./configure", @autogenparams) eq 0 or die ("failing configuring mono");

	system("make clean") eq 0 or die ("failed make cleaning");
	system("make") eq 0 or die ("failing running make for mono");
}

mkpath($bintarget);
mkpath($libtarget);
mkpath("$etctarget/mono");

print "Copying libmono.so\n";
system("cp", "$root/mono/mini/.libs/libmono.so.0","$libtarget/libmono.so") eq 0 or die ("failed copying libmono.so.0");

print "Copying libmono.a\n";
system("cp", "$root/mono/mini/.libs/libmono.a","$libtarget/libmono-static.a") eq 0 or die ("failed copying libmono.a");

print "Copying libMonoPosixHelper.so\n";
system("cp", "$root/support/.libs/libMonoPosixHelper.so","$libtarget/libMonoPosixHelper.so") eq 0 or die ("failed copying libMonoPosixHelper.so");

if ($ENV{"UNITY_THISISABUILDMACHINE"})
{
	system("strip $libtarget/libmono.so") eq 0 or die("failed to strip libmono (shared)");
	system("strip $libtarget/libMonoPosixHelper.so") eq 0 or die("failed to strip libMonoPosixHelper (shared)");
	system("git log --pretty=format:'mono-runtime-$platform = %H %d %ad' --no-abbrev-commit --date=short -1 > $root/builds/versions.txt");
}

system("ln","-f","$root/mono/mini/mono","$bintarget/mono") eq 0 or die("failed symlinking mono executable");
system("ln","-f","$root/mono/metadata/pedump","$bintarget/pedump") eq 0 or die("failed symlinking pedump executable");
system('cp',"$root/data/config","$etctarget/mono/config");
system("chmod","-R","755",$bintarget);
