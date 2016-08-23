use lib ('.', "../../Tools/perl_lib","perl_lib");
use Cwd;
use File::Path;
use Getopt::Long;
use Tools qw(InstallNameTool);

my $root = getcwd();
my $skipbuild=0;
my $debug = 0;
my $minimal = 0;
my $jobs = 4;
my $externalBuildDeps = "$root/../../mono-build-deps/build";

GetOptions(
   "skipbuild=i"=>\$skipbuild,
   "debug=i"=>\$debug,
   "minimal=i"=>\$minimal,
   "j=i"=>\$jobs
) or die ("illegal cmdline options");

if ($ENV{UNITY_THISISABUILDMACHINE})
{
	print "rmtree-ing $root/builds because we're on a buildserver, and want to make sure we don't include old artifacts\n";
	rmtree("$root/builds");
	$jobs = "";
	$ENV{'PATH'} = "/usr/local/bin:$ENV{'PATH'}";
} else {
	print "not rmtree-ing $root/builds, as we're not on a buildmachine\n";
	if (($debug==0) && ($skipbuild==0))
	{
		print "\n\nARE YOU SURE YOU DONT WANT TO MAKE A DEBUG BUILD?!?!?!!!!!\n\n\n";
	}
	$jobs = "-j$jobs";
	my $libtoolize = $ENV{'LIBTOOLIZE'};
	my $libtool = $ENV{'LIBTOOL'};
	if($teamcity)
	{
		$libtoolize = `which glibtoolize`;
		chomp($libtoolize);
		if(!-e $libtoolize)
		{
			$libtoolize = `which libtoolize`;
			chomp($libtoolize);
		}
	}
	if(!-e $libtoolize)
	{
		$libtoolize = 'libtoolize';
	}
	if(!-e $libtool)
	{
		$libtool = $libtoolize;
		$libtool =~ s/ize$//;
	}
	print("Libtool: using $libtoolize and $libtool\n");
	$ENV{'LIBTOOLIZE'} = $libtoolize;
	$ENV{'LIBTOOL'} = $libtool;
}

if ($externalBuildDeps ne "")
{
	print "\n";
	print ">>> Building autoconf, automake, and libtool if needed...\n";
	my $autoconfVersion = "2.69";
	my $automakeVersion = "1.15";
	my $libtoolVersion = "2.4.6";
	my $autoconfDir = "$externalBuildDeps/autoconf-$autoconfVersion";
	my $automakeDir = "$externalBuildDeps/automake-$automakeVersion";
	my $libtoolDir = "$externalBuildDeps/libtool-$libtoolVersion";

	my $builtToolsDir = "$externalBuildDeps/built-tools";

	$ENV{PATH} = "$builtToolsDir/bin:$ENV{PATH}";

	if (!(-d "$autoconfDir"))
	{
		chdir("$externalBuildDeps") eq 1 or die ("failed to chdir to external directory\n");
		system("tar xzf autoconf-$autoconfVersion.tar.gz") eq 0  or die ("failed to extract autoconf\n");

		chdir("$autoconfDir") eq 1 or die ("failed to chdir to autoconf directory\n");
		system("./configure --prefix=$builtToolsDir") eq 0 or die ("failed to configure autoconf\n");
		system("make") eq 0 or die ("failed to make autoconf\n");
		system("make install") eq 0 or die ("failed to make install autoconf\n");

		print ">>> autoconf built\n";

		chdir("$root") eq 1 or die ("failed to chdir to $root\n");
	}

	if (!(-d "$automakeDir"))
	{
		chdir("$externalBuildDeps") eq 1 or die ("failed to chdir to external directory\n");
		system("tar xzf automake-$automakeVersion.tar.gz") eq 0  or die ("failed to extract automake\n");

		chdir("$automakeDir") eq 1 or die ("failed to chdir to automake directory\n");
		system("./configure --prefix=$builtToolsDir") eq 0 or die ("failed to configure automake\n");
		system("make") eq 0 or die ("failed to make automake\n");
		system("make install") eq 0 or die ("failed to make install automake\n");

		print ">>> automake built\n";

		chdir("$root") eq 1 or die ("failed to chdir to $root\n");
	}

	if (!(-d "$libtoolDir"))
	{
		chdir("$externalBuildDeps") eq 1 or die ("failed to chdir to external directory\n");
		system("tar xzf libtool-$libtoolVersion.tar.gz") eq 0  or die ("failed to extract libtool\n");
	
		chdir("$libtoolDir") eq 1 or die ("failed to chdir to libtool directory\n");
		system("./configure --prefix=$builtToolsDir") eq 0 or die ("failed to configure libtool\n");
		system("make") eq 0 or die ("failed to make libtool\n");
		system("make install") eq 0 or die ("failed to make install libtool\n");

		print ">>> libtool built\n";

		chdir("$root") eq 1 or die ("failed to chdir to $root\n");
	}

	$ENV{'LIBTOOLIZE'} = "$builtToolsDir/bin/libtoolize";
	$ENV{'LIBTOOL'} = "$builtToolsDir/bin/libtool";
}

