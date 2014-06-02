use lib ('.', "perl_lib");
use Cwd;
use File::Path;
use File::Copy::Recursive qw(dircopy);
use Getopt::Long;
use File::Basename;

system("source","~/.profile");
print "My Path: $ENV{PATH}\n";

my $root = getcwd();

my $monodistro = "$root/builds/monodistribution";
my $lib = "$monodistro/lib";
my $libmono = "$lib/mono";
my $monoprefix = "$root/tmp/monoprefix";
my $xcodePath = '/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform';
my $macversion = '10.5';
my $sdkversion = '10.6';

my $dependencyBranchToUse = "unity3.0";

my $booCheckout = "external/boo";
my $cecilCheckout = "mcs/class/Mono.Cecil";
my $usCheckout = "external/unityscript";

if ($ENV{UNITY_THISISABUILDMACHINE}) {
	print "rmtree-ing $root/builds because we're on a buildserver, and want to make sure we don't include old artifacts\n";
	rmtree("$root/builds");

	# Force mono 2.6 for 1.1 profile bootstrapping
	my $external_MONO_PREFIX='/Library/Frameworks/Mono.framework/Versions/2.6.7';
	my $external_GNOME_PREFIX=$external_MONO_PREFIX;
	$ENV{'DYLD_FALLBACK_LIBRARY_PATH'}="$external_MONO_PREFIX/lib:/lib:/usr/lib";
	$ENV{'LD_LIBRARY_PATH'}="$external_MONO_PREFIX/lib";
	$ENV{'C_INCLUDE_PATH'}="$external_MONO_PREFIX/include:$external_GNOME_PREFIX/include";
	$ENV{'ACLOCAL_PATH'}="$external_MONO_PREFIX/share/aclocal";
	$ENV{'PKG_CONFIG_PATH'}="$external_MONO_PREFIX/lib/pkgconfig:$external_GNOME_PREFIX/lib/pkgconfig";
	$ENV{'PATH'}="$external_MONO_PREFIX/bin:/usr/local/bin:$ENV{'PATH'}";
} else {
	print "not rmtree-ing $root/builds, as we're not on a buildmachine\n";
}

my $unity=1;
my $monotouch=1;
my $injectSecurityAttributes=0;

my $skipbuild=0;
my $cleanbuild=1;
GetOptions(
   "skipbuild=i"=>\$skipbuild,
   "cleanbuild=i"=>\$cleanbuild,
   "unity=i"=>\$unity,
   "injectsecurityattributes=i"=>\$injectSecurityAttributes,
   "monotouch=i"=>\$monotouch,
) or die ("illegal cmdline options");



if (-d $libmono)
{
	rmtree($libmono);
} 

if (not $skipbuild)
{
	$ENV{CFLAGS}  = "$ENV{CFLAGS} -arch i386 -D_XOPEN_SOURCE";
	$ENV{CXXFLAGS}  = "$ENV{CXXFLAGS} $ENV{CFLAGS}";
	$ENV{LDFLAGS}  = "$ENV{LDFLAGS} -arch i386";
	if ($^O eq 'darwin')
	{
		$ENV{'MACSDKOPTIONS'} = "-mmacosx-version-min=$macversion -isysroot $xcodePath/Developer/SDKs/MacOSX$sdkversion.sdk";
	}

	if ($cleanbuild)
	{
		rmtree($monoprefix);
		chdir("$root/eglib") eq 1 or die ("Failed chdir 1");
		system("make","clean");
	}
	chdir("$root") eq 1 or die ("failed to chdir 2");
	if ($cleanbuild)
	{
		my $withMonotouch = $monotouch ? "yes" : "no";
		my $withUnity = $unity ? "yes" : "no";
		
		chdir("$root/eglib") eq 1 or die("failed to chdir 3");
		print(">>>Calling autoreconf in eglib\n");
		system("autoreconf -i");
		chdir("$root") eq 1 or die("failed to chdir4");
		print(">>>Calling autoreconf in mono\n");
		system("autoreconf -i");
		print(">>>Calling configure in mono\n");
		system("./configure","--prefix=$monoprefix","--with-monotouch=$withMonotouch","-with-unity=$withUnity", "--with-glib=embedded","--with-mcs-docs=no","--with-macversion=10.5", "--disable-nls") eq 0 or die ("failing autogenning mono");
		print("calling make clean in mono\n");
		system("make","clean");
	}
	system("make") eq 0 or die ("Failed running make");
	system("make install") eq 0 or die ("Failed running make install");
	# Couldn't get automake to Just Do The Right Thing
	system('make', '-C', 'scripts');
	system("cp -R scripts/*.bat $monoprefix/bin");
	print(">>>Making micro lib\n");
	chdir("$root/mcs/class/corlib") eq 1 or die("failed to chdir corlib");
	system("make PROFILE=monotouch_bootstrap") eq 0 or die ("Failed making monotouch bootstrap");
	system("make PROFILE=monotouch MICRO=1 clean") eq 0 or die ("Failed cleaning micro corlib");
	system("make PROFILE=monotouch MICRO=1") eq 0 or die ("Failed making micro corlib");
	
}
chdir ($root);

