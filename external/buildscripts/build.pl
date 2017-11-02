use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;
use lib ('external/buildscripts', "../../Tools/perl_lib","perl_lib", 'external/buildscripts/perl_lib');
use Tools qw(InstallNameTool);

print ">>> PATH in Build All = $ENV{PATH}\n\n";

my $currentdir = getcwd();

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);

my $buildscriptsdir = "$monoroot/external/buildscripts";
my $addtoresultsdistdir = "$buildscriptsdir/add_to_build_results/monodistribution";
my $buildsroot = "$monoroot/builds";
my $includesroot = "$buildsroot/include";
my $sourcesroot = "$buildsroot/source";
my $distdir = "$buildsroot/monodistribution";
my $buildMachine = $ENV{UNITY_THISISABUILDMACHINE};

# This script should not be ran on windows, if it is, kindly call the wrapper
# to switch over to cygwin
if ($^O eq "MSWin32")
{
	print(">>> build.pl called from Windows.  Switching over to cygwin\n");
	system("perl", "$buildscriptsdir/build_win_wrapper.pl", @ARGV) eq 0 or die("\n");
	exit 0;
}

system("source","~/.profile");

my $build=0;
my $clean=0;
my $jobs=8;
my $test=0;
my $artifact=0;
my $debug=0;
my $disableMcs=0;
my $mcsOnly=0;
my $buildUsAndBoo=0;
my $artifactsCommon=0;
my $artifactsRuntime=1;
my $runRuntimeTests=1;
my $runClasslibTests=1;
my $checkoutOnTheFly=0;
my $forceDefaultBuildDeps=0;
my $existingMonoRootPath = '';
my $sdk = '';
my $arch32 = 0;
my $winPerl = "";
my $winMonoRoot = "";
my $msBuildVersion = "14.0";
my $buildDeps = "";
my $android=0;
my $androidArch = "";
my $iphone=0;
my $iphoneArch = "";
my $iphoneCross=0;
my $iphoneSimulator=0;
my $iphoneSimulatorArch="";
my $tizen=0;
my $tizenEmulator=0;
my $windowsSubsystemForLinux=0;

# Handy troubleshooting/niche options
my $skipMonoMake=0;

# The prefix hack probably isn't needed anymore.  Let's disable it by default and see how things go
my $shortPrefix=1;

# Disabled by default for now.  causes more problems than it's worth when actively making changes to the build scripts.
# Would be okay to turn on once the build scripts stabilize and you just want to rebuild code changes
my $enableCacheFile=0;

# Linux toolchain setup needs this
my @commandPrefix = ();

print(">>> Build All Args = @ARGV\n");

GetOptions(
	'build=i'=>\$build,
	'clean=i'=>\$clean,
	'test=i'=>\$test,
	'artifact=i'=>\$artifact,
	'artifactscommon=i'=>\$artifactsCommon,
	'artifactsruntime=i'=>\$artifactsRuntime,
	'debug=i'=>\$debug,
	'disablemcs=i'=>\$disableMcs,
	'mcsonly=i'=>\$mcsOnly,
	'buildusandboo=i'=>\$buildUsAndBoo,
	'runtimetests=i'=>\$runRuntimeTests,
	'classlibtests=i'=>\$runClasslibTests,
	'arch32=i'=>\$arch32,
	'jobs=i'=>\$jobs,
	'sdk=s'=>\$sdk,
	'existingmono=s'=>\$existingMonoRootPath,
	'skipmonomake=i'=>\$skipMonoMake,
	'shortprefix=i'=>\$shortPrefix,
	'winperl=s'=>\$winPerl,
	'winmonoroot=s'=>\$winMonoRoot,
	'msbuildversion=s'=>\$msBuildVersion,
	'checkoutonthefly=i'=>\$checkoutOnTheFly,
	'builddeps=s'=>\$buildDeps,
	'forcedefaultbuilddeps=i'=>\$forceDefaultBuildDeps,
	'android=i'=>\$android,
	'androidarch=s'=>\$androidArch,
	'iphone=i'=>\$iphone,
	'iphonearch=s'=>\$iphoneArch,
	'iphonecross=i'=>\$iphoneCross,
	'iphonesimulator=i'=>\$iphoneSimulator,
	'tizen=i'=>\$tizen,
	'tizenemulator=i'=>\$tizenEmulator,
	'windowssubsystemforlinux=i'=>\$windowsSubsystemForLinux,
	'enablecachefile=i'=>\$enableCacheFile,
) or die ("illegal cmdline options");

print ">>> Mono checkout = $monoroot\n";

print(">> System Info : \n");
system("uname", "-a");

my $monoRevision = `git rev-parse HEAD`;
chdir("$buildscriptsdir") eq 1 or die ("failed to chdir : $buildscriptsdir\n");
my $buildScriptsRevision = `git rev-parse HEAD`;
chdir("$monoroot") eq 1 or die ("failed to chdir : $monoroot\n");

print(">>> Mono Revision = $monoRevision\n");
print(">>> Build Scripts Revision = $buildScriptsRevision\n");

if ($androidArch ne "")
{
	$android = 1;
}

if ($iphoneArch ne "")
{
	$iphone = 1;
}

if($iphoneSimulator)
{
	if ($arch32)
	{
		$iphoneSimulatorArch = "i386";
	}
	else
	{
		$iphoneSimulatorArch = "x86_64";
	}
}

my $isDesktopBuild = 1;
if ($android || $iphone || $iphoneCross || $iphoneSimulator || $tizen || $tizenEmulator)
{
	$isDesktopBuild = 0;

	# Disable building of the class libraries by default when building the android runtime
	# since we don't care about a class library build in this situation (as of writing this at least)
	# but only if the test flag is not set.  If the test flag was set, we'd need to build the classlibs
	# in order to run the tests
	$disableMcs = 1 if(!($test));
}

# Do any settings agnostic per-platform stuff
my $externalBuildDeps = "";
my $externalBuildDepsIl2Cpp = "$monoroot/../../il2cpp/build";

if ($buildDeps ne "" && not $forceDefaultBuildDeps)
{
	$externalBuildDeps = $buildDeps;
}
else
{
	$externalBuildDeps = "$monoroot/../../mono-build-deps/build";
}

# Only clean up the path if the directory exists, if it doesn't exist,
# abs_path ends up returning an empty string
$externalBuildDeps = abs_path($externalBuildDeps) if (-d $externalBuildDeps);

my $existingExternalMonoRoot = "$externalBuildDeps/mono";
my $existingExternalMono = "";
my $monoHostArch = "";
my $monoprefix = "$monoroot/tmp";
my $runningOnWindows=0;
if($^O eq "linux")
{
	$monoHostArch = $arch32 ? "i686" : "x86_64";
	$existingExternalMono = "$existingExternalMonoRoot/linux";
}
elsif($^O eq 'darwin')
{
	$monoHostArch = $arch32 ? "i386" : "x86_64";
	$existingExternalMono = "$existingExternalMonoRoot/osx";

	# From Massi: I was getting failures in install_name_tool about space
	# for the commands being too small, and adding here things like
	# $ENV{LDFLAGS} = '-headerpad_max_install_names' and
	# $ENV{LDFLAGS} = '-headerpad=0x40000' did not help at all (and also
	# adding them to our final gcc invocation to make the bundle).
	# Lucas noticed that I was lacking a Mono prefix, and having a long
	# one would give us space, so here is this silly looong prefix.
	if (not $shortPrefix)
	{
		$monoprefix = "$monoroot/tmp/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting";
	}
}
else
{
	$monoHostArch = "i686";
	$existingExternalMono = "$existingExternalMonoRoot/win";
	$runningOnWindows = 1;

	# We only care about an existing mono if we need to build.
	# So only do this path clean up if we are building.
	if ($build)
	{
		if ($existingMonoRootPath ne "" && not $existingMonoRootPath =~ /^\/cygdrive/)
		{
			$existingMonoRootPath = `cygpath -u $existingMonoRootPath`;
			chomp($existingMonoRootPath);
		}

		$existingMonoRootPath =~ tr/\\//d;
	}
}

if ($runningOnWindows)
{
	# Fixes a line ending issue that happens on windows when we try to run autogen.sh
	$ENV{'SHELLOPTS'} = "igncr";
}

print(">>> Existing Mono = $existingMonoRootPath\n");
print(">>> Mono Arch = $monoHostArch\n");

