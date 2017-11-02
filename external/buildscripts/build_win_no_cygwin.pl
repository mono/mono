use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;
use File::Copy;
use lib ('external/buildscripts', "../../Tools/perl_lib","perl_lib", 'external/buildscripts/perl_lib');
use Tools qw(InstallNameTool);

print ">>> PATH in Build All = $ENV{PATH}\n\n";

my $currentdir = getcwd();

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);

$monoroot =~ tr{/}{\\};

print ">>> monoroot = $monoroot\n";

my $buildscriptsdir = "$monoroot\\external\\buildscripts";
my $addtoresultsdistdir = "$buildscriptsdir\\add_to_build_results\\monodistribution";
my $monoprefix = "$monoroot\\tmp\\monoprefix";
my $buildsroot = "$monoroot\\builds";
my $distdir = "$buildsroot\\monodistribution";
my $buildMachine = $ENV{UNITY_THISISABUILDMACHINE};

my $build=0;
my $clean=0;
my $artifact=0;
my $debug=0;
my $checkoutOnTheFly=0;
my $forceDefaultBuildDeps=0;
my $existingMonoRootPath = '';
my $arch32 = 0;
my $winPerl = "perl";
my $winMonoRoot = $monoroot;
my $msBuildVersion = "14.0";
my $buildDeps = "";

print(">>> Build All Args = @ARGV\n");

GetOptions(
	'build=i'=>\$build,
	'clean=i'=>\$clean,
	'artifact=i'=>\$artifact,
	'debug=i'=>\$debug,
	'arch32=i'=>\$arch32,
	'existingmono=s'=>\$existingMonoRootPath,
	'winperl=s'=>\$winPerl,
	'winmonoroot=s'=>\$winMonoRoot,
	'msbuildversion=s'=>\$msBuildVersion,
	'checkoutonthefly=i'=>\$checkoutOnTheFly,
	'builddeps=s'=>\$buildDeps,
	'forcedefaultbuilddeps=i'=>\$forceDefaultBuildDeps,
) or die ("illegal cmdline options");

my $monoRevision = `git rev-parse HEAD`;
chdir("$buildscriptsdir") eq 1 or die ("failed to chdir : $buildscriptsdir\n");
my $buildScriptsRevision = `git rev-parse HEAD`;
chdir("$monoroot") eq 1 or die ("failed to chdir : $monoroot\n");

print(">>> Mono Revision = $monoRevision\n");
print(">>> Build Scripts Revision = $buildScriptsRevision\n");

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

my $existingExternalMonoRoot = "$externalBuildDeps\\mono";
my $existingExternalMono = "$existingExternalMonoRoot\\win";

if ($clean)
{
	print(">>> Cleaning $monoprefix\n");
	rmtree($monoprefix);
}

# *******************  Build Stage  **************************