$File::Copy::Recursive::CopyLink = 0;  #make sure we copy files as files and not as symlinks, as TC unfortunately doesn't pick up symlinks.

mkpath("$libmono/2.0");
dircopy("$monoprefix/lib/mono/2.0","$libmono/2.0");
# system("rm $libmono/2.0/*.mdb");
mkpath("$libmono/micro");
system("cp $root/mcs/class/lib/monotouch/mscorlib.dll $libmono/micro") eq 0 or die("Failed to copy micro corlib");
system("cp $monoprefix/lib/mono/gac/Mono.Cecil/*/Mono.Cecil.dll $libmono/2.0") eq 0 or die("failed to copy Mono.Cecil.dll");
system("cp -r $monoprefix/bin $monodistro/") eq 0 or die ("failed copying bin folder");

system("cp -r $monoprefix/etc $monodistro/") eq 0 or die("failed copy 4");
mkpath("$root/builds/headers/mono");
system("cp -r $monoprefix/include/mono-1.0/mono $root/builds/headers/") eq 0 or die("failed copy 5");
system("cp $root/eglib/src/glib.h $root/builds/headers/") eq 0 or die("failed copying glib.h");
system("cp $root/eglib/src/eglib-config.hw $root/builds/headers/") eq 0 or die ("failed copying eglib-config.hw");
system('perl -e \"s/\\bmono_/mangledmono_/g;\" -pi $(find '.$root.'/builds/headers -type f)');

sub CopyIgnoringHiddenFiles
{
	my $sourceDir = shift;
	my $targetDir = shift;

	#really need to find a better way to copy a dir, ignoring .svn's than rsync.	
	system("rsync -a -v --exclude='.*' $sourceDir $targetDir") eq 0 or die("failed to rsync $sourceDir to $targetDir");
}

CopyIgnoringHiddenFiles("add_to_build_results/monodistribution/", "$monoprefix/");

sub cp
{
	my $cmdLine = shift;
	system("cp $cmdLine") eq 0 or die("failed to copy '$cmdLine'");
}

sub CopyAssemblies
{
	my $sourceFolder = shift; 
	my $targetFolder = shift;
	
	print "Copying assemblies from '$sourceFolder' to '$targetFolder'...\n";
	
	mkpath($targetFolder);
	cp("$sourceFolder/*.dll $targetFolder/");
	cp("$sourceFolder/*.exe $targetFolder/");
	cp("$sourceFolder/*.mdb $targetFolder/");
}

sub CopyProfileAssemblies
{
	my $sourceName = shift;
	my $targetName = shift;
	CopyProfileAssembliesToPrefix($sourceName, $targetName, $monodistro)
}

sub CopyProfileAssembliesToPrefix
{
	my $sourceName = shift;
	my $targetName = shift;
	my $prefix = shift;
	
	my $targetDir = "$prefix/lib/mono/$targetName";
	CopyAssemblies("$root/mcs/class/lib/$sourceName", $targetDir);
}

my $securityAttributesPath = "tuning/SecurityAttributes";

sub InjectSecurityAttributesOnProfile
{
	if ($injectSecurityAttributes)
	{
		my $profile = shift;
		RunXBuildTargetOnProfile("Install", $profile);
	}
}

sub XBuild
{
   system("$monoprefix/bin/xbuild", @_) eq 0 or die("Failed to xbuild @_");
}

sub RunXBuildTargetOnProfile
{
	my $target = shift;	
	my $profile = shift;
	
	XBuild("$securityAttributesPath/SecurityAttributes.proj", "/p:Profile=$profile", "/p:ProfilePrefix=$monodistro", "/t:$target") eq 0 or die("failed to run target '$target' on $profile");
}

sub PackageSecurityAttributeInjectionTools
{
	if ($injectSecurityAttributes)
	{
		my $libSecAttrs = "$lib/SecurityAttributes";
		CopyAssemblies("$securityAttributesPath/bin", $libSecAttrs);
		cp("$root/mcs/tools/security/sn.exe $libSecAttrs/");
	}
}