if ($build)
{
	my $platformflags = '';
	my $host = '';
	my $mcs = '';

	my $iphoneCrossAbi = "arm-apple-darwin10";
	my $iphoneCrossMonoBinToUse = "$monoroot/builds/monodistribution/bin";

	my @configureparams = ();

	push @configureparams, "--disable-mcs-build" if($disableMcs);
	push @configureparams, "--with-glib=embedded";
	push @configureparams, "--disable-nls";  #this removes the dependency on gettext package
	push @configureparams, "--disable-btls";  #this removes the dependency on cmake to build btls for now
	push @configureparams, "--with-mcs-docs=no";
	push @configureparams, "--prefix=$monoprefix";

	if(!($disableMcs))
	{
		push @configureparams, "--with-unityjit=yes";
		push @configureparams, "--with-unityaot=yes";
	}

	if ($isDesktopBuild)
	{
		push @configureparams, "--with-monotouch=no";
	}

	if ($existingMonoRootPath eq "")
	{
		print(">>> No existing mono supplied.  Checking for external...\n");

		if (!(-d "$externalBuildDeps"))
		{
			if (not $checkoutonthefly)
			{
				print(">>> No external build deps found.  Might as well try to check them out.  If it fails, we'll continue and trust mono is in your PATH\n");
			}

			# Check out on the fly
			print(">>> Checking out mono build dependencies to : $externalBuildDeps\n");
			my $repo = "https://ono.unity3d.com/unity-extra/mono-build-deps";
			print(">>> Cloning $repo at $externalBuildDeps\n");
			my $checkoutResult = system("hg", "clone", $repo, "$externalBuildDeps");

			if ($checkoutOnTheFly && $checkoutResult ne 0)
			{
				die("failed to checkout mono build dependencies\n");
			}

			# Only clean up if the dir exists.   Otherwise abs_path will return empty string
			$externalBuildDeps = abs_path($externalBuildDeps) if (-d $externalBuildDeps);
		}

		if (!(-d "$externalBuildDepsIl2Cpp"))
		{
			my $il2cpp_repo = "https://bitbucket.org/Unity-Technologies/il2cpp";
            print(">>> Cloning $il2cpp_repo at $externalBuildDepsIl2Cpp\n");
            $checkoutResult = system("hg", "clone", $il2cpp_repo, "$externalBuildDepsIl2Cpp");

            if ($checkoutOnTheFly && $checkoutResult ne 0)
            {
                die("failed to checkout IL2CPP for the mono build dependencies\n");
            }
		}

		if (-d "$existingExternalMono")
		{
			print(">>> External mono found at : $existingExternalMono\n");

			if (-d "$existingExternalMono/builds")
			{
				print(">>> Mono already extracted at : $existingExternalMono/builds\n");
			}

			if (!(-d "$existingExternalMono/builds"))
			{
				# We need to extract builds.zip
				print(">>> Extracting mono builds.zip...\n");
				system("unzip", "$existingExternalMono/builds.zip", "-d", "$existingExternalMono") eq 0 or die("failed to extract mono builds.zip\n");
			}

			$existingMonoRootPath = "$existingExternalMono/builds";
		}
		else
		{
			print(">>> No external mono found.  Trusting a new enough mono is in your PATH.\n");
		}
	}

	if ($existingMonoRootPath ne "" && !(-d $existingMonoRootPath))
	{
		die("Existing mono not found at : $existingMonoRootPath\n");
	}

	if ($externalBuildDeps ne "")
	{
		print "\n";
		print ">>> Building autoconf, texinfo, automake, and libtool if needed...\n";
		my $autoconfVersion = "2.69";
		my $texinfoVersion = "4.8";
		my $automakeVersion = "1.15";
		my $libtoolVersion = "2.4.6";
		my $autoconfDir = "$externalBuildDeps/autoconf-$autoconfVersion";
		my $texinfoDir = "$externalBuildDeps/texinfo-$texinfoVersion";
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

			chdir("$monoroot") eq 1 or die ("failed to chdir to $monoroot\n");
		}

		if (!(-d "$texinfoDir") and $windowsSubsystemForLinux)
		{
			chdir("$externalBuildDeps") eq 1 or die ("failed to chdir to external directory\n");
			system("tar xzf texinfo-$texinfoVersion.tar.gz") eq 0 or die ("failed to extract texinfo\n");

			chdir($texinfoDir) eq 1 or die ("failed to chdir to texinfo directory\n");
			system("./configure --prefix=$builtToolsDir") eq 0 or die ("failed to configure texinfo\n");
			system("make") eq 0 or die ("failed to make texinfo\n");
			system("make install") eq 0 or die ("failed to make install texinfo\n");

			chdir("$monoroot") eq 1 or die ("failed to chdir to $monoroot\n");
		}

		if (!(-d "$automakeDir"))
		{
			my $automakeMakeFlags = "";
			chdir("$externalBuildDeps") eq 1 or die ("failed to chdir to external directory\n");
			system("tar xzf automake-$automakeVersion.tar.gz") eq 0  or die ("failed to extract automake\n");

			chdir("$automakeDir") eq 1 or die ("failed to chdir to automake directory\n");
			if($windowsSubsystemForLinux)
			{
				#Windows subsystem needs to run bootstrap, and make needs to be run with -i due to one doc failing to build
				system("./bootstrap.sh") eq 0 or die ("failed to bootstrap automake\n");
				$automakeMakeFlags = "-i";
			}
			system("./configure --prefix=$builtToolsDir") eq 0 or die ("failed to configure automake\n");
			system("make $automakeMakeFlags") eq 0 or die ("failed to make automake\n");
			system("make install");
			chdir("$monoroot") eq 1 or die ("failed to chdir to $monoroot\n");
		}

		if (!(-d "$libtoolDir"))
		{
			chdir("$externalBuildDeps") eq 1 or die ("failed to chdir to external directory\n");
			system("tar xzf libtool-$libtoolVersion.tar.gz") eq 0  or die ("failed to extract libtool\n");

			chdir("$libtoolDir") eq 1 or die ("failed to chdir to libtool directory\n");
			system("./configure --prefix=$builtToolsDir") eq 0 or die ("failed to configure libtool\n");
			system("make") eq 0 or die ("failed to make libtool\n");
			system("make install") eq 0 or die ("failed to make install libtool\n");

			chdir("$monoroot") eq 1 or die ("failed to chdir to $monoroot\n");
		}

		$ENV{'LIBTOOLIZE'} = "$builtToolsDir/bin/libtoolize";
		$ENV{'LIBTOOL'} = "$builtToolsDir/bin/libtool";
	}

	my $macSdkPath = "";
	my $macversion = '10.8';
	my $darwinVersion = "10";
	if ($^O eq 'darwin')
	{
		if ($sdk eq '')
		{
			$sdk='10.11';
		}

		my $macBuildEnvDir = "$externalBuildDeps/MacBuildEnvironment";
		$macSdkPath = "$macBuildEnvDir/builds/MacOSX$sdk.sdk";
		if (! -d $macSdkPath)
		{
			print(">>> Unzipping mac build toolchain\n");
			system("$externalBuildDeps/unzip", '-qd', "$macBuildEnvDir", "$macBuildEnvDir/builds.zip") eq 0 or die ("failed unzipping mac build toolchain\n");
		}
	}

	if ($iphone || $iphoneSimulator)
	{
		if ($runningOnWindows)
		{
			die("This build is not supported on Windows\n");
		}

		my $iosBuildEnvDir = "$externalBuildDeps/iOSBuildEnvironment";
		my $iosXcodeDefaultToolchainRoot = "$iosBuildEnvDir/builds/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain";

		if (! -d "$iosBuildEnvDir/builds")
		{
			print(">>> Unzipping ios build toolchain\n");
			system("$externalBuildDeps/unzip", '-qd', "$iosBuildEnvDir/builds", "$iosBuildEnvDir/builds.zip") eq 0 or die ("failed unzipping ios build toolchain\n");
		}

		$ENV{PATH} = "$iosXcodeDefaultToolchainRoot/usr/bin:$iosBuildEnvDir/builds/Xcode.app/Contents/Developer/usr/bin:$ENV{PATH}";
		# Need to keep our libtool in front
		$ENV{PATH} = "$externalBuildDeps/built-tools/bin:$ENV{PATH}";

		push @configureparams, "--cache-file=iphone-$iphoneArch.cache" if ($enableCacheFile);

		my $iosMinimalCommon = "com,remoting,shared_perfcounters,appdomains";
		my $iosCFlagsCommon = "-DMONOTOUCH -DHOST_IOS -DDISABLE_POLICY_EVIDENCE=1 -DDISABLE_PROCESS_HANDLING=1";

		push @configureparams, "--with-tls=pthread";
		push @configureparams, "--without-ikvm-native";
		push @configureparams, "--disable-executables";
		push @configureparams, "--disable-visibility-hidden";

		if ($iphone)
		{
			my $iosSdkVersion = "9.3";
			my $iphoneOsMinVersion = "3.0";
			my $iosSdkRoot = "$iosBuildEnvDir/builds/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS$iosSdkVersion.sdk";

			print(">>> iOS Build Environment = $iosBuildEnvDir\n");
			print(">>> iOS SDK Version = $iosSdkVersion\n");
			print(">>> iOS SDK Root = $iosSdkRoot\n");
			print(">>> iPhone Arch = $iphoneArch\n");

			$ENV{PATH} = "$iosSdkRoot/usr/bin:$ENV{PATH}";

			$ENV{C_INCLUDE_PATH} = "$iosSdkRoot/usr/include";
			$ENV{CPLUS_INCLUDE_PATH} = "$iosSdkRoot/usr/include";

			$ENV{CC} = "$iosBuildEnvDir/builds/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang -arch $iphoneArch";
			$ENV{CXX} = "$iosBuildEnvDir/builds/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang++ -arch $iphoneArch";
			$ENV{LD} = "$iosBuildEnvDir/builds/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/ld";

			$ENV{CFLAGS} = "$iosCFlagsCommon -gdwarf-2 -DSMALL_CONFIG -DHAVE_LARGE_FILE_SUPPORT=1 -DHAVE_ARMV6=1 -DARM_FPU_VFP=1 -Wl,-application_extension -miphoneos-version-min=$iphoneOsMinVersion -mno-thumb -Os -isysroot $iosSdkRoot";

			# Unity defines
			$ENV{CFLAGS} = "-DPLATFORM_IPHONE $ENV{CFLAGS}";

			$ENV{CXXFLAGS} = "$ENV{CFLAGS} -U__powerpc__ -U__i386__ -D__arm__";
			$ENV{CPPFLAGS} = $ENV{CXXFLAGS};

			$ENV{LDFLAGS} = "-arch $iphoneArch -liconv -lobjc -lc++ -Wl,-syslibroot,$iosSdkRoot";

			print "\n";
			print ">>> Environment:\n";
			print ">>> \tCC = $ENV{CC}\n";
			print ">>> \tCXX = $ENV{CXX}\n";
			print ">>> \tLD = $ENV{LD}\n";
			print ">>> \tCFLAGS = $ENV{CFLAGS}\n";
			print ">>> \tCXXFLAGS = $ENV{CXXFLAGS}\n";
			print ">>> \tCPPFLAGS = $ENV{CPPFLAGS}\n";
			print ">>> \tLDFLAGS = $ENV{LDFLAGS}\n";
			print ">>> \tCPLUS_INCLUDE_PATH = $ENV{CPLUS_INCLUDE_PATH}\n";
			print ">>> \tC_INCLUDE_PATH = $ENV{C_INCLUDE_PATH}\n";

			push @configureparams, "--host=arm-apple-darwin$darwinVersion";

			push @configureparams, "--with-sigaltstack=no";
			push @configureparams, "--disable-shared-handles";
			push @configureparams, "--with-monotouch";

			push @configureparams, "--enable-llvm-runtime";
			push @configureparams, "--with-bitcode=yes";

			push @configureparams, "--with-lazy-gc-thread-creation=yes";
			push @configureparams, "--enable-icall-export";
			push @configureparams, "--enable-dtrace=no";

			push @configureparams, "--enable-minimal=$iosMinimalCommon,ssa,jit,reflection_emit_save,reflection_emit,portability,assembly_remapping,attach,verifier,full_messages,security,sgen_remset,sgen_marksweep_par,sgen_marksweep_fixed,sgen_marksweep_fixed_par,sgen_copying,logging";

			push @configureparams, "mono_cv_uscore=yes";
			push @configureparams, "cv_mono_sizeof_sunpath=104";
			push @configureparams, "ac_cv_func_posix_getpwuid_r=yes";
			push @configureparams, "ac_cv_func_backtrace_symbols=no";
			push @configureparams, "ac_cv_func_finite=no";
			push @configureparams, "ac_cv_header_curses_h=no";
		}
		elsif ($iphoneSimulator)
		{
			my $iosSdkVersion = "9.3";
			my $iosSimMinVersion = "4.3";
			my $iosSdkRoot = "$iosBuildEnvDir/builds/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator$iosSdkVersion.sdk";

			print(">>> iOS Sim Build Environment = $iosBuildEnvDir\n");
			print(">>> iOS Sim SDK Version = $iosSdkVersion\n");
			print(">>> iOS Sim SDK Root = $iosSdkRoot\n");
			print(">>> iOS Sim Arch = $iphoneSimulatorArch\n");

			$ENV{PATH} = "$iosSdkRoot/usr/bin:$ENV{PATH}";

			$ENV{MACSDKOPTIONS} = "$iosCFlagsCommon -D_XOPEN_SOURCE=1 -g -O0 -DTARGET_IPHONE_SIMULATOR -mios-simulator-version-min=$iosSimMinVersion -isysroot $iosSdkRoot";
			$ENV{CFLAGS} = "-arch $iphoneSimulatorArch $ENV{MACSDKOPTIONS}";
			$ENV{CXXFLAGS} = "$ENV{CFLAGS}";
			$ENV{CPPFLAGS} = "$ENV{CFLAGS}";
			$ENV{CC} = "$iosBuildEnvDir/builds/Xcode.app/Contents/Developer/usr/bin/gcc";
			$ENV{CXX} = "$iosBuildEnvDir/builds/Xcode.app/Contents/Developer/usr/bin/g++";

			print "\n";
			print ">>> Environment:\n";
			print ">>> \tCC = $ENV{CC}\n";
			print ">>> \tCXX = $ENV{CXX}\n";
			print ">>> \tLD = $ENV{LD}\n";
			print ">>> \tCFLAGS = $ENV{CFLAGS}\n";
			print ">>> \tCXXFLAGS = $ENV{CXXFLAGS}\n";
			print ">>> \tCPPFLAGS = $ENV{CPPFLAGS}\n";
			print ">>> \tMACSDKOPTIONS = $ENV{MACSDKOPTIONS}\n";

			push @configureparams, "--host=$iphoneSimulatorArch-apple-darwin$darwinVersion";
			push @configureparams, "--enable-minimal=$iosMinimalCommon";

			push @configureparams, "mono_cv_uscore=yes";
			push @configureparams, "ac_cv_func_clock_nanosleep=no";
		}
		else
		{
			die("This should not be hit\n");
		}
	}
	elsif ($iphoneCross)
	{
		if ($runningOnWindows)
		{
			die("Not implemented\n");
		}
		else
		{
			$ENV{CFLAGS} = "-DMONOTOUCH -DARM_FPU_VFP=1 -DUSE_MUNMAP -DPLATFORM_IPHONE_XCOMP  -mmacosx-version-min=$macversion";
			$ENV{CXXFLAGS} = "-mmacosx-version-min=$macversion -stdlib=libc++";
			$ENV{CPPFLAGS} = "$ENV{CFLAGS} -mmacosx-version-min=$macversion";

			$ENV{CC} = "$macSdkPath/../usr/bin/clang -arch i386";
			$ENV{CXX} = "$macSdkPath/../usr/bin/clang++ -arch i386";
			$ENV{CPP} = "$ENV{CC} -E";
			$ENV{LD} = $ENV{CC};
			$ENV{LDFLAGS} = "-stdlib=libc++";
			$ENV{MACSDKOPTIONS} = "-mmacosx-version-min=$macversion -isysroot $macSdkPath";

			print "\n";
			print ">>> Environment:\n";
			print ">>> \tCC = $ENV{CC}\n";
			print ">>> \tCXX = $ENV{CXX}\n";
			print ">>> \tLD = $ENV{LD}\n";
			print ">>> \tCFLAGS = $ENV{CFLAGS}\n";
			print ">>> \tCXXFLAGS = $ENV{CXXFLAGS}\n";
			print ">>> \tCPPFLAGS = $ENV{CPPFLAGS}\n";
			print ">>> \tLDFLAGS = $ENV{LDFLAGS}\n";
			print ">>> \tMACSDKOPTIONS = $ENV{MACSDKOPTIONS}\n";

			push @configureparams, "--cache-file=iphone-cross.cache" if ($enableCacheFile);

			push @configureparams, "--with-sigaltstack=no";
			push @configureparams, "--disable-shared-handles";
			push @configureparams, "--with-tls=pthread";

			push @configureparams, "--target=arm-darwin";
			push @configureparams, "--with-macversion=$macversion";
			push @configureparams, "--with-cross-offsets=$iphoneCrossAbi.h";

			push @configureparams, "--build=i386-apple-darwin10";
			push @configureparams, "--disable-libraries";
			push @configureparams, "--enable-icall-symbol-map";
			push @configureparams, "--enable-minimal=com,remoting";

			#push @configureparams, "--enable-llvm";
			#push @configureparams, "--with-llvm=llvm/usr";

			my @mcsArgs = ();
			push @mcsArgs, "$monoroot/tools/offsets-tool/MonoAotOffsetsDumper.cs";
			push @mcsArgs, "$monoroot/mcs/class/Mono.Options/Mono.Options/Options.cs";
			push @mcsArgs, "/r:$externalBuildDeps/CppSharpBinaries/CppSharp.AST.dll";
			push @mcsArgs, "/r:$externalBuildDeps/CppSharpBinaries/CppSharp.Generator.dll";
			push @mcsArgs, "/r:$externalBuildDeps/CppSharpBinaries/CppSharp.Parser.CSharp.dll";
			push @mcsArgs, "/r:$externalBuildDeps/CppSharpBinaries/CppSharp.Parser.dll";
			push @mcsArgs, "/r:$externalBuildDeps/CppSharpBinaries/CppSharp.dll";
			push @mcsArgs, "/debug";
			push @mcsArgs, "/nowarn:0436";
			push @mcsArgs, "/out:$monoroot/tools/offsets-tool/MonoAotOffsetsDumper.exe";

			print ">>> Compiling MonoAotOffsetDumper : $iphoneCrossMonoBinToUse/mcs @mcsArgs\n";
			system("$iphoneCrossMonoBinToUse/mcs", @mcsArgs) eq 0 or die("failed to compile MonoAotOffsetsDumper\n");

			# clean up any pre-existing offset header just in case
			if (-f "$monoroot/$iphoneCrossAbi.h")
			{
				system("rm", "-rf", "$iphoneCrossAbi.h");
			}
		}
	}
	elsif ($android)
	{
		if (!(-d $externalBuildDeps))
		{
			die("mono build deps are required and the directory was not found : $externalBuildDeps\n");
		}

		my $ndkVersion = "r13b";
		my $isArmArch = 1;
		my $toolchainName = "";
		my $platformRootPostfix = "";
		my $useKraitPatch = 1;
		my $kraitPatchPath = "$monoroot/../../android_krait_signal_handler/build";
		my $toolChainExtension = "";

		$isArmArch = 0 if ($androidArch eq "x86");

		$ENV{ANDROID_PLATFORM} = "android-9";
		$ENV{GCC_VERSION} = "4.9";

		if ($isArmArch)
		{
			$ENV{GCC_PREFIX} = "arm-linux-androideabi-";
			$toolchainName = "$ENV{GCC_PREFIX}$ENV{GCC_VERSION}";
			$platformRootPostfix = "arm";
		}
		else
		{
			$ENV{GCC_PREFIX} = "i686-linux-android-";
			$toolchainName = "x86-$ENV{GCC_VERSION}";
			$platformRootPostfix = "x86";
			$useKraitPatch = 0;
		}

		if ($^O eq "linux")
		{
			$ENV{HOST_ENV} = "linux";
		}
		elsif ($^O eq 'darwin')
		{
			$ENV{HOST_ENV} = "darwin";
		}
		else
		{
			$ENV{HOST_ENV} = "windows";
		}

		print "\n";
		print(">>> Android Platform = $ENV{ANDROID_PLATFORM}\n");
		print(">>> Android NDK Version = $ndkVersion\n");
		print(">>> Android GCC Prefix = $ENV{GCC_PREFIX}\n");
		print(">>> Android GCC Version = $ENV{GCC_VERSION}\n");

		my $ndkName = "";
		if($^O eq "linux")
		{
			$ndkName = "android-ndk-$ndkVersion-linux-x86_64.zip";
		}
		elsif($^O eq "darwin")
		{
			$ndkName = "android-ndk-$ndkVersion-darwin-x86_64.zip";
		}
		else
		{
			$ndkName = "android-ndk-$ndkVersion-windows-x86.zip";
		}

		my $depsNdkArchive = "$externalBuildDeps/$ndkName";
		my $depsNdkFinal = "$externalBuildDeps/android-ndk-$ndkVersion";

		print(">>> Android NDK Archive = $depsNdkArchive\n");
		print(">>> Android NDK Extraction Destination = $depsNdkFinal\n");
		print("\n");

		$ENV{ANDROID_NDK_ROOT} = "$depsNdkFinal";

		if (-d $depsNdkFinal)
		{
			print(">>> Android NDK already extracted\n");
		}
		else
		{
			print(">>> Android NDK needs to be extracted\n");

			if ($runningOnWindows)
			{
				my $sevenZip = "$externalBuildDeps/7z/win64/7za.exe";
				my $winDepsNdkArchive = `cygpath -w $depsNdkArchive`;
				my $winDepsNdkExtract = `cygpath -w $externalBuildDeps`;

				# clean up trailing new lines that end up in the output from cygpath.  If left, they cause problems down the line
				# for 7zip
				$winDepsNdkArchive =~ s/\n+$//;
				$winDepsNdkExtract =~ s/\n+$//;

				system($sevenZip, "x", "$winDepsNdkArchive", "-o$winDepsNdkExtract");
			}
			else
			{
				my ($name,$path,$suffix) = fileparse($depsNdkArchive, qr/\.[^.]*/);

				print(">>> Android NDK Extension = $suffix\n");

				# Versions after r11 use .zip extension.  Currently we use r10e, but let's support the .zip extension in case
				# we upgrade down the road
				if (lc $suffix eq '.zip')
				{
					system("unzip", "-q", $depsNdkArchive, "-d", $externalBuildDeps);
				}
				elsif (lc $suffix eq '.bin')
				{	chmod(0755, $depsNdkArchive);
					system($depsNdkArchive, "-o$externalBuildDeps");
				}
				else
				{
					die "Unknown file extension '" . $suffix . "'\n";
				}
			}
		}

		if (!(-f "$ENV{ANDROID_NDK_ROOT}/ndk-build"))
		{
			die("Something went wrong with the NDK extraction\n");
		}

		my $androidNdkRoot = $ENV{ANDROID_NDK_ROOT};
		my $androidPlatformRoot = "$androidNdkRoot/platforms/$ENV{ANDROID_PLATFORM}/arch-$platformRootPostfix";
		my $androidToolchain = "$androidNdkRoot/toolchains/$toolchainName/prebuilt/$ENV{HOST_ENV}";

		if (!(-d "$androidToolchain"))
		{
			if (-d "$androidToolchain-x86")
			{
				$androidToolchain = "$androidToolchain-x86";
			}
			else
			{
				$androidToolchain = "$androidToolchain-x86_64";
			}
		}

		if ($runningOnWindows)
		{
			$toolChainExtension = ".exe";

			$androidPlatformRoot = `cygpath -w $androidPlatformRoot`;
			# clean up trailing new lines that end up in the output from cygpath.
			$androidPlatformRoot =~ s/\n+$//;
			# Switch over to forward slashes.  They propagate down the toolchain correctly
			$androidPlatformRoot =~ s/\\/\//g;

			# this will get passed as a path to the linker, so we need to windows-ify the path
			$kraitPatchPath = `cygpath -w $kraitPatchPath`;
			$kraitPatchPath =~ s/\n+$//;
			$kraitPatchPath =~ s/\\/\//g;
		}

		print(">>> Android Arch = $androidArch\n");
		print(">>> Android NDK Root = $androidNdkRoot\n");
		print(">>> Android Platform Root = $androidPlatformRoot\n");
		print(">>> Android Toolchain = $androidToolchain\n");

		if (!(-d "$androidToolchain"))
		{
			die("Failed to locate android toolchain\n");
		}

		if (!(-d "$androidPlatformRoot"))
		{
			die("Failed to locate android platform root\n");
		}

		if ("$androidArch" eq 'armv5')
		{
			$ENV{CFLAGS} = "-DARM_FPU_NONE=1 -march=armv5te -mtune=xscale -msoft-float";
		}
		elsif ("$androidArch" eq 'armv6_vfp')
		{
			$ENV{CFLAGS} = "-DARM_FPU_VFP=1  -march=armv6 -mtune=xscale -msoft-float -mfloat-abi=softfp -mfpu=vfp -DHAVE_ARMV6=1";
		}
		elsif ("$androidArch" eq 'armv7a')
		{
			$ENV{CFLAGS} = "-DARM_FPU_VFP=1  -march=armv7-a -mfloat-abi=softfp -mfpu=vfp -DHAVE_ARMV6=1";
			$ENV{LDFLAGS} = "-Wl,--fix-cortex-a8";
		}
		elsif ("$androidArch" eq 'x86')
		{
			$ENV{LDFLAGS} = "-lgcc"
		}
		else
		{
			die("Unsupported android arch : $androidArch\n");
		}

		if ($isArmArch)
		{
			$ENV{CFLAGS} = "-funwind-tables $ENV{CFLAGS}";
			$ENV{LDFLAGS} = "-Wl,-rpath-link=$androidPlatformRoot/usr/lib $ENV{LDFLAGS}";
		}

		$ENV{PATH} = "$androidToolchain/bin:$ENV{PATH}";
		$ENV{CC} = "$androidToolchain/bin/$ENV{GCC_PREFIX}gcc$toolChainExtension --sysroot=$androidPlatformRoot";
		$ENV{CXX} = "$androidToolchain/bin/$ENV{GCC_PREFIX}g++$toolChainExtension --sysroot=$androidPlatformRoot";
		$ENV{CPP} = "$androidToolchain/bin/$ENV{GCC_PREFIX}cpp$toolChainExtension";
		$ENV{CXXCPP} = "$androidToolchain/bin/$ENV{GCC_PREFIX}cpp$toolChainExtension";
		$ENV{CPATH} = "$androidPlatformRoot/usr/include";
		$ENV{LD} = "$androidToolchain/bin/$ENV{GCC_PREFIX}ld$toolChainExtension";
		$ENV{AS} = "$androidToolchain/bin/$ENV{GCC_PREFIX}as$toolChainExtension";
		$ENV{AR} = "$androidToolchain/bin/$ENV{GCC_PREFIX}ar$toolChainExtension";
		$ENV{RANLIB} = "$androidToolchain/bin/$ENV{GCC_PREFIX}ranlib$toolChainExtension";
		$ENV{STRIP} = "$androidToolchain/bin/$ENV{GCC_PREFIX}strip$toolChainExtension";

		$ENV{CFLAGS} = "-DANDROID -DPLATFORM_ANDROID -DLINUX -D__linux__ -DHAVE_USR_INCLUDE_MALLOC_H -DPAGE_SIZE=0x1000 -D_POSIX_PATH_MAX=256 -DS_IWRITE=S_IWUSR -DHAVE_PTHREAD_MUTEX_TIMEDLOCK -fpic -g -ffunction-sections -fdata-sections $ENV{CFLAGS}";
		$ENV{CXXFLAGS} = $ENV{CFLAGS};
		$ENV{CPPFLAGS} = $ENV{CFLAGS};

		if ($useKraitPatch)
		{
			$ENV{LDFLAGS} = "-Wl,--wrap,sigaction -L$kraitPatchPath/obj/local/armeabi -lkrait-signal-handler $ENV{LDFLAGS}";
		}

		$ENV{LDFLAGS} = "-Wl,--no-undefined -Wl,--gc-sections -ldl -lm -llog -lc $ENV{LDFLAGS}";

		print "\n";
		print ">>> Environment:\n";
		print ">>> \tCC = $ENV{CC}\n";
		print ">>> \tCXX = $ENV{CXX}\n";
		print ">>> \tCPP = $ENV{CPP}\n";
		print ">>> \tCXXCPP = $ENV{CXXCPP}\n";
		print ">>> \tCPATH = $ENV{CPATH}\n";
		print ">>> \tLD = $ENV{LD}\n";
		print ">>> \tAS = $ENV{AS}\n";
		print ">>> \tAR = $ENV{AR}\n";
		print ">>> \tRANLIB = $ENV{RANLIB}\n";
		print ">>> \tSTRIP = $ENV{STRIP}\n";
		print ">>> \tCFLAGS = $ENV{CFLAGS}\n";
		print ">>> \tCXXFLAGS = $ENV{CXXFLAGS}\n";
		print ">>> \tCPPFLAGS = $ENV{CPPFLAGS}\n";
		print ">>> \tLDFLAGS = $ENV{LDFLAGS}\n";

		if ($useKraitPatch)
		{
			my $kraitPatchRepo = "git://github.com/Unity-Technologies/krait-signal-handler.git";
			if (-d "$kraitPatchPath")
			{
				print ">>> Krait patch repository already cloned\n";
			}
			else
			{
				system("git", "clone", "--branch", "master", "$kraitPatchRepo", "$kraitPatchPath") eq 0 or die ('failing cloning Krait patch');
			}

			chdir("$kraitPatchPath") eq 1 or die ("failed to chdir to krait patch directory\n");
			system('$ANDROID_NDK_ROOT/ndk-build clean') eq 0 or die ('failing to clean Krait patch');
			system('$ANDROID_NDK_ROOT/ndk-build') eq 0 or die ('failing to build Krait patch');
			chdir("$monoroot") eq 1 or die ("failed to chdir to $monoroot\n");
		}

		if ($isArmArch)
		{
			push @configureparams, "--host=armv5-linux-androideabi";
		}
		elsif ("$androidArch" eq 'x86')
		{
			push @configureparams, "--host=i686-linux-android";
		}
		else
		{
			die("Unsupported android arch : $androidArch\n");
		}

		push @configureparams, "--cache-file=android-$androidArch.cache" if ($enableCacheFile);

		push @configureparams, "--disable-parallel-mark";
		push @configureparams, "--disable-shared-handles";
		push @configureparams, "--with-sigaltstack=no";
		push @configureparams, "--with-tls=pthread";
		push @configureparams, "--disable-visibility-hidden";
		push @configureparams, "mono_cv_uscore=yes";
		push @configureparams, "ac_cv_header_zlib_h=no" if($runningOnWindows);
	}
	elsif ($tizen)
	{
		if (!(-d $externalBuildDeps))
		{
			die("mono build deps are required and the directory was not found : $externalBuildDeps\n");
		}

		my $sdkVersion = "2.4.0r1";
		my $isArmArch = 1;

		$isArmArch = 0 if ($tizenEmulator);

		$ENV{TIZEN_PLATFORM} = "tizen-2.4";

		if ($tizenEmulator)
		{
			$ENV{TIZEN_ROOTSTRAP} = "mobile-2.4-emulator.core";
		}
		else
		{
			$ENV{TIZEN_ROOTSTRAP} = "mobile-2.4-device.core";
		}

		if ($^O eq "linux")
		{
			$ENV{HOST_ENV} = "linux";
		}
		elsif ($^O eq 'darwin')
		{
			$ENV{HOST_ENV} = "darwin";
		}
		else
		{
			$ENV{HOST_ENV} = "windows";
		}

		print "\n";
		print(">>> Tizen Platform = $ENV{TIZEN_PLATFORM}\n");
		print(">>> Tizen SDK Version = $sdkVersion\n");

		my $sdkName = "tizen-$sdkVersion-$ENV{HOST_ENV}.tar.bz2";
		my $depsSdkArchive = "$externalBuildDeps/$sdkName";
		my $depsSdkFinal = "$externalBuildDeps/tizen-$sdkVersion-$ENV{HOST_ENV}";

		print(">>> Tizen SDK Archive = $depsSdkArchive\n");
		print(">>> Tizen SDK Extraction Destination = $depsSdkFinal\n");
		print("\n");

		$ENV{TIZEN_SDK_ROOT} = "$depsSdkFinal";

		if (-d $depsSdkFinal)
		{
			print(">>> Tizen SDK already extracted\n");
		}
		else
		{
			print(">>> Tizen SDK needs to be extracted\n");

			if ($runningOnWindows)
			{
				my $sevenZip = "$externalBuildDeps/7z/win64/7za.exe";
				my $winDepsSdkArchive = `cygpath -w $depsSdkArchive`;
				my $winDepsSdkExtract = `cygpath -w $externalBuildDeps`;

				# clean up trailing new lines that end up in the output from cygpath.  If left, they cause problems down the line
				# for 7zip
				$winDepsSdkArchive =~ s/\n+$//;
				$winDepsSdkExtract =~ s/\n+$//;

				system($sevenZip, "x", "$winDepsSdkArchive", "-o$winDepsSdkExtract");
			}
			else
			{
				my ($name,$path,$suffix) = fileparse($depsSdkArchive, qr/\.[^.]*/);

				print(">>> Tizen SDK Extension = $suffix\n");

				if (lc $suffix eq '.bz2')
				{	chmod(0755, $depsSdkArchive);
					system("tar xjf $depsSdkArchive -C $externalBuildDeps") eq 0  or die ("failed to extract Tizen SDK\n");
				}
				else
				{
					die "Unknown file extension '" . $suffix . "'\n";
				}
			}
		}

		if (!(-f "$ENV{TIZEN_SDK_ROOT}/tools/sdb"))
		{
			die("Something went wrong with the SDK extraction\n");
		}

		my $tizenSdkRoot = $ENV{TIZEN_SDK_ROOT};
		my $tizenPlatformRoot = "$tizenSdkRoot/platforms/$ENV{TIZEN_PLATFORM}/mobile/rootstraps/$ENV{TIZEN_ROOTSTRAP}";
		my $tizenToolchain = "$tizenSdkRoot/tools/llvm-3.6/bin";

		if ($runningOnWindows)
		{
			$toolChainExtension = ".exe";

			$tizenPlatformRoot = `cygpath -w $tizenPlatformRoot`;
			# clean up trailing new lines that end up in the output from cygpath.
			$tizenPlatformRoot =~ s/\n+$//;
			# Switch over to forward slashes.  They propagate down the toolchain correctly
			$tizenPlatformRoot =~ s/\\/\//g;
		}

		if ($tizenEmulator)
		{
			$tizenToolchain = "$tizenSdkRoot/tools/i386-linux-gnueabi-gcc-4.9/bin/i386";
			$ENV{CFLAGS} = "-Os -g -march=i686 -msse2 -mfpmath=sse";
		}
		else
		{
			$tizenToolchain = "$tizenSdkRoot/tools/arm-linux-gnueabi-gcc-4.9/bin/arm";
			$ENV{CFLAGS} = "-Os -g -march=armv7-a -mfpu=vfp -mfloat-abi=softfp -DARM_FPU_VFP=1 -DHAVE_ARMV6=1";
			$ENV{LDFLAGS} = "-Wl,-rpath-link=$tizenPlatformRoot/usr/lib -L$tizenPlatformRoot/usr/lib $ENV{LDFLAGS}";
		}

		print(">>> Tizen SDK Root = $tizenSdkRoot\n");
		print(">>> Tizen Platform Root = $tizenPlatformRoot\n");
		print(">>> Tizen Toolchain Prefix = $tizenToolchain\n");

		if (!(-d "$tizenPlatformRoot"))
		{
			die("Failed to locate Tizen platform root\n");
		}

		$ENV{PATH} = "$tizenToolchain/bin:$ENV{PATH}";
		$ENV{CC} = "$tizenToolchain-linux-gnueabi-gcc --sysroot=$tizenPlatformRoot";
		$ENV{CXX} = "$tizenToolchain-linux-gnueabi-g++ --sysroot=$tizenPlatformRoot";
		$ENV{CPP} = "$tizenToolchain-linux-gnueabi-cpp";
		$ENV{CXXCPP} = "$tizenToolchain-linux-gnueabi-cpp";
		$ENV{CPATH} = "$tizenPlatformRoot/usr/include";
		$ENV{LD} = "$tizenToolchain-linux-gnueabi-ld --sysroot=$tizenPlatformRoot";
		$ENV{AS} = "$tizenToolchain-linux-gnueabi-as";
		$ENV{STRIP} = "$tizenToolchain-linux-gnueabi-strip";

		if ($tizenEmulator)
		{
			$ENV{AR} = "$ENV{TIZEN_SDK_ROOT}/tools/i386-linux-gnueabi-gcc-4.9/bin/i386-linux-gnueabi-ar";
			$ENV{RANLIB} = "$ENV{TIZEN_SDK_ROOT}/tools/i386-linux-gnueabi-gcc-4.9/bin/i386-linux-gnueabi-ranlib";
		}
		else
		{
			$ENV{AR} = "$ENV{TIZEN_SDK_ROOT}/tools/arm-linux-gnueabi-gcc-4.9/bin/arm-linux-gnueabi-ar";
			$ENV{RANLIB} = "$ENV{TIZEN_SDK_ROOT}/tools/arm-linux-gnueabi-gcc-4.9/bin/arm-linux-gnueabi-ranlib";
		}

		$ENV{CFLAGS} = "-DTIZEN -DLINUX -D__linux__ -DHAVE_USR_INCLUDE_MALLOC_H -DPAGE_SIZE=0x1000 -D_POSIX_PATH_MAX=256 -DS_IWRITE=S_IWUSR -DHAVE_PTHREAD_MUTEX_TIMEDLOCK -fpic -g -ffunction-sections -fdata-sections $ENV{CFLAGS}";
		$ENV{CXXFLAGS} = $ENV{CFLAGS};
		$ENV{CPPFLAGS} = $ENV{CFLAGS};
		$ENV{LDFLAGS} = "-Wl,--no-undefined -ldlog -shared -Xlinker --as-needed $ENV{LDFLAGS}";

		print "\n";
		print ">>> Environment:\n";
		print ">>> \tCC = $ENV{CC}\n";
		print ">>> \tCXX = $ENV{CXX}\n";
		print ">>> \tCPP = $ENV{CPP}\n";
		print ">>> \tCXXCPP = $ENV{CXXCPP}\n";
		print ">>> \tCPATH = $ENV{CPATH}\n";
		print ">>> \tLD = $ENV{LD}\n";
		print ">>> \tAS = $ENV{AS}\n";
		print ">>> \tAR = $ENV{AR}\n";
		print ">>> \tRANLIB = $ENV{RANLIB}\n";
		print ">>> \tSTRIP = $ENV{STRIP}\n";
		print ">>> \tCFLAGS = $ENV{CFLAGS}\n";
		print ">>> \tCXXFLAGS = $ENV{CXXFLAGS}\n";
		print ">>> \tCPPFLAGS = $ENV{CPPFLAGS}\n";
		print ">>> \tLDFLAGS = $ENV{LDFLAGS}\n";

		if ($tizenEmulator)
		{
			push @configureparams, "--host=i386-tizen-linux-gnueabi";
		}
		else
		{
			push @configureparams, "--host=arm-tizen-linux-gnueabi";
		}

		push @configureparams, "--cache-file=tizen-cross.cache" if ($enableCacheFile);
		push @configureparams, "--disable-parallel-mark";
		push @configureparams, "--disable-shared-handles";
		push @configureparams, "--with-sigaltstack=no";
		push @configureparams, "--with-tls=pthread";
		push @configureparams, "--disable-visibility-hidden";
		push @configureparams, "--disable-executables";
		push @configureparams, "--with-gnu-ld=yes";
		push @configureparams, "mono_cv_uscore=yes";
		push @configureparams, "ac_cv_header_zlib_h=no" if($runningOnWindows);
	}
	elsif($^O eq "linux")
	{
		if (!(-d $externalBuildDeps))
		{
			die("mono build deps are required and the directory was not found : $externalBuildDeps\n");
		}

		if($ENV{UNITY_THISISABUILDMACHINE} || $ENV{UNITY_USE_LINUX_SDK})
		{
			my $sdkVersion = '20170609';
			my $schroot = "LinuxBuildEnvironment-$sdkVersion";
			my @linuxToolchain = ('schroot', '-c', $schroot, '--');

			print "\n";
			print(">>> Linux SDK Version = $sdkVersion\n");

			my $sdkName = "linux-sdk-$sdkVersion.tar.bz2";
			my $depsSdkArchive = "$externalBuildDeps/$sdkName";
			my $depsSdkFinal = "$externalBuildDeps/linux-sdk-$sdkVersion";

			print(">>> Linux SDK Archive = $depsSdkArchive\n");
			print(">>> Linux SDK Extraction Destination = $depsSdkFinal\n");
			print("\n");

			my $linuxSdkRoot = $depsSdkFinal;

			if (-d $depsSdkFinal)
			{
				print(">>> Linux SDK already extracted\n");
			}
			else
			{
				print(">>> Linux SDK needs to be extracted\n");
				system('mkdir', '-p', $depsSdkFinal);
				system('tar', 'xaf', $depsSdkArchive, '-C', $depsSdkFinal) eq 0  or die ("failed to extract Linux SDK\n");
				system('sudo', 'cp', '-R', "$depsSdkFinal/linux-sdk-$sdkVersion", '/etc/schroot');
				system("sed 's,^directory=.*,directory=$depsSdkFinal/$schroot,' \"$depsSdkFinal/$schroot.conf\" | sudo tee /etc/schroot/chroot.d/$schroot.conf") eq 0 or die ("failed to deploy Linux SDK\n");
			}

			@commandPrefix = @linuxToolchain;
			print(">>> Linux SDK Root = $linuxSdkRoot\n");
			print(">>> Linux Toolchain Command Prefix = " . join(' ', @commandPrefix) . "\n");
		}

		push @configureparams, "--host=$monoHostArch-pc-linux-gnu";

		push @configureparams, "--disable-parallel-mark";  #this causes crashes

		my $archflags = '';
		if ($arch32)
		{
			$archflags = '-m32';
		}
		else
		{
			$archflags = '-fPIC';
		}

		if ($debug)
		{
			$ENV{CFLAGS} = "$archflags -g -O0";
		}
		else
		{
			$ENV{CFLAGS} = "$archflags -Os";  #optimize for size
		}
	}
	elsif($^O eq 'darwin')
	{
		# Set up mono for bootstrapping
		if ($existingMonoRootPath eq "")
		{
			# Find the latest mono version and use that for boostrapping
			my $monoInstalls = '/Library/Frameworks/Mono.framework/Versions';
			my @monoVersions = ();

			opendir( my $DIR, $monoInstalls );
			while ( my $entry = readdir $DIR )
			{
				next unless -d $monoInstalls . '/' . $entry;
				next if $entry eq '.' or $entry eq '..' or $entry eq 'Current';
				push @monoVersions, $entry;
			}
			closedir $DIR;
			@monoVersions = sort @monoVersions;
			my $monoVersionToUse = pop @monoVersions;
			$existingMonoRootPath = "$monoInstalls/$monoVersionToUse";
		}

		$mcs = "EXTERNAL_MCS=$existingMonoRootPath/bin/mcs";

		$ENV{'CC'} = "$macSdkPath/../usr/bin/clang";
		$ENV{'CXX'} = "$macSdkPath/../usr/bin/clang++";
		$ENV{'CFLAGS'} = $ENV{MACSDKOPTIONS} = "-D_XOPEN_SOURCE -I$macBuildEnvDir/builds/usr/include -mmacosx-version-min=$macversion -isysroot $macSdkPath";

		$ENV{CFLAGS} = "$ENV{CFLAGS} -g -O0" if $debug;
		$ENV{CFLAGS} = "$ENV{CFLAGS} -Os" if not $debug; #optimize for size

		$ENV{CC} = "$ENV{CC} -arch $monoHostArch";
		$ENV{CXX} = "$ENV{CXX} -arch $monoHostArch";

		# Add OSX specific autogen args
		push @configureparams, "--host=$monoHostArch-apple-darwin12.2.0";

		# Need to define because Apple's SIP gets in the way of us telling mono where to find this
		push @configureparams, "--with-libgdiplus=$addtoresultsdistdir/lib/libgdiplus.dylib";

		print "\n";
		print ">>> Setting environment:\n";
		print ">>> PATH = ".$ENV{PATH}."\n";
		print ">>> C_INCLUDE_PATH = ".$ENV{C_INCLUDE_PATH}."\n";
		print ">>> CPLUS_INCLUDE_PATH = ".$ENV{CPLUS_INCLUDE_PATH}."\n";
		print ">>> CFLAGS = ".$ENV{CFLAGS}."\n";
		print ">>> CXXFLAGS = ".$ENV{CXXFLAGS}."\n";
		print ">>> CC = ".$ENV{CC}."\n";
		print ">>> CXX = ".$ENV{CXX}."\n";
		print ">>> CPP = ".$ENV{CPP}."\n";
		print ">>> CXXPP = ".$ENV{CXXPP}."\n";
		print ">>> LD = ".$ENV{LD}."\n";
		print ">>> LDFLAGS = ".$ENV{LDFLAGS}."\n";
		print "\n";
	}
	else
	{
		push @configureparams, "--host=$monoHostArch-pc-mingw32";
	}

	if ($isDesktopBuild)
	{
		my $cacheArch = $arch32 ? "i386" : "x86_64";
		push @configureparams, "--cache-file=desktop-$cacheArch.cache" if ($enableCacheFile);
	}

	print ">>> Existing Mono : $existingMonoRootPath\n\n";
	$ENV{'PATH'} = "$existingMonoRootPath/bin:$ENV{'PATH'}";

	print ">>> PATH before Build = $ENV{PATH}\n\n";

	print(">>> mcs Information : \n");
	system(@commandPrefix, ("which", "mcs"));
	system(@commandPrefix, ("mcs", "--version"));
	print("\n");

	print ">>> Checking on some tools...\n";
	system(@commandPrefix, ("which", "autoconf"));
	system(@commandPrefix, ("autoconf", "--version"));

	system(@commandPrefix, ("which", "texi2dvi"));
	system(@commandPrefix, ("texi2dvi", "--version"));

	system(@commandPrefix, ("which", "automake"));
	system(@commandPrefix, ("automake", "--version"));

	system(@commandPrefix, ("which", "libtool"));
	system(@commandPrefix, ("libtool", "--version"));

	system(@commandPrefix, ("which", "libtoolize"));
	system(@commandPrefix, ("libtoolize", "--version"));
	print("\n");

	print ">>> LIBTOOLIZE before Build = $ENV{LIBTOOLIZE}\n";
	print ">>> LIBTOOL before Build = $ENV{LIBTOOL}\n";

	chdir("$monoroot") eq 1 or die ("failed to chdir 2\n");

	if (not $skipMonoMake)
	{
		if ($clean)
		{
			if (!($mcsOnly))
			{
				print(">>> Cleaning $monoprefix\n");
				rmtree($monoprefix);
			}

			# Avoid "source directory already configured" ...
			system(@commandPrefix, ('rm', '-f', 'config.status', 'eglib/config.status', 'libgc/config.status'));

			print("\n>>> Calling autogen in mono\n");
			print("\n");
			print("\n>>> Configure parameters are : @configureparams\n");
			print("\n");

			system(@commandPrefix, ('./autogen.sh', @configureparams)) eq 0 or die ('failing autogenning mono');

			if ($mcsOnly)
			{
				print("\n>>> Calling make clean in mcs\n");
				chdir("$monoroot/mcs");
				system(@commandPrefix, ("make","clean")) eq 0 or die ("failed to make clean\n");
				chdir("$monoroot");
			}
			else
			{
				print("\n>>> Calling make clean in mono\n");
				system(@commandPrefix, ("make","clean")) eq 0 or die ("failed to make clean\n");
			}
		}

		# this step needs to run after configure
		if ($iphoneCross || $iphone || $android || $tizen)
		{
			# This step generates the arm_dpimacros.h file, which is needed by the offset dumper
			chdir("$monoroot/mono/arch/arm");
			system("make") eq 0 or die("failed to make in $monoroot/mono/arch/arm\n");
			chdir("$monoroot");
		}

		if ($iphoneCross)
		{
			my @monoArgs = ();
			push @monoArgs, "$monoroot/tools/offsets-tool/MonoAotOffsetsDumper.exe";
			push @monoArgs, "--abi";
			push @monoArgs, "$iphoneCrossAbi";
			push @monoArgs, "--out";
			push @monoArgs, "$monoroot";
			push @monoArgs, "--mono";
			push @monoArgs, "$monoroot";
			push @monoArgs, "--maccore";
			push @monoArgs, "$monoroot";

			$ENV{MONO_PATH} = "$externalBuildDeps/CppSharpBinaries";
			# Need to use 32bit mono because there is a native CppSharp dylib that will be used and there's only a 32bit version of it
			print ">>> Running MonoAotOffsetDumper : arch -i386 $iphoneCrossMonoBinToUse/mono @monoArgs\n";
			system("arch", "-i386", "$iphoneCrossMonoBinToUse/mono", @monoArgs) eq 0 or die("failed to run MonoAotOffsetsDumper\n");
		}

		if ($mcsOnly)
		{
			print("\n>>> Calling make in mcs\n");
			chdir("$monoroot/mcs");
			my @makeCommand = (@commandPrefix, ('make', "-j$jobs"));
			if($mcs ne '')
			{
				push(@makeCommand, $mcs);
			}
			system(@makeCommand) eq 0 or die ("Failed to make\n");
			chdir("$monoroot");
		}
		else
		{
			print("\n>>> Calling make\n");
			my @makeCommand = (@commandPrefix, ('make', "-j$jobs"));
			if($mcs ne '')
			{
				push(@makeCommand, $mcs);
			}
			system(@makeCommand) eq 0 or die ("Failed to make\n");
		}

		if ($isDesktopBuild)
		{
			print("\n>>> Calling make install\n");
			system(@commandPrefix, ('make', 'install')) eq 0 or die ("Failed to make install\n");
		}
		else
		{
			if ($disableMcs)
			{
				print(">>> Skipping make install.  We don't need to run this step when building the runtime on non-desktop platforms.\n");
			}
			else
			{
				# Note by Mike : make install on Windows for android runtime runs into more cygwin path issues.  The one I hit was related to ranlib.exe being passed cygwin linux paths
				# and as a result not being able to find stuff.  The previous build scripts didn't run make install for android or iOS, so I think we are fine to skip this step.
				# However, if we were to build the class libs for these cases, then we probably would need to run make install.  If that day comes, we'll have to figure out what to do here.
				print(">>> Attempting to build class libs for a non-desktop platform.  The `make install` step is probably needed, but it has cygwin path related problems on Windows for android\n");
				die("Blocking this code path until we need it.  It probably should be looked at more closely before letting it proceed\n");
			}
		}
	}

	if ($isDesktopBuild)
	{
		if ($^O eq "cygwin")
		{
			system("$winPerl", "$winMonoRoot/external/buildscripts/build_runtime_vs.pl", "--build=$build", "--arch32=$arch32", "--msbuildversion=$msBuildVersion", "--clean=$clean", "--debug=$debug") eq 0 or die ('failed building mono with VS\n');

			# Copy over the VS built stuff that we want to use instead into the prefix directory
			my $archNameForBuild = $arch32 ? 'Win32' : 'x64';
			my $config = $debug ? "Debug" : "Release";
			system("cp $monoroot/msvc/$archNameForBuild/bin/$config/mono.exe $monoprefix/bin/.") eq 0 or die ("failed copying mono.exe\n");
			system("cp $monoroot/msvc/$archNameForBuild/bin/$config/mono-2.0.dll $monoprefix/bin/.") eq 0 or die ("failed copying mono-2.0.dll\n");
			system("cp $monoroot/msvc/$archNameForBuild/bin/$config/mono-2.0.pdb $monoprefix/bin/.") eq 0 or die ("failed copying mono-2.0.pdb\n");
		}

		system("cp -R $addtoresultsdistdir/bin/. $monoprefix/bin/") eq 0 or die ("Failed copying $addtoresultsdistdir/bin to $monoprefix/bin\n");
	}

	if(!($disableMcs))
	{
		my @additionalProfiles = ();
		push @additionalProfiles, "unityjit";
		push @additionalProfiles, "unityaot";

		chdir("$monoroot/mcs");
		foreach my $profileName(@additionalProfiles)
		{
			print(">>> Making profile : $profileName\n");
			system("make", "PROFILE=$profileName") eq 0 or die ("Failed to make $profileName profile in mcs\n");

			my $profileDestDir = "$monoprefix/lib/mono/$profileName";
			print(">>> Copying $profileName to $profileDestDir directory\n");

			print(">>> Cleaning $profileDestDir\n");
			system("rm -rf $profileDestDir");

			system("mkdir -p $profileDestDir") eq 0 or die("failed to make directory $profileDestDir\n");
			system("mkdir -p $profileDestDir/Facades") eq 0 or die("failed to make directory $profileDestDir/Facades\n");

			system("cp $monoroot/mcs/class/lib/$profileName/*.dll $profileDestDir") eq 0 or die("Failed copying dlls from $monoroot/mcs/class/lib/$profileName to $profileDestDir\n");
			system("cp $monoroot/mcs/class/lib/$profileName/Facades/*.dll $profileDestDir/Facades") eq 0 or die("Failed copying dlls from $monoroot/mcs/class/lib/$profileName/Facades to $profileDestDir/Facades\n");
		}

		chdir("$monoroot");
	}
}
else
{
	print(">>> Skipping build\n");
}

