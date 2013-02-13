use strict;

use lib ('.', "../../Tools/perl_lib","perl_lib");
use Cwd;
use File::Path;
use Getopt::Long;
use Tools qw(InstallNameTool);

my $root = getcwd();
my $buildsroot = "$root/builds";
my $buildir = "$buildsroot/src";
my $embeddir = "$buildsroot/embedruntimes";
my $distdir = "$buildsroot/monodistribution/bin";
my $skipbuild=0;
my $debug = 0;
my $minimal = 0;
my $cleanpath = 0;
my $cleanbuild = 1;
my $dobuild = 'osx';
my $jobs = 4;

GetOptions(
   "skipbuild=i"=>\$skipbuild,
   "debug=i"=>\$debug,
   "minimal=i"=>\$minimal,
   "cleanpath=i"=>\$cleanpath,
   "cleanbuild=i"=>\$cleanbuild,
   "build=s"=>\$dobuild,
   "j=i"=>\$jobs
) or die ("illegal cmdline options");

my $teamcity=0;
if ($ENV{UNITY_THISISABUILDMACHINE})
{
	print "rmtree-ing $buildsroot because we're on a buildserver, and want to make sure we don't include old artifacts\n";
	rmtree("$buildsroot");
	$teamcity=1;
	$jobs = "";
} else {
	print "not rmtree-ing $buildsroot, as we're not on a buildmachine\n";
	if (($debug==0) && ($skipbuild==0))
	{
		print "\n\nARE YOU SURE YOU DONT WANT TO MAKE A DEBUG BUILD?!?!?!!!!!\n\n\n";
	}

	$ENV{"PATH"} = "/usr/bin:/bin:/usr/sbin:/sbin:/usr/X11/bin" if $cleanpath;
	$jobs = "-j$jobs";
}


# From Massi: I was getting failures in install_name_tool about space
# for the commands being too small, and adding here things like
# $ENV{LDFLAGS} = '-headerpad_max_install_names' and
# $ENV{LDFLAGS} = '-headerpad=0x40000' did not help at all (and also
# adding them to our final gcc invocation to make the bundle).
# Lucas noticed that I was lacking a Mono prefix, and having a long
# one would give us space, so here is this silly looong prefix.
my $prefix = "$buildsroot/tmp/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting";


my $savedpath = $ENV{PATH};
my $savedcinclude = $ENV{C_INCLUDE_PATH};
my $savedcppinclude = $ENV{CPLUS_INCLUDE_PATH};
my $savedcflags = $ENV{CFLAGS};
my $savedcxxflags = $ENV{CXXFLAGS};
my $savedcc = $ENV{CC};
my $savedcxx = $ENV{CXX};
my $savedcpp = $ENV{CPP};
my $savedcxxpp = $ENV{CXXPP};
my $savedld = $ENV{LD};
my $savedldflags = $ENV{LDFLAGS};


sub configure_mono
{
	chdir("$root/eglib") eq 1 or die ("Failed chdir 1");
	
	#this will fail on a fresh working copy, so don't die on it.
	#system("make distclean");

	print "calling autoreconf -i on $root/eglib\n";
	system("autoreconf -i") eq 0 or die ("Failed autoreconfing eglib");
	
	chdir("$root") eq 1 or die ("failed to chdir 2");

	print "calling autoreconf -i on $root\n";	
	system("autoreconf -i") eq 0 or die ("Failed autoreconfing mono");

}

