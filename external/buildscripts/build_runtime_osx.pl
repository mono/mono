use lib ('.', "../../Tools/perl_lib","perl_lib");
use Cwd;
use File::Path;
use Getopt::Long;
use Tools qw(InstallNameTool);

my $root = getcwd();
my $skipbuild=0;
my $debug = 0;
my $minimal = 0;
my $iphone_simulator = 0;
my $jobs = 4;
my $xcodePath = '/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform';
my $unityPath = "$root/../../unity/build";

GetOptions(
   "skipbuild=i"=>\$skipbuild,
   "debug=i"=>\$debug,
   "minimal=i"=>\$minimal,
   "iphone_simulator=i"=>\$iphone_simulator,
   "j=i"=>\$jobs
) or die ("illegal cmdline options");

my $teamcity=0;
if ($ENV{UNITY_THISISABUILDMACHINE})
{
	print "rmtree-ing $root/builds because we're on a buildserver, and want to make sure we don't include old artifacts\n";
	rmtree("$root/builds");
	$teamcity=1;
	$jobs = "";
	$ENV{'PATH'} = "/usr/local/bin:$ENV{'PATH'}";
} else {
	print "not rmtree-ing $root/builds, as we're not on a buildmachine\n";
	if (($debug==0) && ($skipbuild==0))
	{
		print "\n\nARE YOU SURE YOU DONT WANT TO MAKE A DEBUG BUILD?!?!?!!!!!\n\n\n";
	}
	$jobs = "-j$jobs";
	$ENV{'LIBTOOLIZE'} = 'glibtoolize';
}

my @arches = ('x86_64','i386');
if ($iphone_simulator || $minimal) {
	@arches = ('i386');
}