if ($build)
{
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
				my $SevenZip = "$externalBuildDeps/7z/win64/7za.exe";
				print(">>> Using 7z : $SevenZip\n");
				system("$SevenZip", "x", "$existingExternalMono/builds.zip", "-o$existingExternalMono") eq 0 or die("Failed extracting mono builds.zip\n");
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

	system("$winPerl", "$winMonoRoot/external/buildscripts/build_runtime_vs.pl", "--build=$build", "--arch32=$arch32", "--msbuildversion=$msBuildVersion", "--clean=$clean", "--debug=$debug", "--gc=bdwgc") eq 0 or die ('failed building mono bdwgc with VS\n');
	system("$winPerl", "$winMonoRoot/external/buildscripts/build_runtime_vs.pl", "--build=$build", "--arch32=$arch32", "--msbuildversion=$msBuildVersion", "--clean=$clean", "--debug=$debug", "--gc=sgen") eq 0 or die ('failed building mono sgen with VS\n');

	if (!(-d "$monoroot\\tmp"))
	{
		print(">>> Creating directory $monoroot\\tmp\n");
		system("mkdir $monoroot\\tmp") eq 0 or die ("failing creating $monoroot\\tmp\n");;
	}

	if (!(-d "$monoprefix"))
	{
		print(">>> Creating directory $monoprefix\n");
		system("mkdir $monoprefix") eq 0 or die ("failing creating $monoprefix\n");;
	}

	if (!(-d "$monoprefix\\bin"))
	{
		print(">>> Creating directory $monoprefix\\bin\n");
		system("mkdir $monoprefix\\bin") eq 0 or die ("failing creating $monoprefix\\bin\n");;
	}

	# Copy over the VS built stuff that we want to use instead into the prefix directory
	my $archNameForBuild = $arch32 ? 'Win32' : 'x64';
	my $configDirName = $debug ? "Debug" : "Release";

	copy("$monoroot/msvc/build/bdwgc/$archNameForBuild/bin/$configDirName/mono-bdwgc.exe", "$monoprefix/bin/.") or die ("failed copying mono-bdwgc.exe\n");
	copy("$monoroot/msvc/build/bdwgc/$archNameForBuild/bin/$configDirName/mono-2.0-bdwgc.dll", "$monoprefix/bin/.") or die ("failed copying mono-2.0-bdwgc.dll\n");
	copy("$monoroot/msvc/build/bdwgc/$archNameForBuild/bin/$configDirName/mono-2.0-bdwgc.pdb", "$monoprefix/bin/.") or die ("failed copying mono-2.0-bdwgc.pdb\n");

	copy("$monoroot/msvc/build/sgen/$archNameForBuild/bin/$configDirName/mono-sgen.exe", "$monoprefix/bin/.") or die ("failed copying mono-sgen.exe\n");
	copy("$monoroot/msvc/build/sgen/$archNameForBuild/bin/$configDirName/mono-2.0-sgen.dll", "$monoprefix/bin/.") or die ("failed copying mono-2.0-sgen.dll\n");
	copy("$monoroot/msvc/build/sgen/$archNameForBuild/bin/$configDirName/mono-2.0-sgen.pdb", "$monoprefix/bin/.") or die ("failed copying mono-2.0-sgen.pdb\n");

	# sgen as default exe
	copy("$monoroot/msvc/build/sgen/$archNameForBuild/bin/$configDirName/mono-sgen.exe", "$monoprefix/bin/mono.exe") or die ("failed copying mono-sgen.exe to mono.exe\n");

	copy("$monoroot/msvc/build/bdwgc/$archNameForBuild/bin/$configDirName/MonoPosixHelper.dll", "$monoprefix/bin/.") or die ("failed copying MonoPosixHelper.dll\n");
	copy("$monoroot/msvc/build/bdwgc/$archNameForBuild/bin/$configDirName/MonoPosixHelper.pdb", "$monoprefix/bin/.") or die ("failed copying MonoPosixHelper.pdb\n");

	system("xcopy /y /f $addtoresultsdistdir\\bin\\*.* $monoprefix\\bin\\") eq 0 or die ("Failed copying $addtoresultsdistdir/bin to $monoprefix/bin\n");
}

# *******************  Artifact Stage  **************************