sub setenv
{
	my $envpath = shift;
	my $envcinclude = shift;
	my $envcppinclude = shift;
	my $envcflags = shift;
	my $envcxxflags = shift;
	my $envcc = shift;
	my $envcxx = shift;
	my $envcpp = shift;
	my $envcxxpp = shift;
	my $envld = shift;
	my $envldflags = shift;

	$ENV{PATH} = $savedpath;
	$ENV{C_INCLUDE_PATH} = $savedcinclude;
	$ENV{CPLUS_INCLUDE_PATH} = $savedcppinclude;
	$ENV{CFLAGS} = $savedcflags;
	$ENV{CXXFLAGS} = $savedcxxflags;
	$ENV{CC} = $savedcc;
	$ENV{CXX} = $savedcxx;
	$ENV{CPP} = $savedcpp;
	$ENV{CXXPP} = $savedcxxpp;
	$ENV{LD} = $savedld;
	$ENV{LDFLAGS} = $savedldflags;

	$ENV{PATH} = "$envpath:".$ENV{PATH} if $envpath;
	$ENV{C_INCLUDE_PATH} = $envcinclude if $envcinclude;
	$ENV{CPLUS_INCLUDE_PATH} = $envcppinclude if $envcppinclude;
	$ENV{CFLAGS} = $envcflags if $envcflags;
	$ENV{CXXFLAGS} = $envcxxflags if $envcxxflags;
	$ENV{CC} = $envcc if $envcc;
	$ENV{CXX} = $envcxx if $envcxx;
	$ENV{CPP} = $envcpp if $envcpp;
	$ENV{CXXPP} = $envcxxpp if $envcxxpp;
	$ENV{LD} = $envld if $envld;
	$ENV{LDFLAGS} = $envldflags if $envldflags;


	print "\n";
	print "Setting environment:\n";
	print "PATH = ".$ENV{PATH}."\n";;
	print "C_INCLUDE_PATH = ".$ENV{C_INCLUDE_PATH}."\n";;
	print "CPLUS_INCLUDE_PATH = ".$ENV{CPLUS_INCLUDE_PATH}."\n";;
	print "CFLAGS = ".$ENV{CFLAGS}."\n";;
	print "CXXFLAGS = ".$ENV{CXXFLAGS}."\n";;
	print "CC = ".$ENV{CC}."\n";;
	print "CXX = ".$ENV{CXX}."\n";;
	print "CPP = ".$ENV{CPP}."\n";;
	print "CXXPP = ".$ENV{CXXPP}."\n";;
	print "LD = ".$ENV{LD}."\n";;
	print "LDFLAGS = ".$ENV{LDFLAGS}."\n";;
	print "\n";

}

sub setenv_iphone_runtime
{
	my $arch = shift;
	my $cachefile = shift;
	my $sdkversion = shift;

	my $aspen_root = "/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer";
	my $aspen_sdk = "$aspen_root/SDKs/iPhoneOS${sdkversion}.sdk";
	
	$sdkversion = "5.1" unless (-d $aspen_sdk);
	$sdkversion = "6.0" unless (-d $aspen_sdk);

	$aspen_sdk = "$aspen_root/SDKs/iPhoneOS${sdkversion}.sdk";

	my $path = "$aspen_root/usr/bin";
	my $cinclude = "$aspen_sdk/usr/lib/gcc/arm-apple-darwin9/4.2.1/include:$aspen_sdk/usr/include";
	my $cppinclude = "$aspen_sdk/usr/lib/gcc/arm-apple-darwin9/4.2.1/include:$aspen_sdk/usr/include";
	my $cflags = "-DHAVE_ARMV6=1 -DZ_PREFIX -DPLATFORM_IPHONE -DARM_FPU_VFP=1 -miphoneos-version-min=3.0 -mno-thumb -fvisibility=hidden -Os";
	my $cxxflags = "$cflags";
	my $cc = "gcc -arch $arch";
	my $cxx = "g++ -arch $arch";
	my $cpp = "cpp -nostdinc -U__powerpc__ -U__i386__ -D__arm__";
	my $cxxpp = "cpp -nostdinc -U__powerpc__ -U__i386__ -D__arm__";
	my $ld = $cc;
	my $ldflags = "-liconv -Wl,-syslibroot,$aspen_sdk";

	my @configureparams = ();
	unshift(@configureparams, "--cache-file=$cachefile");
	unshift(@configureparams, "--disable-mcs-build");
	unshift(@configureparams, "--host=arm-apple-darwin9");
	unshift(@configureparams, "--disable-shared-handles");
	unshift(@configureparams, "--with-tls=pthread");
	unshift(@configureparams, "--with-sigaltstack=no");
	unshift(@configureparams, "--with-glib=embedded");
	unshift(@configureparams, "--enable-minimal=jit,profiler,com");
	unshift(@configureparams, "--disable-nls");
	unshift(@configureparams, "--with-sgen=yes");
	unshift(@configureparams, "--prefix=$prefix");

	$ENV{mono_cv_uscore} = "yes";
	$ENV{mono_cv_clang} = "no";
	$ENV{cv_mono_sizeof_sunpath} = "104";
	$ENV{ac_cv_func_posix_getpwuid_r} = "yes";
	$ENV{ac_cv_func_backtrace_symbols} = "no";

	setenv ($path, $cinclude, $cppinclude, $cflags, $cxxflags, $cc, $cxx, $cpp, $cxxpp, $ld, $ldflags);

	return (@configureparams);
}