print ">>> Checking on some tools...\n";
system("which", "autoconf");
system("autoconf", "--version");

system("which", "automake");
system("automake", "--version");

system("which", "libtool");
system("libtool", "--version");

system("which", "libtoolize");
system("libtoolize", "--version");

system("which", "autoreconf");
print("\n");

print ">>> LIBTOOLIZE before Build = $ENV{LIBTOOLIZE}\n";
print ">>> LIBTOOL before Build = $ENV{LIBTOOL}\n";

my @arches = ('i386','x86_64');
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

	# Set up clang toolchain
	$sdkPath = "$externalBuildDeps/MacBuildEnvironment/builds/MacOSX$sdkversion.sdk";
	if (! -d $sdkPath)
	{
		print("Unzipping mac build toolchain\n");
		system('unzip', '-qd', "$externalBuildDeps/MacBuildEnvironment", "$externalBuildDeps/MacBuildEnvironment/builds.zip");
	}
	$ENV{'CC'} = "$sdkPath/../usr/bin/clang";
	$ENV{'CXX'} = "$sdkPath/../usr/bin/clang++";

	# Make architecture-specific targets and lipo at the end
	my $bintarget = "$root/builds/monodistribution/bin-$arch";
	my $libtarget = "$root/builds/embedruntimes/osx-$arch";
	my $sdkoptions = '';

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
		$sdkOptions = "-isysroot $sdkPath -mmacosx-version-min=$macversion $ENV{CFLAGS}";
		$ENV{'MACSDKOPTIONS'} = $sdkOptions;

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
		system("make $jobs") eq 0 or die ("failing runnig make for mono");
	}

	chdir($root);

	mkpath($bintarget);
	mkpath($libtarget);

	my $cmdline = "gcc -arch $arch $sdkoptions -bundle -Wl,-reexport_library mono/mini/.libs/libmono.a $sdkOptions -all_load -liconv -o $libtarget/MonoBundleBinary";
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

# Create universal binaries
mkpath ("$root/builds/embedruntimes/osx");
for $file ('libmono.0.dylib','libmono.a','libMonoPosixHelper.dylib') {
	system ('lipo', "$root/builds/embedruntimes/osx-i386/$file", "$root/builds/embedruntimes/osx-x86_64/$file", '-create', '-output', "$root/builds/embedruntimes/osx/$file");
}
system('cp', "$root/builds/embedruntimes/osx-i386/MonoBundleBinary", "$root/builds/embedruntimes/osx/MonoBundleBinary");

# Create universal binaries
mkpath ("$root/builds/embedruntimes/osx");
for $file ('libmono.0.dylib','libmono.a','libMonoPosixHelper.dylib') {
	system ('lipo', "$root/builds/embedruntimes/osx-i386/$file", "$root/builds/embedruntimes/osx-x86_64/$file", '-create', '-output', "$root/builds/embedruntimes/osx/$file");
}
system('cp', "$root/builds/embedruntimes/osx-i386/MonoBundleBinary", "$root/builds/embedruntimes/osx/MonoBundleBinary-i386");
system('cp', "$root/builds/embedruntimes/osx-x86_64/MonoBundleBinary", "$root/builds/embedruntimes/osx/MonoBundleBinary-x86_64");

if ($ENV{"UNITY_THISISABUILDMACHINE"}) {
	# Clean up temporary arch-specific directories
	rmtree("$root/builds/embedruntimes/osx-i386");
	rmtree("$root/builds/embedruntimes/osx-x86_64");
	rmtree("$root/builds/monodistribution/bin-i386");
	rmtree("$root/builds/monodistribution/bin-x86_64");
}