for my $arch (@arches)
{
	print "Building for architecture: $arch\n";

	my $macversion = '10.5';
	my $sdkversion = '10.6';
	if ($arch eq 'x86_64') {
		$macversion = '10.6';
	}

	my $sdkPath = "$xcodePath/Developer/SDKs/MacOSX$sdkversion.sdk";
	if ($ENV{'UNITY_THISISABUILDMACHINE'} && !$iphone_simulator)
	{
		# Set up clang toolchain
		$sdkPath = "$unityPath/External/MacBuildEnvironment/builds/MacOSX$sdkversion.sdk";
		if (! -d $sdkPath)
		{
			print("Unzipping mac build toolchain\n");
			system('unzip', '-qd', "$unityPath/External/MacBuildEnvironment/builds", "$unityPath/External/MacBuildEnvironment/builds.zip");
		}
		$ENV{'CC'} = "$sdkPath/../usr/bin/clang";
		$ENV{'CXX'} = "$sdkPath/../usr/bin/clang++";
	}

	# Make architecture-specific targets and lipo at the end
	my $bintarget = "$root/builds/monodistribution/bin-$arch";
	my $libtarget = "$root/builds/embedruntimes/osx-$arch";

	if ($minimal)
	{
		$libtarget = "$root/builds/embedruntimes/osx-minimal";
	}
	print("libtarget: $libtarget\n");

	system("rm $bintarget/mono");
	system("rm $libtarget/libmono.0.dylib");
	system("rm $libtarget/libMonoPosixHelper.dylib");
	system("rm -rf $libtarget/libmono.0.dylib.dSYM");

	if (not $skipbuild)
	{
		$stackrealign = '-mstackrealign';

		if ($debug)
		{
			$ENV{CFLAGS} = "-arch $arch -g -O0 -D_XOPEN_SOURCE=1 -DMONO_DISABLE_SHM=1 -DDISABLE_SHARED_HANDLES=1 $stackrealign";
			$ENV{CXXFLAGS} = $ENV{CFLAGS};
			$ENV{LDFLAGS} = "-arch $arch";
		}
		else
		{
			# Switch -fomit-frame-pointer to -fno-omit-frame-pointer as omitting frame pointer screws up stack traces
			my $Os = '-Os -fno-omit-frame-pointer';
			$ENV{CFLAGS} = "-arch $arch -Os -D_XOPEN_SOURCE=1 -DMONO_DISABLE_SHM=1 -DDISABLE_SHARED_HANDLES=1 $stackrealign";  #optimize for size
			$ENV{CXXFLAGS} = $ENV{CFLAGS};
			$ENV{LDFLAGS} = "-arch $arch";
		}
		my $sdkOptions = "-isysroot $sdkPath -mmacosx-version-min=$macversion";

		if ($iphone_simulator)
		{
			$ENV{CFLAGS} = "-D_XOPEN_SOURCE=1 -DTARGET_IPHONE_SIMULATOR -g -O0";
			$macversion = "10.6";
			$sdkversion = "10.6";
		} else {
			$ENV{'MACSDKOPTIONS'} = $sdkOptions;
		}
		
		#this will fail on a fresh working copy, so don't die on it.
		system("make distclean");
		#were going to tell autogen to use a specific cache file, that we purposely remove before starting.
		#that way, autogen is forced to do all its config stuff again, which should make this buildscript
		#more robust if other targetplatforms have been built from this same workincopy
		system("rm osx.cache");

		chdir("$root/eglib") eq 1 or die ("Failed chdir 1");
		
		#this will fail on a fresh working copy, so don't die on it.
		system("make distclean");
		system("autoreconf -i");
		chdir("$root") eq 1 or die ("failed to chdir 2");
		system("autoreconf -i");
		my @autogenparams = ();
		unshift(@autogenparams, "--cache-file=osx.cache");
		unshift(@autogenparams, "--disable-mcs-build");
		unshift(@autogenparams, "--with-glib=embedded");
		unshift(@autogenparams, "--disable-nls");  #this removes the dependency on gettext package

		# From Massi: I was getting failures in install_name_tool about space
		# for the commands being too small, and adding here things like
		# $ENV{LDFLAGS} = '-headerpad_max_install_names' and
		# $ENV{LDFLAGS} = '-headerpad=0x40000' did not help at all (and also
		# adding them to our final gcc invocation to make the bundle).
		# Lucas noticed that I was lacking a Mono prefix, and having a long
		# one would give us space, so here is this silly looong prefix.
		unshift(@autogenparams, "--prefix=/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting");

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
		if ($iphone_simulator)
		{
			system("perl -pi -e 's/#define HAVE_STRNDUP 1//' eglib/config.h");
		}
		system("make $jobs") eq 0 or die ("failing runnig make for mono");
	}

	chdir($root);

	mkpath($bintarget);
	mkpath($libtarget);

	if (!$iphone_simulator)
	{
		my $cmdline = "gcc -arch $arch -bundle -Wl,-reexport_library mono/mini/.libs/libmono.a $sdkOptions -all_load -liconv -o $libtarget/MonoBundleBinary";
		print "About to call this cmdline to make a bundle:\n$cmdline\n";
		system($cmdline) eq 0 or die("failed to link libmono.a into mono bundle");

		print "Symlinking libmono.dylib\n";
		system("ln","-f", "$root/mono/mini/.libs/libmono.0.dylib","$libtarget/libmono.0.dylib") eq 0 or die ("failed symlinking libmono.0.dylib");

		print "Symlinking libmono.a\n";
		system("ln", "-f", "$root/mono/mini/.libs/libmono.a","$libtarget/libmono.a") eq 0 or die ("failed symlinking libmono.a");

		print "Symlinking libMonoPosixHelper.dylib\n";
		system("ln", "-f", "$root/support/.libs/libMonoPosixHelper.dylib","$libtarget/libMonoPosixHelper.dylib") eq 0 or die ("failed symlinking libMonoPosixHelper.dylib");

		if (not $ENV{"UNITY_THISISABUILDMACHINE"})
		{
			rmtree ("$libtarget/libmono.0.dylib.dSYM");
			system ('cp', '-R', "$root/mono/mini/.libs/libmono.0.dylib.dSYM","$libtarget/libmono.0.dylib.dSYM") eq 0 or warn ("Failed copying libmono.0.dylib.dSYM");
		}
	 
		if ($ENV{"UNITY_THISISABUILDMACHINE"})
		{
		#	system("strip $libtarget/libmono.0.dylib") eq 0 or die("failed to strip libmono");
		#	system("strip $libtarget/MonoBundleBinary") eq 0 or die ("failed to strip MonoBundleBinary");
			system("git log --pretty=format:'mono-runtime-osx = %H %d %ad' --no-abbrev-commit --date=short -1 > $root/builds/versions.txt");
		}

		InstallNameTool("$libtarget/libmono.0.dylib", "\@executable_path/../Frameworks/MonoEmbedRuntime/osx/libmono.0.dylib");
		InstallNameTool("$libtarget/libMonoPosixHelper.dylib", "\@executable_path/../Frameworks/MonoEmbedRuntime/osx/libMonoPosixHelper.dylib");

		system("ln","-f","$root/mono/mini/mono","$bintarget/mono") eq 0 or die("failed symlinking mono executable");
		system("ln","-f","$root/mono/metadata/pedump","$bintarget/pedump") eq 0 or die("failed symlinking pedump executable");
	}
}


if (!$iphone_simulator)
{
	# Create universal binaries
	mkpath ("$root/builds/embedruntimes/osx");
	for $file ('libmono.0.dylib','libmono.a','libMonoPosixHelper.dylib') {
		system ('lipo', "$root/builds/embedruntimes/osx-i386/$file", "$root/builds/embedruntimes/osx-x86_64/$file", '-create', '-output', "$root/builds/embedruntimes/osx/$file");
	}
	system('cp', "$root/builds/embedruntimes/osx-i386/MonoBundleBinary", "$root/builds/embedruntimes/osx/MonoBundleBinary");

	mkpath ("$root/builds/monodistribution/bin");
	for $file ('mono','pedump') {
		system ('lipo', "$root/builds/monodistribution/bin-i386/$file", '-create', '-output', "$root/builds/monodistribution/bin/$file");
		# Don't add 64bit executables for now...
		# system ('lipo', "$root/builds/monodistribution/bin-i386/$file", "$root/builds/monodistribution/bin-x86_64/$file", '-create', '-output', "$root/builds/monodistribution/bin/$file");
	}
	
	if ($ENV{"UNITY_THISISABUILDMACHINE"}) {
		# Clean up temporary arch-specific directories
		rmtree("$root/builds/embedruntimes/osx-i386");
		rmtree("$root/builds/embedruntimes/osx-x86_64");
		rmtree("$root/builds/monodistribution/bin-i386");
		rmtree("$root/builds/monodistribution/bin-x86_64");
	}
}