sub setenv_iphone_crosscompiler
{
	my $arch = shift;
	my $cachefile = shift;
	my $sdkversion = shift;


	my $xcomp_aspen_root = "/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$sdkversion.sdk";

	$ENV{"MACSDKOPTIONS"} = "-D_XOPEN_SOURCE -mmacosx-version-min=$sdkversion -isysroot $xcomp_aspen_root";
	$ENV{"PLATFORM_IPHONE_XCOMP"} = 1;


	my $path;
	my $cinclude;
	my $cppinclude;
	my $cflags = "-DARM_FPU_VFP=1 -DUSE_MUNMAP -DPLATFORM_IPHONE_XCOMP";
	my $cxxflags;
	my $cc = "gcc -arch $arch";
	my $cxx = "g++ -arch $arch";
	my $cpp = "$cc -E";
	my $cxxpp;
	my $ld = $cc;
	my $ldflags;

	my @configureparams = ();
	unshift(@configureparams, "--cache-file=$cachefile");
	unshift(@configureparams, "--disable-mcs-build");
	unshift(@configureparams, "--disable-shared-handles");
	unshift(@configureparams, "--with-tls=pthread");
	unshift(@configureparams, "--with-sigaltstack=no");
	unshift(@configureparams, "--with-glib=embedded");
	unshift(@configureparams, "--disable-nls");

	unshift(@configureparams, "--with-macversion=$sdkversion");
	unshift(@configureparams, "--target=arm-darwin");

	unshift(@configureparams, "--prefix=$prefix");

	$ENV{mono_cv_uscore} = "yes";
	$ENV{mono_cv_clang} = "no";
	$ENV{cv_mono_sizeof_sunpath} = "104";
	$ENV{ac_cv_func_posix_getpwuid_r} = "yes";
	$ENV{ac_cv_func_backtrace_symbols} = "no";

	setenv ($path, $cinclude, $cppinclude, $cflags, $cxxflags, $cc, $cxx, $cpp, $cxxpp, $ld, $ldflags);

	return (@configureparams);
}