if ($buildUsAndBoo)
{
	print(">>> Building Unity Script and Boo...\n");
	if($windowsSubsystemForLinux)
	{
		#boo scripts expect a bin-platform folder, but we haven't built them that way
		system("ln -s $monoprefix/bin $monoprefix/bin-linux64");
		system("ln -s $monoprefix/bin $monoprefix/bin-linux32");
	}

	system(@commandPrefix, ("perl", "$buildscriptsdir/build_us_and_boo.pl", "--monoprefix=$monoprefix")) eq 0 or die ("Failed building Unity Script and Boo\n");

	print(">>> Copying Unity Script and Boo *.Lang.dll's from 4.5 profile to unityjit profile...\n");
	system("cp $monoprefix/lib/mono/4.5/Boo*.dll $monoprefix/lib/mono/unityjit/.") eq 0 or die("Failed copying Boo*.dll\n");
	system("cp $monoprefix/lib/mono/4.5/UnityScript*.dll $monoprefix/lib/mono/unityjit/.") eq 0 or die("Failed copying UnityScript*.dll\n");
	system("cp $monoprefix/lib/mono/4.5/booc.exe $monoprefix/lib/mono/unityjit/.") eq 0 or die("Failed copying booc.exe\n");
	system("cp $monoprefix/lib/mono/4.5/us.exe $monoprefix/lib/mono/unityjit/.") eq 0 or die("Failed copying us.exe\n");
}
else
{
	print(">>> Skipping build Unity Script and Boo\n");
}