my $monoprefixUnity = "$monoprefix/lib/mono/unity";
my $monodistroLibMono = "$monodistro/lib/mono";
my $monodistroUnity = "$monodistroLibMono/unity";
my $monoprefixUnityWeb = "$monoprefix/lib/mono/unity_web";
my $monodistroUnityWeb = "$monodistro/lib/mono/unity_web";

sub UnityBooc
{
	my $commandLine = shift;
	
	system("$monoprefixUnity/booc -debug- $commandLine") eq 0 or die("booc failed to execute: $commandLine");
}

sub BuildUnityScriptForUnity
{
	# TeamCity is handling this
	if (!$ENV{UNITY_THISISABUILDMACHINE}) {
		GitClone("git://github.com/Unity-Technologies/boo.git", $booCheckout);
	}
	UnityXBuild("$booCheckout/src/booc/booc.csproj");
	
	cp("$booCheckout/ide-build/Boo.Lang*.dll $monoprefixUnity/");
	cp("$booCheckout/ide-build/booc.exe $monoprefixUnity/");
	UnityBooc("-out:$monoprefixUnity/Boo.Lang.Extensions.dll -noconfig -nostdlib -srcdir:$booCheckout/src/Boo.Lang.Extensions -r:System.dll -r:System.Core.dll -r:mscorlib.dll -r:Boo.Lang.dll -r:Boo.Lang.Compiler.dll");
	UnityBooc("-out:$monoprefixUnity/Boo.Lang.Useful.dll -srcdir:$booCheckout/src/Boo.Lang.Useful -r:Boo.Lang.Parser");
	UnityBooc("-out:$monoprefixUnity/Boo.Lang.PatternMatching.dll -srcdir:$booCheckout/src/Boo.Lang.PatternMatching");
	
	# micro profile version
	UnityXBuild("$booCheckout/src/Boo.Lang/Boo.Lang.csproj", "Micro-Release");
	cp("$booCheckout/src/Boo.Lang/bin/Micro-Release/Boo.Lang.dll $monodistroLibMono/micro/");
	
	if (!$ENV{UNITY_THISISABUILDMACHINE}) {
		GitClone("git://github.com/Unity-Technologies/unityscript.git", $usCheckout);
	}
	
	my $UnityScriptLangDLL = "$monoprefixUnity/UnityScript.Lang.dll";
	UnityBooc("-out:$UnityScriptLangDLL -srcdir:$usCheckout/src/UnityScript.Lang");
	
	my $UnityScriptDLL = "$monoprefixUnity/UnityScript.dll";
	UnityBooc("-out:$UnityScriptDLL -srcdir:$usCheckout/src/UnityScript -r:$UnityScriptLangDLL -r:Boo.Lang.Parser.dll -r:Boo.Lang.PatternMatching.dll");
	UnityBooc("-out:$monoprefixUnity/us.exe -srcdir:$usCheckout/src/us -r:$UnityScriptLangDLL -r:$UnityScriptDLL -r:Boo.Lang.Useful.dll");
	
	# unityscript test suite
	my $UnityScriptTestsCSharpDLL = "$usCheckout/src/UnityScript.Tests.CSharp/bin/Debug/UnityScript.Tests.CSharp.dll";
	UnityXBuild("$usCheckout/src/UnityScript.Tests.CSharp/UnityScript.Tests.CSharp.csproj", "Debug");
	
	my $usBuildDir = "$usCheckout/build";
	mkdir($usBuildDir);
	
	my $UnityScriptTestsDLL = <$usBuildDir/UnityScript.Tests.dll>;
	UnityBooc("-out:$UnityScriptTestsDLL -srcdir:$usCheckout/src/UnityScript.Tests -r:$UnityScriptLangDLL -r:$UnityScriptDLL -r:$UnityScriptTestsCSharpDLL -r:Boo.Lang.Compiler.dll -r:Boo.Lang.Useful.dll");
	
	cp("$UnityScriptTestsCSharpDLL $usBuildDir/");
	cp("$monoprefixUnity/Boo.* $usBuildDir/");
	cp("$monoprefixUnity/UnityScript.* $usBuildDir/");
	cp("$monoprefixUnity/us.exe $usBuildDir/");
	
	$ENV{MONO_EXECUTABLE} = <$monoprefix/bin/cli>;
	system(<$monoprefix/bin/nunit-console2>, "-noshadow", "-exclude=FailsOnMono", $UnityScriptTestsDLL) eq 0 or die("UnityScript test suite failed");
}
	