sub setenv_osx
{
	my $arch = shift;
	my $cachefile = shift;
	my $macversion = shift;

	my $path;

	if ($ENV{"UNITY_THISISABUILDMACHINE"}) {
		#we need to manually set the compiler to gcc4, because the 10.4 sdk only shipped with the gcc4 headers
		#their setup is a bit broken as they dont autodetect this, but basically the gist is if you want to copmile
		#against the 10.4 sdk, you better use gcc4, otherwise things go boink.
		$ENV{CC} = "gcc-4.0" unless ($ENV{CC});
		$ENV{CXX} = "gcc-4.0" unless ($ENV{CXX});
	}


	my $cinclude;
	my $cppinclude;

	my $cflags = "-D_XOPEN_SOURCE=1 -arch $arch -DMONO_DISABLE_SHM=1 -DDISABLE_SHARED_HANDLES=1";
	$cflags = "$cflags -g -O0" if $debug;	
	$cflags = "$cflags -Os" if not $debug; #optimize for size

	my $cxxflags = "$cflags";

	my $cc;
	my $cxx;
	if ($ENV{"UNITY_THISISABUILDMACHINE"}) {
		$cc = "gcc-4.0" unless ($ENV{CC});
		$cxx = "gcc-4.0" unless ($ENV{CXX});
	}

	my $cpp;
	my $cxxpp;
	my $ld;
	my $ldflags;

	my @configureparams = ();
	unshift(@configureparams, "--cache-file=$cachefile");
	unshift(@configureparams, "--disable-mcs-build");
	unshift(@configureparams, "--with-glib=embedded");
	unshift(@configureparams, "--with-macversion=$macversion");
	unshift(@configureparams, "--disable-nls");  #this removes the dependency on gettext package
	unshift(@configureparams, "--prefix=$prefix");
	unshift(@configureparams, "--enable-minimal=aot,logging,com,profiler,debug") if $minimal;

	$ENV{mono_cv_uscore} = "";
	$ENV{mono_cv_clang} = "";
	$ENV{cv_mono_sizeof_sunpath} = "";
	$ENV{ac_cv_func_posix_getpwuid_r} = "";
	$ENV{ac_cv_func_backtrace_symbols} = "";

	setenv ($path, $cinclude, $cppinclude, $cflags, $cxxflags, $cc, $cxx, $cpp, $cxxpp, $ld, $ldflags);

	return (@configureparams);
}

sub setenv_iphone_simulator
{
	my $arch = shift;
	my $cachefile = shift;
	my $sdkversion = shift;

	$debug = 1;

	my $path;

	if ($ENV{"UNITY_THISISABUILDMACHINE"}) {
		#we need to manually set the compiler to gcc4, because the 10.4 sdk only shipped with the gcc4 headers
		#their setup is a bit broken as they dont autodetect this, but basically the gist is if you want to copmile
		#against the 10.4 sdk, you better use gcc4, otherwise things go boink.
		$ENV{CC} = "gcc-4.0" unless ($ENV{CC});
		$ENV{CXX} = "gcc-4.0" unless ($ENV{CXX});
	}


	my $aspen_root = "/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer";
	my $aspen_sdk = "$aspen_root/SDKs/iPhoneOS${sdkversion}.sdk";
	
	$sdkversion = "5.1" unless (-d $aspen_sdk);
	$sdkversion = "6.0" unless (-d $aspen_sdk);
	$aspen_sdk = "$aspen_root/SDKs/iPhoneSimulator${sdkversion}.sdk";

	my $macsysroot = "-isysroot $aspen_sdk";
	my $macsdkoptions = "-miphoneos-version-min=3.0 $macsysroot";

	my $cinclude;
	my $cppinclude;

	my $cflags = "-D_XOPEN_SOURCE=1 -DTARGET_IPHONE_SIMULATOR";
	$cflags = "$cflags -g -O0" if $debug;	
	$cflags = "$cflags -Os" if not $debug; #optimize for size

	my $cxxflags = "$cflags";

	my $cc = "$aspen_root/usr/bin/gcc -arch $arch";
	my $cxx = "$aspen_root/usr/bin/g++ -arch $arch";

	my $cpp = "$cc -E";
	my $cxxpp;
	my $ld;
	my $ldflags;


	my @configureparams = ();
	unshift(@configureparams, "--cache-file=$cachefile");
	unshift(@configureparams, "--disable-mcs-build");
	unshift(@configureparams, "--with-glib=embedded");
	unshift(@configureparams, "--disable-nls");  #this removes the dependency on gettext package
	unshift(@configureparams, "--prefix=$prefix");

	$ENV{mono_cv_uscore} = "yes";
	$ENV{mono_cv_clang} = "no";
	$ENV{cv_mono_sizeof_sunpath} = "104";
	$ENV{ac_cv_func_posix_getpwuid_r} = "yes";
	$ENV{ac_cv_func_backtrace_symbols} = "no";

	$ENV{"MACSDKOPTIONS"} = $macsdkoptions;
	$ENV{"MACSYSROOT"} = $macsysroot;


	setenv ($path, $cinclude, $cppinclude, $cflags, $cxxflags, $cc, $cxx, $cpp, $cxxpp, $ld, $ldflags);

	return (@configureparams);
}