if ($artifact)
{
	print(">>> Creating artifact...\n");

	# Do the platform specific logic to create the builds output structure that we want

	my $embedDirRoot = "$buildsroot\\embedruntimes";

	my $embedDirArchDestination = $arch32 ? "$embedDirRoot\\win32" : "$embedDirRoot\\win64";
	my $distDirArchBin = $arch32 ? "$distdir\\bin" : "$distdir\\bin-x64";
	my $versionsOutputFile = $arch32 ? "$buildsroot\\versions-win32.txt" : "$buildsroot\\versions-win64.txt";

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

	if (!(-d "$buildsroot"))
	{
		print(">>> Creating directory $buildsroot\n");
		system("mkdir $buildsroot") eq 0 or die("failed to create directory $buildsroot\n");
	}

	if (!(-d "$embedDirRoot"))
	{
		print(">>> Creating directory $embedDirRoot\n");
		system("mkdir $embedDirRoot") eq 0 or die("failed to create directory $embedDirRoot\n");
	}

	if (!(-d "$distdir"))
	{
		print(">>> Creating directory $distdir\n");
		system("mkdir $distdir") eq 0 or die("failed to create directory $distdir\n");
	}

	print(">>> Creating directory $embedDirArchDestination\n");
	system("mkdir $embedDirArchDestination") eq 0 or die("failed to create directory $embedDirArchDestination\n");

	print(">>> Creating directory $distDirArchBin\n");
	system("mkdir $distDirArchBin") eq 0 or die("failed to create directory $distDirArchBin\n");

	# embedruntimes directory setup
	print(">>> Creating embedruntimes directory : $embedDirArchDestination\n");

	copy("$monoprefix/bin/mono-2.0-bdwgc.dll", "$embedDirArchDestination/.") or die ("failed copying mono-2.0-bdwgc.dll\n");
	copy("$monoprefix/bin/mono-2.0-bdwgc.pdb", "$embedDirArchDestination/.") or die ("failed copying mono-2.0-bdwgc.pdb\n");

	copy("$monoprefix/bin/mono-2.0-sgen.dll", "$embedDirArchDestination/.") or die ("failed copying mono-2.0-sgen.dll\n");
	copy("$monoprefix/bin/mono-2.0-sgen.pdb", "$embedDirArchDestination/.") or die ("failed copying mono-2.0-sgen.pdb\n");

	copy("$monoprefix/bin/MonoPosixHelper.dll", "$embedDirArchDestination/.") or die ("failed copying MonoPosixHelper.dll\n");
	copy("$monoprefix/bin/MonoPosixHelper.pdb", "$embedDirArchDestination/.") or die ("failed copying MonoPosixHelper.pdb\n");

	# monodistribution directory setup
	print(">>> Creating monodistribution directory\n");
	copy("$monoprefix/bin/mono-2.0-bdwgc.dll", "$distDirArchBin/.") or die ("failed copying mono-2.0-bdwgc.dll\n");
	copy("$monoprefix/bin/mono-2.0-bdwgc.pdb", "$distDirArchBin/.") or die ("failed copying mono-2.0-bdwgc.pdb\n");

	copy("$monoprefix/bin/mono-2.0-sgen.dll", "$distDirArchBin/.") or die ("failed copying mono-2.0-sgen.dll\n");
	copy("$monoprefix/bin/mono-2.0-sgen.pdb", "$distDirArchBin/.") or die ("failed copying mono-2.0-sgen.pdb\n");

	copy("$monoprefix/bin/mono-sgen.exe", "$distDirArchBin/.") or die ("failed copying mono-sgen.exe\n");
	copy("$monoprefix/bin/mono-bdwgc.exe", "$distDirArchBin/.") or die ("failed copying mono-bdwgc.exe\n");
	copy("$monoprefix/bin/mono.exe", "$distDirArchBin/.") or die ("failed copying mono.exe\n");

	copy("$monoprefix/bin/MonoPosixHelper.dll", "$distDirArchBin/.") or die ("failed copying MonoPosixHelper.dll\n");
	copy("$monoprefix/bin/MonoPosixHelper.pdb", "$distDirArchBin/.") or die ("failed copying MonoPosixHelper.pdb\n");


	# Output version information
	print(">>> Creating version file : $versionsOutputFile\n");
	open(my $fh, '>', $versionsOutputFile) or die "Could not open file '$versionsOutputFile' $!";
	say $fh "mono-version =";
	my $monoVersionInfo = `$distDirArchBin\\mono --version`;
	say $fh "$monoVersionInfo";
	say $fh "unity-mono-revision = $monoRevision";
	say $fh "unity-mono-build-scripts-revision = $buildScriptsRevision";
	my $tmp = `date /T`;
	say $fh "$tmp";
	close $fh;
}
else
{
	print(">>> Skipping artifact creation\n");
}