if ($artifact)
{
	print(">>> Creating artifact...\n");

	if ($artifactsCommon)
	{
		print(">>> Creating common artifacts...\n");
		print(">>> distribution directory = $distdir\n");

		if (!(-d "$distdir"))
		{
			system("mkdir -p $distdir") eq 0 or die("failed to make directory $distdir\n");
		}

		$File::Copy::Recursive::CopyLink = 0;  #make sure we copy files as files and not as symlinks, as TC unfortunately doesn't pick up symlinks.

		my $distdirlibmono = "$distdir/lib/mono";

		print(">>> Cleaning $distdir/lib\n");
		system("rm -rf $distdir/lib");
		system("mkdir -p $distdir/lib");

		print(">>> Creating normal profile artifacts...\n");
		system("cp -R $addtoresultsdistdir/. $distdir/") eq 0 or die ("Failed copying $addtoresultsdistdir to $distdir\n");

		system("cp -r $monoprefix/lib/mono $distdir/lib");

		if($^O ne 'darwin')
		{
			# On OSX we build a universal binary for 32-bit and 64-bit in the mono executable. The class library build
			# only creates the 64-bit slice, so we don't want to end up with a single slice binary in the output.
			# If we do, it will step on the universal binary produced but the OSX runtime build.
			system("cp -r $monoprefix/bin $distdir/") eq 0 or die ("failed copying bin folder\n");
		}
		system("cp -r $monoprefix/etc $distdir/") eq 0 or die("failed copying etc folder\n");

		system("cp -R $externalBuildDeps/reference-assemblies/unity $distdirlibmono/unity");
 		system("cp -R $externalBuildDeps/reference-assemblies/unity_web $distdirlibmono/unity_web");

 		system("cp -R $externalBuildDeps/reference-assemblies/unity/Boo*.dll $distdirlibmono/2.0-api");
 		system("cp -R $externalBuildDeps/reference-assemblies/unity/UnityScript*.dll $distdirlibmono/2.0-api");

 		system("cp -R $externalBuildDeps/reference-assemblies/unity/Boo*.dll $distdirlibmono/4.0-api");
 		system("cp -R $externalBuildDeps/reference-assemblies/unity/UnityScript*.dll $distdirlibmono/4.0-api");

		system("cp -R $externalBuildDeps/reference-assemblies/unity/Boo*.dll $distdirlibmono/4.5-api");
		system("cp -R $externalBuildDeps/reference-assemblies/unity/UnityScript*.dll $distdirlibmono/4.5-api");

		# now remove nunit from a couple places (but not all, we need some of them)
		# linux tar is not happy these are removed(at least on wsl), so don't remove them for now
		if(not $windowsSubsystemForLinux)
		{
			system("rm -rf $distdirlibmono/2.0/nunit*");
			system("rm -rf $distdirlibmono/gac/nunit*");
		}

		# Remove a self referencing sym link that causes problems
		system("rm -rf $monoprefix/bin/bin");

		if (-f "$monoroot/ZippedClasslibs.tar.gz")
		{
			system("rm -f $monoroot/ZippedClasslibs.tar.gz") eq 0 or die("Failed to clean existing ZippedClasslibs.tar.gz\n");
		}

		print(">>> Creating ZippedClasslibs.tar.gz\n");
		print(">>> Changing directory to : $buildsroot\n");
		chdir("$buildsroot");
		system("tar -hpczf ../ZippedClasslibs.tar.gz *") eq 0 or die("Failed to zip up classlibs\n");
		print(">>> Changing directory back to : $currentdir\n");
		chdir("$currentdir");
	}

	# Do the platform specific logic to create the builds output structure that we want

	my $embedDirRoot = "$buildsroot/embedruntimes";
	my $embedDirArchDestination = "";
	my $distDirArchBin = "";
	my $versionsOutputFile = "";
	my $crossCompilerRoot = "$buildsroot/crosscompiler";
	my $crossCompilerDestination = "";

	if ($iphone)
	{
		$embedDirArchDestination = "$embedDirRoot/iphone/$iphoneArch";
		$versionsOutputFile = "$buildsroot/versions-iphone-$iphoneArch.txt";
	}
	elsif ($iphoneCross)
	{
		$crossCompilerDestination = "$buildsroot/crosscompiler/iphone";
		$versionsOutputFile = "$buildsroot/versions-iphone-xcompiler.txt";
	}
	elsif ($iphoneSimulator)
	{
		$embedDirArchDestination = "$embedDirRoot/iphone/$iphoneSimulatorArch";
		$versionsOutputFile = "$buildsroot/versions-iphone-$iphoneSimulatorArch.txt";
	}
	elsif ($android)
	{
		$embedDirArchDestination = "$embedDirRoot/android/$androidArch";
		$versionsOutputFile = "$buildsroot/versions-android-$androidArch.txt";
	}
	elsif ($tizenEmulator)
	{
		$embedDirArchDestination = "$embedDirRoot/tizenemulator/";
		$versionsOutputFile = "$buildsroot/versions-tizenemulator.txt";
	}
	elsif ($tizen)
	{
		$embedDirArchDestination = "$embedDirRoot/tizen/";
		$versionsOutputFile = "$buildsroot/versions-tizen.txt";
	}
	elsif($^O eq "linux")
	{
		$embedDirArchDestination = $arch32 ? "$embedDirRoot/linux32" : "$embedDirRoot/linux64";
		$distDirArchBin = $arch32 ? "$distdir/bin-linux32" : "$distdir/bin-linux64";
		$versionsOutputFile = $arch32 ? "$buildsroot/versions-linux32.txt" : "$buildsroot/versions-linux64.txt";
	}
	elsif($^O eq 'darwin')
	{
		# Note these tmp directories will get merged into a single 'osx' directory later by a parent script
		$embedDirArchDestination = "$embedDirRoot/osx-tmp-$monoHostArch";
		$distDirArchBin = "$distdir/bin-osx-tmp-$monoHostArch";
		$versionsOutputFile = $arch32 ? "$buildsroot/versions-osx32.txt" : "$buildsroot/versions-osx64.txt";
	}
	else
	{
		$embedDirArchDestination = $arch32 ? "$embedDirRoot/win32" : "$embedDirRoot/win64";
		$distDirArchBin = $arch32 ? "$distdir/bin" : "$distdir/bin-x64";
		$versionsOutputFile = $arch32 ? "$buildsroot/versions-win32.txt" : "$buildsroot/versions-win64.txt";
	}

	# Make sure the directory for our architecture is clean before we copy stuff into it
	if (-d "$embedDirArchDestination")
	{
		print(">>> Cleaning $embedDirArchDestination\n");
		rmtree($embedDirArchDestination);
	}

	if (-d "$distDirArchBin")
	{
		print(">>> Cleaning $distDirArchBin\n");
		rmtree($distDirArchBin);
	}

	if ($artifactsRuntime)
	{
		system("mkdir -p $embedDirArchDestination") if ($embedDirArchDestination ne "");
		system("mkdir -p $distDirArchBin") if ($distDirArchBin ne "");
		system("mkdir -p $crossCompilerDestination") if ($crossCompilerDestination ne "");

		# embedruntimes directory setup
		print(">>> Creating embedruntimes directory : $embedDirArchDestination\n");
		if ($iphone || $iphoneSimulator)
		{
			for my $file ('libmonosgen-2.0.a','libmonobdwgc-2.0.a')
			{
				print ">>> Copying $file\n";
				system("cp", "$monoroot/mono/mini/.libs/$file","$embedDirArchDestination/$file") eq 0 or die ("failed copying $file\n");
			}
		}
		elsif ($iphoneCross)
		{
			# Nothing to do
		}
		elsif ($android)
		{
			for my $file ('libmonosgen-2.0.so','libmonosgen-2.0.a','libmonobdwgc-2.0.so','libmonobdwgc-2.0.a')
			{
				print ">>> Copying $file\n";
				system("cp", "$monoroot/mono/mini/.libs/$file","$embedDirArchDestination/$file") eq 0 or die ("failed copying $file\n");
			}
		}
		elsif ($tizen || $tizenEmulator)
		{
			for my $file ('libmonosgen-2.0.so','libmonobdwgc-2.0.so')
			{
				print ">>> Copying $file\n";
				system("cp", "$monoroot/mono/mini/.libs/$file","$embedDirArchDestination/$file") eq 0 or die ("failed copying $file\n");
			}
		}
		elsif($^O eq "linux")
		{
			print ">>> Copying libmonosgen-2.0\n";
			system("cp", "$monoroot/mono/mini/.libs/libmonobdwgc-2.0.so","$embedDirArchDestination/libmonobdwgc-2.0.so") eq 0 or die ("failed copying libmonobdwgc-2.0.so\n");
			system("cp", "$monoroot/mono/mini/.libs/libmonosgen-2.0.so","$embedDirArchDestination/libmonosgen-2.0.so") eq 0 or die ("failed copying libmonosgen-2.0.so\n");

			print ">>> Copying libMonoPosixHelper.so\n";
			system("cp", "$monoroot/support/.libs/libMonoPosixHelper.so","$embedDirArchDestination/libMonoPosixHelper.so") eq 0 or die ("failed copying libMonoPosixHelper.so\n");

			if ($buildMachine)
			{
				system("strip $embedDirArchDestination/libmonobdwgc-2.0.so") eq 0 or die("failed to strip libmonobdwgc-2.0.so (shared)\n");
				system("strip $embedDirArchDestination/libmonosgen-2.0.so") eq 0 or die("failed to strip libmonosgen-2.0.so (shared)\n");
				system("strip $embedDirArchDestination/libMonoPosixHelper.so") eq 0 or die("failed to strip libMonoPosixHelper (shared)\n");
			}
		}
		elsif($^O eq 'darwin')
		{
			# embedruntimes directory setup
	 		print ">>> Hardlinking libmonosgen-2.0\n";

			system("ln","-f", "$monoroot/mono/mini/.libs/libmonobdwgc-2.0.dylib","$embedDirArchDestination/libmonobdwgc-2.0.dylib") eq 0 or die ("failed symlinking libmonobdwgc-2.0.dylib\n");
			system("ln","-f", "$monoroot/mono/mini/.libs/libmonosgen-2.0.dylib","$embedDirArchDestination/libmonosgen-2.0.dylib") eq 0 or die ("failed symlinking libmonosgen-2.0.dylib\n");

			print "Hardlinking libMonoPosixHelper.dylib\n";
			system("ln","-f", "$monoroot/support/.libs/libMonoPosixHelper.dylib","$embedDirArchDestination/libMonoPosixHelper.dylib") eq 0 or die ("failed symlinking $libtarget/libMonoPosixHelper.dylib\n");

			InstallNameTool("$embedDirArchDestination/libmonobdwgc-2.0.dylib", "\@executable_path/../Frameworks/MonoEmbedRuntime/osx/libmonobdwgc-2.0.dylib");
			InstallNameTool("$embedDirArchDestination/libmonosgen-2.0.dylib", "\@executable_path/../Frameworks/MonoEmbedRuntime/osx/libmonosgen-2.0.dylib");
			InstallNameTool("$embedDirArchDestination/libMonoPosixHelper.dylib", "\@executable_path/../Frameworks/MonoEmbedRuntime/osx/libMonoPosixHelper.dylib");

			print ">>> Copying mono public headers\n";
			system("mkdir -p $includesroot/mono");
			system("cp -R $monoprefix/include/mono-2.0/mono $includesroot/mono");
		}
		else
		{
			# embedruntimes directory setup
			system("cp", "$monoprefix/bin/mono-2.0-bdwgc.dll", "$embedDirArchDestination/mono-2.0-bdwgc.dll") eq 0 or die ("failed copying mono-2.0-bdwgc.dll\n");
			system("cp", "$monoprefix/bin/mono-2.0-bdwgc.pdb", "$embedDirArchDestination/mono-2.0-bdwgc.pdb") eq 0 or die ("failed copying mono-2.0-bdwgc.pdb\n");

			system("cp", "$monoprefix/bin/mono-2.0-sgen.dll", "$embedDirArchDestination/mono-2.0-sgen.dll") eq 0 or die ("failed copying mono-2.0-sgen.dll\n");
			system("cp", "$monoprefix/bin/mono-2.0-sgen.pdb", "$embedDirArchDestination/mono-2.0-sgen.pdb") eq 0 or die ("failed copying mono-2.0-sgen.pdb\n");
		}

		# sources directory setup
		print ">>> Copying mono sources needed for il2cpp\n";
		system("mkdir -p $sourcesroot") eq 0 or die "failed making directory $sourcesroot\n";

		# need to build some arm headers to package up for android
		chdir("$monoroot/mono/arch/arm") eq 1 or die ("failed to chdir to mono/arch/arm directory\n");
		system(@commandPrefix, ("make")) eq 0 or die ("failed to make arm headers\n");
		print(">>> Changing directory back to : $currentdir\n");
		chdir("$currentdir") eq 1 or die ("failed to chdir to $currentdir\n");

		system("perl", "$buildscriptsdir/copy_il2cpp_sources.pl", "--monoroot=$monoroot", "--sourcesroot=$sourcesroot") eq 0 or die ('failed to copy the source files needed for il2cpp');

		# monodistribution directory setup
		print(">>> Creating monodistribution directory\n");
		if ($android || $iphone || $iphoneCross || $iphoneSimulator || $tizen || $tizenEmulator)
		{
			# Nothing to do
		}
		elsif($^O eq "linux")
		{
			my $distDirArchEtc = $arch32 ? "$distdir/etc-linux32" : "$distdir/etc-linux64";

			if (-d "$distDirArchEtc")
			{
				print(">>> Cleaning $distDirArchEtc\n");
				rmtree($distDirArchEtc);
			}

			system("mkdir -p $distDirArchBin");
			system("mkdir -p $distDirArchEtc");
			system("mkdir -p $distDirArchEtc/mono");

			system("ln", "-f", "$monoroot/mono/mini/mono-sgen","$distDirArchBin/mono") eq 0 or die("failed symlinking mono executable\n");
			system("ln", "-f", "$monoroot/tools/pedump/pedump","$distDirArchBin/pedump") eq 0 or die("failed symlinking pedump executable\n");
			system('cp', "$monoroot/data/config","$distDirArchEtc/mono/config") eq 0 or die("failed to copy config\n");
		}
		elsif($^O eq 'darwin')
		{
			system("ln", "-f", "$monoroot/mono/mini/mono","$distDirArchBin/mono") eq 0 or die("failed hardlinking mono executable\n");
			system("ln", "-f", "$monoroot/tools/pedump/pedump","$distDirArchBin/pedump") eq 0 or die("failed hardlinking pedump executable\n");
		}
		else
		{
			system("cp", "$monoprefix/bin/mono-2.0.dll", "$distDirArchBin/mono-2.0.dll") eq 0 or die ("failed copying mono-2.0.dll\n");
			system("cp", "$monoprefix/bin/mono-2.0.pdb", "$distDirArchBin/mono-2.0.pdb") eq 0 or die ("failed copying mono-2.0.pdb\n");
			system("cp", "$monoprefix/bin/mono.exe", "$distDirArchBin/mono.exe") eq 0 or die ("failed copying mono.exe\n");
		}
	}

	# cross compiler directory setup
	if ($iphoneCross)
	{
		print ">>> Copying mono-xcompiler\n";
		if($runningOnWindows)
		{
			die("Not implemented\n");
		}
		else
		{
			system("cp", "$monoroot/mono/mini/mono","$crossCompilerDestination/mono-xcompiler") eq 0 or die ("failed copying mono-xcompiler\n");
		}
	}

	# Not all build configurations output to the distro dir, so only chmod it if it exists
	system("chmod", "-R", "755", $distDirArchBin) if (-d "$distDirArchBin");

	# Output version information
	print(">>> Creating version file : $versionsOutputFile\n");
	system("echo \"mono-version =\" > $versionsOutputFile");

	# Not all build configurations output to the distro dir, only try to output version info if there is a distro dir
	system("$distDirArchBin/mono --version >> $versionsOutputFile") if (-d "$distDirArchBin");

	system("echo \"unity-mono-revision = $monoRevision\" >> $versionsOutputFile");
	system("echo \"unity-mono-build-scripts-revision = $buildScriptsRevision\" >> $versionsOutputFile");
	my $tmp = `date`;
	system("echo \"build-date = $tmp\" >> $versionsOutputFile");
}
else
{
	print(">>> Skipping artifact creation\n");
}

if ($test)
{
	if ($runRuntimeTests)
	{
		my $runtimeTestsDir = "$monoroot/mono/mini";
		chdir("$runtimeTestsDir") eq 1 or die ("failed to chdir");
		print("\n>>> Calling make check in $runtimeTestsDir\n\n");
		system("make","check") eq 0 or die ("runtime tests failed\n");
	}
	else
	{
		print(">>> Skipping runtime unit tests\n");
	}

	if ($runClasslibTests)
	{
		if ($disableMcs)
		{
			print(">>> Skipping classlib unit tests because building the class libs was disabled\n");
		}
		else
		{
			my $classlibTestsDir = "$monoroot/mcs/class";
			chdir("$classlibTestsDir") eq 1 or die ("failed to chdir");
			print("\n>>> Calling make run-test in $runtimeTestsDir\n\n");
			system("make","run-test") eq 0 or die ("classlib tests failed\n");
		}
	}
	else
	{
		print(">>> Skipping classlib unit tests\n");
	}
}
else
{
	print(">>> Skipping unit tests\n");
}

chdir ($currentdir);