sub build_mono
{
	my $arch = shift;
	my $buildtarget = shift;
	my $cachefile = shift;

	my $os = shift;
	my @configureparams = @{$_[0]};

	print("buildtarget: $buildtarget\n");

	my $exists = 0;
	$exists = 1 if (chdir("$buildtarget") eq 1);

	my $saved_skipbuild = $skipbuild;
	my $saved_cleanbuild = $cleanbuild;

	if ($cleanbuild == 0 && $skipbuild == 0 && $exists == 0) {
		$cleanbuild = 1;
		$skipbuild = 0;
	}

	if ($cleanbuild == 1) {
		system("rm $cachefile");
		if (chdir("$buildtarget") eq 1) {
			system("make clean");
		}
	}

	mkpath($buildtarget);
	chdir("$buildtarget") eq 1 or die ("failed to chdir to $buildtarget");

	if ($cleanbuild == 1) {
		print("\n\nCalling configure with these parameters: ");
		system("echo", @configureparams);
		print("\n\n");
		system("calling ./configure on $buildtarget",@configureparams);

		system("$root/configure", @configureparams) eq 0 or die ("failing configuring mono");

		system("perl -pi -e 's/MONO_SIZEOF_SUNPATH 0/MONO_SIZEOF_SUNPATH 104/' config.h") if ($arch eq 'armv6' || $arch eq 'armv7');
		system("perl -pi -e 's/#define HAVE_FINITE 1//' config.h") if ($arch eq 'armv6' || $arch eq 'armv7');
		system("perl -pi -e 's/#define HAVE_CURSES_H 1//' config.h") if ($arch eq 'armv6' || $arch eq 'armv7');
		system("perl -pi -e 's/#define HAVE_STRNDUP 1//' eglib/config.h") if ($os eq 'iphone' || $os eq 'crosscompiler');
	}

	system("make $jobs") eq 0 or die ("failing running make for mono");

	$skipbuild = $saved_skipbuild;
	$cleanbuild = $saved_cleanbuild;

}


