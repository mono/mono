use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;
use File::Copy;
use lib ('.', 'perl_lib', 'external/buildscripts/perl_lib');
use Tools qw(GitClone);

system("source","~/.profile");
print ">>> My Path: $ENV{PATH}\n";

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildscriptsdir = "$monoroot/external/buildscripts";
my $monoprefix = "$monoroot/tmp/monoprefix";
my $buildsroot = "$monoroot/builds";
my $buildMachine = $ENV{UNITY_THISISABUILDMACHINE};

GetOptions(
	'monoprefix=s'=>\$monoprefix,
) or die ("illegal cmdline options");

my $xbuildPath = "$monoprefix/bin/xbuild";
my $monoprefix45 = "$monoprefix/lib/mono/4.5";

if (!(-f "$xbuildPath"))
{
	die("Unable to locate xbuild at : $xbuildPath\n");
}

BuildUnityScriptFor45();

sub XBuild
{
	print(">>> Running : $monoprefix/bin/xbuild @_\n");
	system("$monoprefix/bin/xbuild", @_) eq 0 or die("Failed to xbuild @_\n");
}

sub Booc45
{
	my $commandLine = shift;
	
	system("$monoprefix/bin/mono $monoprefix45/booc.exe -debug- $commandLine") eq 0 or die("booc failed to execute: $monoprefix/bin/booc -debug- $commandLine\n");
}

sub BuildUnityScriptFor45
{
	my $booCheckout = "$monoroot/../../boo/build";
	print(">>> Using mono prefix $monoprefix45\n");
	
	# Build host is handling this
	if (!$buildMachine)
	{
		if (!(-d "$booCheckout"))
		{
			print(">>> Checking out boo\n");
			GitClone("git://github.com/Unity-Technologies/boo.git", $booCheckout, "unity-trunk");
		}
	}
	
	my $usCheckout = "$monoroot/../../unityscript/build";
	if (!$buildMachine)
	{
		if (!(-d "$usCheckout"))
		{
			print(">>> Checking out unity script\n");
			GitClone("git://github.com/Unity-Technologies/unityscript.git", $usCheckout, "unity-trunk");
		}
	}
	
	my $boocCsproj = "$booCheckout/src/booc/booc.csproj";
	if (!(-f "$boocCsproj"))
	{
		die("Unable to locate : $boocCsproj\n");
	}
	
	XBuild("$boocCsproj", "/t:Rebuild");
	
	print(">>> Mono Prefix 4.5 = $monoprefix45\n");
	foreach my $file (glob "$booCheckout/ide-build/Boo.Lang*.dll")
	{
		print(">>> Copying $file to $monoprefix45\n");
		copy($file, "$monoprefix45/.");
	}
	
	copy("$booCheckout/ide-build/booc.exe", "$monoprefix45/.");
	
	foreach my $file (glob "$buildscriptsdir/add_to_build_results/monodistribution/lib/mono/4.5/*")
	{
		print(">>> Copying $file to $monoprefix45\n");
		copy($file, "$monoprefix45/.");
		my $nameOnly = basename($file);
		system("chmod", "755", "$monoprefix45/$nameOnly");
	}
	
	Booc45("-out:$monoprefix45/Boo.Lang.Extensions.dll -noconfig -nostdlib -srcdir:$booCheckout/src/Boo.Lang.Extensions -r:System.dll -r:System.Core.dll -r:mscorlib.dll -r:Boo.Lang.dll -r:Boo.Lang.Compiler.dll");
	Booc45("-out:$monoprefix45/Boo.Lang.Useful.dll -srcdir:$booCheckout/src/Boo.Lang.Useful -r:Boo.Lang.Parser");
	Booc45("-out:$monoprefix45/Boo.Lang.PatternMatching.dll -srcdir:$booCheckout/src/Boo.Lang.PatternMatching");
	
	my $UnityScriptLangDLL = "$monoprefix45/UnityScript.Lang.dll";
	Booc45("-out:$UnityScriptLangDLL -srcdir:$usCheckout/src/UnityScript.Lang");
	
	my $UnityScriptDLL = "$monoprefix45/UnityScript.dll";
	Booc45("-out:$UnityScriptDLL -srcdir:$usCheckout/src/UnityScript -r:$UnityScriptLangDLL -r:Boo.Lang.Parser.dll -r:Boo.Lang.PatternMatching.dll");
	Booc45("-out:$monoprefix45/us.exe -srcdir:$usCheckout/src/us -r:$UnityScriptLangDLL -r:$UnityScriptDLL -r:Boo.Lang.Useful.dll");
	
	# # unityscript test suite
	# my $UnityScriptTestsCSharpDLL = "$usCheckout/src/UnityScript.Tests.CSharp/bin/Debug/UnityScript.Tests.CSharp.dll";
	# XBuild("$usCheckout/src/UnityScript.Tests.CSharp/UnityScript.Tests.CSharp.csproj", "/t:Rebuild");
	
	my $usBuildDir = "$usCheckout/build";
	
	if (!(-d "$usBuildDir"))
	{
		rmtree($usBuildDir);
	}
	
	mkdir($usBuildDir);
	
	# my $UnityScriptTestsDLL = <$usBuildDir/UnityScript.Tests.dll>;
	# Booc("-out:$UnityScriptTestsDLL -srcdir:$usCheckout/src/UnityScript.Tests -r:$UnityScriptLangDLL -r:$UnityScriptDLL -r:$UnityScriptTestsCSharpDLL -r:Boo.Lang.Compiler.dll -r:Boo.Lang.Useful.dll");
	
	# cp("$UnityScriptTestsCSharpDLL $usBuildDir/");
	print(">>> Populating Unity Script Build Directory : $usBuildDir\n");
	foreach my $file (glob "$monoprefix45/Boo.*")
	{
		print(">>> Copying $file to $usBuildDir\n");
		copy($file, "$usBuildDir/.");
	}
	
	foreach my $file (glob "$monoprefix45/UnityScript.*")
	{
		print(">>> Copying $file to $usBuildDir\n");
		copy($file, "$usBuildDir/.");
	}
	
	print(">>> Copying $monoprefix45/us.exe to $usBuildDir\n");
	copy("$monoprefix45/us.exe", "$usBuildDir/.");
}