sub UnityXBuild
{
	my $projectFile = shift;
	
	my $optionalConfiguration = shift; 
	my $configuration = defined($optionalConfiguration) ? $optionalConfiguration : "Release";
	
	my $target = "Rebuild";
	my $commandLine = "$monoprefix/bin/xbuild $projectFile /p:CscToolExe=smcs /p:CscToolPath=$monoprefixUnity /p:MonoTouch=True /t:$target /p:Configuration=$configuration /p:AssemblySearchPaths=$monoprefixUnity";
	
	system($commandLine) eq 0 or die("Failed to xbuild '$projectFile' for unity");
}

sub GitClone
{
	my $repo = shift;
	my $localFolder = shift;
	my $branch = shift;
	$branch = defined($branch)?$branch:master;

	if (-d $localFolder) {
		return;
	}
	system("git clone --branch $branch $repo $localFolder") eq 0 or die("git clone $repo $localFolder failed!");
}

sub BuildCecilForUnity
{
	my $useCecilLight = 0;
	
	
	if ($useCecilLight) {
		
		$cecilCheckout = "external/cecil";
		if (!$ENV{UNITY_THISISABUILDMACHINE}) {
			GitClone("http://github.com/Unity-Technologies/cecil", $cecilCheckout, $dependencyBranchToUse);
		}
		
	}
	
	UnityXBuild("$cecilCheckout/Mono.Cecil.csproj");
	cp("$cecilCheckout/bin/Release/Mono.Cecil.dll $monoprefixUnity/");
		
}

sub AddRequiredExecutePermissionsToUnity
{
	my @scripts = ("smcs", "booc", "us");
	for my $script (@scripts) { 
		chmod(0777, $monoprefixUnity . "/$script");
	}
}

sub RunCSProj
{
	my $csprojnoext = shift;

    XBuild("$csprojnoext.csproj");
        
	my $dir = dirname($csprojnoext);
	my $basename = basename($csprojnoext);
	my $exe = "$dir/bin/Debug/$basename.exe";

	my @args = ();
	push(@args,"$monoprefix/bin/cli");
	push(@args,$exe);

	print("Starting $exer\n");
	my $ret = system(@args);
	print("$exe finished. exitcode: $ret\n");
	$ret eq 0 or die("Failed running $exe");
}

sub RunLinker()
{
	RunCSProj("tuning/UnityProfileShaper/UnityProfileShaper");
}

sub RunSecurityInjection
{
	RunCSProj("tuning/SecurityAttributes/DetectMethodPrivileges/DetectMethodPrivileges");
}

sub CopyUnityScriptAndBooFromUnityProfileTo20
{
	my $twozeroprofile = "$monodistro/lib/mono/2.0";
	system("cp $monodistroUnity/Boo* $twozeroprofile/") && die("failed copying");
	system("cp $monodistroUnity/boo* $twozeroprofile/") && die("failed copying");
	system("cp $monodistroUnity/us* $twozeroprofile/") && die("failed copying");
	system("cp $monodistroUnity/UnityScript* $twozeroprofile/") && die("failed copying");

}


if ($unity)
{
	CopyProfileAssembliesToPrefix("unity", "unity", $monoprefix);
	
	AddRequiredExecutePermissionsToUnity();
	BuildUnityScriptForUnity();
	BuildCecilForUnity();

	CopyAssemblies($monoprefixUnity,$monodistroUnity);
	#now, we have a functioning, raw, unity profile in builds/monodistribution/lib/mono/unity
	#we're now going to transform that into the unity_web profile by running it trough the linker, and decorating it with security attributes.	

	CopyUnityScriptAndBooFromUnityProfileTo20();

	RunLinker();
	RunSecurityInjection();
}

#Overlaying files
CopyIgnoringHiddenFiles("add_to_build_results/", "$root/builds/");

if($ENV{UNITY_THISISABUILDMACHINE})
{
	my %checkouts = (
		'mono-classlibs' => 'BUILD_VCS_NUMBER_Mono____Mono2_6_x_Unity3_x',
		'boo' => 'BUILD_VCS_NUMBER_Boo',
		'unityscript' => 'BUILD_VCS_NUMBER_UnityScript',
		'cecil' => 'BUILD_VCS_NUMBER_Cecil'
	);

	system("echo '' > $root/builds/versions.txt");
	for my $key (keys %checkouts) {
		system("echo \"$key = $ENV{$checkouts{$key}}\" >> $root/builds/versions.txt");
	}
}

#zip up the results for teamcity
chdir("$root/builds");
system("tar -hpczf ../ZippedClasslibs.tar.gz *") && die("Failed to zip up classlibs for teamcity");	