sub build_osx
{
	my $os = "osx";
	my @arches = ('i386','x86_64');
	for my $arch (@arches) {
		print "Building $os for architecture: $arch\n";

		my $macversion = '10.7';
		my $sdkversion = '10.7';

		if ($ENV{"UNITY_THISISABUILDMACHINE"}) {
			$macversion = '10.4' unless $arch eq 'x86_64';
			$sdkversion = '10.4u' unless $arch eq 'x86_64';
			$macversion = '10.6' if $arch eq 'x86_64';
			$sdkversion = '10.6' if $arch eq 'x86_64';
		}

		# Make architecture-specific targets and lipo at the end
		my $bintarget = "$distdir-$arch";
		my $libtarget = "$embeddir/$os-$arch";
		my $buildtarget = "$buildir/$os-$arch";
		my $cachefile = "$buildir/$os-$arch.cache";
		$libtarget = "$embeddir/$os-minimal" if $minimal;

		print("bintarget: $bintarget\n");
		print("libtarget: $libtarget\n");
		print("buildtarget: $buildtarget\n");

		system("rm $bintarget/mono");
		system("rm $libtarget/libmono.0.dylib");
		system("rm $libtarget/libMonoPosixHelper.dylib");
		system("rm -rf $libtarget/libmono.0.dylib.dSYM");

		if (not $skipbuild)
		{
			my @configureparams = setenv_osx ($arch, $cachefile, $macversion);
			build_mono ($arch, $buildtarget, $cachefile, $os, \@configureparams)
		}

		chdir("$buildtarget") eq 1 or die ("failed to chdir to $buildtarget");

		mkpath($bintarget);
		mkpath($libtarget);

		if ($ENV{"UNITY_THISISABUILDMACHINE"})
		{
		#	system("strip $libtarget/libmono.0.dylib") eq 0 or die("failed to strip libmono");
		#	system("strip $libtarget/MonoBundleBinary") eq 0 or die ("failed to strip MonoBundleBinary");
			system("echo \"mono-runtime-osx = $ENV{'BUILD_VCS_NUMBER'}\" > $buildsroot/versions.txt");
		}

		my $cmdline = "gcc -arch $arch -bundle -reexport_library $buildtarget/mono/mini/.libs/libmono.a -isysroot /Developer/SDKs/MacOSX$sdkversion.sdk -mmacosx-version-min=$macversion -all_load -liconv -o $libtarget/MonoBundleBinary";
		print "About to call this cmdline to make a bundle:\n$cmdline\n";
		system($cmdline) eq 0 or die("failed to link libmono.a into mono bundle");

		print "Symlinking libmono.dylib\n";
		system("ln","-f", "$buildtarget/mono/mini/.libs/libmono.0.dylib","$libtarget/libmono.0.dylib") eq 0 or die ("failed symlinking libmono.0.dylib");

		print "Symlinking libmono.a\n";
		system("ln", "-f", "$buildtarget/mono/mini/.libs/libmono.a","$libtarget/libmono.a") eq 0 or die ("failed symlinking libmono.a");

		print "Symlinking libMonoPosixHelper.dylib\n";
		system("ln","-f", "$buildtarget/support/.libs/libMonoPosixHelper.dylib","$libtarget/libMonoPosixHelper.dylib") eq 0 or die ("failed symlinking libMonoPosixHelper.dylib");

		InstallNameTool("$libtarget/libmono.0.dylib", "\@executable_path/../Frameworks/MonoEmbedRuntime/$os/libmono.0.dylib");
		InstallNameTool("$libtarget/libMonoPosixHelper.dylib", "\@executable_path/../Frameworks/MonoEmbedRuntime/$os/libMonoPosixHelper.dylib");

		system("ln","-f","$buildtarget/mono/mini/mono","$bintarget/mono") eq 0 or die("failed symlinking mono executable");
		system("ln","-f","$buildtarget/mono/metadata/pedump","$bintarget/pedump") eq 0 or die("failed symlinking pedump executable");
	}



	mkpath ("$embeddir/$os");


	# Create universal binaries
	for my $file ('libmono.0.dylib','libmono.a','libMonoPosixHelper.dylib') {
		system ('lipo', "$embeddir/$os-i386/$file", "$embeddir/$os-x86_64/$file", '-create', '-output', "$embeddir/$os/$file");
	}

	if (not $ENV{"UNITY_THISISABUILDMACHINE"})
	{
		for my $file ('libmono.0.dylib','libMonoPosixHelper.dylib') {
			rmtree ("$embeddir/$os/$file.dSYM");
			system ('dsymutil', "$embeddir/$os/$file") eq 0 or warn ("Failed creating $embeddir/$os/$file.dSYM");
		}
	}

	system('cp', "$embeddir/$os-i386/MonoBundleBinary", "$embeddir/$os/MonoBundleBinary");

	mkpath ("$distdir");
	for my $file ('mono','pedump') {
		system ('lipo', "$distdir-i386/$file", '-create', '-output', "$distdir/$file");
		# Don't add 64bit executables for now...
		# system ('lipo', "$buildsroot/monodistribution/bin-i386/$file", "$buildsroot/monodistribution/bin-x86_64/$file", '-create', '-output', "$buildsroot/monodistribution/bin/$file");
	}

	if ($ENV{"UNITY_THISISABUILDMACHINE"}) {
		for my $arch (@arches) {
			# Clean up temporary arch-specific directories
			rmtree("$embeddir/$os-$arch");
			rmtree("$distdir-$arch");
		}
	}
}

