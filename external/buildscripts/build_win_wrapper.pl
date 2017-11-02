use Cwd;
use Cwd 'abs_path';
use Getopt::Long;
use File::Basename;
use File::Path;
use Config;

print ">>> My Path: $ENV{PATH}\n\n";

my $monoroot = File::Spec->rel2abs(dirname(__FILE__) . "/../..");
my $monoroot = abs_path($monoroot);
my $buildScriptsRoot = "$monoroot/external/buildscripts";
print ">>> Mono checkout found in $monoroot\n\n";

my $cygwinRootWindows = "";
my $monoInstallLinux = "";
my $checkoutOnTheFly=0;
my $buildDeps = "";
my $forceDefaultBuildDeps = 0;

my @thisScriptArgs = ();
my @passAlongArgs = ();
foreach my $arg (@ARGV)
{
	push @backupArgs, $arg;

	if ($arg =~ /^--cygwin=/)
	{
		push @thisScriptArgs, $arg;
	}
	elsif ($arg =~ /^--existingmono=/)
	{
		push @thisScriptArgs, $arg;
	}
	elsif ($arg =~ /^--checkoutonthefly=/)
	{
		push @thisScriptArgs, $arg;
		push @passAlongArgs, $arg;
	}
	elsif ($arg =~ /^--builddeps=/)
	{
		push @thisScriptArgs, $arg;
		push @passAlongArgs, $arg;
	}
	elsif ($arg =~ /^--forcedefaultbuilddeps=/)
	{
		push @thisScriptArgs, $arg;
		push @passAlongArgs, $arg;
	}
	else
	{
		push @passAlongArgs, $arg;
	}
}

print(">>> This Script Args = @thisScriptArgs\n");
print(">>> Pass Along Args = @passAlongArgs\n");

@ARGV = @thisScriptArgs;
GetOptions(
	'cygwin=s'=>\$cygwinRootWindows,
	'existingmono=s'=>\$monoInstallLinux,
	'checkoutonthefly=i'=>\$checkoutOnTheFly,
	'builddeps=s'=>\$buildDeps,
	'forcedefaultbuilddeps=i'=>\$forceDefaultBuildDeps,
);

my $externalBuildDeps = "";
my $externalBuildDepsIl2Cpp = "$monoroot/../../il2cpp/build";

if ($buildDeps ne "")
{
	$externalBuildDeps = $buildDeps;
}
else
{
	if (-d "$monoroot/../../mono-build-deps/build" || $forceDefaultBuildDeps)
	{
		$externalBuildDeps = "$monoroot/../../mono-build-deps/build";
	}

	if (!(-d "$externalBuildDeps"))
	{
		if (not $checkoutonthefly && $cygwinRootWindows eq "")
		{
			print(">>> No external build deps found and --cygwin not used.  Might as well try to check them out.  If the checkout fails, we'll continue, but the build will probably fail\n");
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
}

print(">>> externalBuildDeps = $externalBuildDeps\n");

my $SevenZip = "$externalBuildDeps/7z/win64/7za.exe";

# Attempt to find common default cygwin install locations
if ($cygwinRootWindows eq "")
{
	print(">>> No cygwin install specified.  Looking for defaults...\n");

	my $externalCygwin = "$externalBuildDeps/cygwin64/builds";
	my $externalCygwinZip = "$externalBuildDeps/cygwin64/builds.zip";

	if (-d "$externalCygwin")
	{
		$cygwinRootWindows = $externalCygwin;
		print(">>> Found Cygwin at : $cygwinRootWindows\n");
	}
	elsif(-f "$externalCygwinZip")
	{
		print(">>> Found unextracted cygwin builds.zip : $externalCygwinZip\n");
		print(">>> Using 7z : $SevenZip\n");
		print(">>> Extracting...\n");
		system("$SevenZip", "x", "$externalCygwinZip", "-o$externalBuildDeps/cygwin64") eq 0 or die("Failed extracting cygwin\n");
		$cygwinRootWindows = $externalCygwin;
	}
	else
	{
		if ($forceDefaultBuildDeps)
		{
			die("\nCould not fined Cygwin in default external build deps location : $externalBuildDeps\n")
		}
		else
		{
			if (-d "C:\\Cygwin64")
			{
				$cygwinRootWindows = "C:\\Cygwin64";
				print(">>> Found Cygwin at : $cygwinRootWindows\n");
			}
			elsif (-d "C:\\Cygwin")
			{
				$cygwinRootWindows = "C:\\Cygwin";
				print(">>> Found Cygwin at : $cygwinRootWindows\n");
			}
			else
			{
				die("\nCould not fined Cygwin.  Define path using --cygwin=<path>\n")
			}
		}
	}
}
else
{
	print(">>> Cygwin Path = $cygwinRootWindows\n");
}

if ($monoInstallLinux eq "")
{
	print(">>> No mono install specified.  Looking for defaults...\n");

	my $externalMono = "$externalBuildDeps/mono/win/builds";
	my $externalMonoZip = "$externalBuildDeps/mono/win/builds.zip";

	if (-d "$externalMono")
	{
		$monoInstallLinux = $externalMono;
		$monoInstallLinux =~ s/\\/\//g;
		print(">>> Found Mono at : $monoInstallLinux\n");
	}
	elsif(-f "$externalMonoZip")
	{
		print(">>> Found unextracted mono builds.zip : $externalMonoZip\n");
		print(">>> Using 7z : $SevenZip\n");
		print(">>> Extracting...\n");
		system("$SevenZip", "x", "$externalMonoZip", "-o$externalBuildDeps/mono/win") eq 0 or die("Failed extracting mono\n");
		$monoInstallLinux = $externalMono;
		$monoInstallLinux =~ s/\\/\//g;
		print(">>> Found Mono at : $monoInstallLinux\n");
	}
	else
	{
		if ($forceDefaultBuildDeps)
		{
			die("\nCould not fined mono in default external build deps location : $externalBuildDeps\n")
		}
		else
		{
			if (-d "C:\\Program Files (x86)\\Mono")
			{
				# Pass over the cygwin format since I already have it escaped correctly to survive
				# crossing over the shell
				$monoInstallLinux = "/cygdrive/c/Program\\ Files\\ \\(x86\\)/Mono";
				print(">>> Found Mono at : $monoInstallLinux\n");
			}
			else
			{
				die("\n--existingmono=<path> is required and should be in the cygwin path format\n");
			}
		}
	}
}
else
{
	$monoInstallLinux =~ s/\\/\//g;
	print(">>> Linux Mono Path = $monoInstallLinux\n");
}

push @passAlongArgs, "--existingmono=$monoInstallLinux" if $monoInstallLinux ne "";

my $windowsPerl = $Config{perlpath};
print ">>> Perl Exe = $windowsPerl\n";
push @passAlongArgs, "--winperl=$windowsPerl";
push @passAlongArgs, "--winmonoroot=$monoroot";

# In some cases the file gets windowsified, use SHELLOPTS to avoid issues instead of dos2unixing the file, which will cause it to show up as modified by source control
$ENV{'SHELLOPTS'} = "igncr";

print ">>> Calling $cygwinRootWindows\\bin\\sh.exe with @passAlongArgs";
system("$cygwinRootWindows\\bin\\sh.exe", "$monoroot/external/buildscripts/build_win_wrapper.sh", @passAlongArgs) eq 0 or die("failed building mono\n");