sub build_iphone_crosscompiler
{
	my $os = "crosscompiler";
	mkpath ("$buildsroot/$os/iphone");

	my $sdkversion = '10.6';

	for my $arch ('i386') {
		my $buildtarget = "$buildir/$os-$arch";
		my $cachefile = "$buildir/$os-$arch.cache";

		print "Building $os for architecture: $arch\n";

		my @configureparams = setenv_iphone_crosscompiler ($arch, $cachefile, $sdkversion);
		build_mono ($arch, $buildtarget, $cachefile, $os, \@configureparams);

		print "Copying mono runtime to final destination";
		for my $file ('mono') {
			system("ln","-f","$buildtarget/mono/mini/$file","$buildsroot/$os/iphone/$file-xcompiler") eq 0 or die("failed symlinking $buildtarget/mono/mini/$file to $buildsroot/$os/iphone/$file-xcompiler");
		}
	}

	$ENV{"MACSDKOPTIONS"} = "";
	$ENV{"MACSYSROOT"} = "";
}

sub build_iphone_runtime
{
	my $os = "iphone";
	mkpath ("$embeddir/$os");

	my $macversion = '10.6';
	my $sdkversion = '5.0';


	for my $arch ('armv7') {
		my $buildtarget = "$buildir/$os-$arch";
		my $cachefile = "$buildir/$os-$arch.cache";

		print "Building $os for architecture: $arch\n";

		if (not $skipbuild)
		{
			my @configureparams = setenv_iphone_runtime ($arch, $cachefile, $sdkversion);
			build_mono ($arch, $buildtarget, $cachefile, $os, \@configureparams);
		}

		print "Copying iPhone static lib to final destination";
		system("ln","-f","$buildtarget/mono/mini/.libs/libmono.a","$embeddir/$os/libmono-$arch.a") eq 0 or die("failed symlinking libmono-$arch.a");

	}

	for my $file ('libmono') {
		system("libtool", "-static", "-o", "$embeddir/$os/$file.a", "$embeddir/$os/$file-armv7.a") eq 0 or dir("failed libtool");
		system("rm", "$embeddir/$os/$file-armv7.a");
	}


	$ENV{"MACSDKOPTIONS"} = "";
	$ENV{"MACSYSROOT"} = "";
}

sub build_iphone_simulator
{
	my $os = "iphone";
	mkpath ("$embeddir/$os");


	for my $arch ('i386') {
		my $buildtarget = "$buildir/$os-$arch";
		my $cachefile = "$buildir/$os-$arch.cache";


		print "Building $os for architecture: $arch\n";

		my $macversion = '10.6';
		my $sdkversion = '5.0';

		print("buildtarget: $buildtarget\n");

		if (not $skipbuild)
		{
			my @configureparams = setenv_iphone_simulator ($arch, $cachefile, $sdkversion);
			build_mono ($arch, $buildtarget, $cachefile, $os, \@configureparams);
		}

		print "Copying iPhone static lib to final destination";
		system("ln","-f","$buildtarget/mono/mini/.libs/libmono.a","$embeddir/$os/libmono-$arch.a") eq 0 or die("failed symlinking libmono-$arch.a");
	}

	$ENV{"MACSDKOPTIONS"} = "";
	$ENV{"MACSYSROOT"} = "";
}

if ($cleanbuild && not $skipbuild)
{
	configure_mono;
	mkpath("$buildir");
}

my $doiphone;
my $doiphonex;
my $doiphones;
my $doosx;

$doiphone = 1 if $dobuild eq 'runtime';
$doiphonex = 1 if $dobuild eq 'cross';
$doiphones = 1 if $dobuild eq 'simulator';
$doiphone = $doiphones = $doiphonex = 1 if $dobuild eq 'iphone';
$doosx = 1 if $dobuild eq 'osx' || (not $doiphone) && (not $doiphones) && (not $doiphonex);

build_iphone_simulator if $doiphones;
build_iphone_runtime if $doiphone;
build_iphone_crosscompiler if $doiphonex;
build_osx if $doosx;
