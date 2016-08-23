use Cwd;
use File::Path;
my $root = getcwd();
my $externalBuildDeps = "$root/../../mono-build-deps/build";

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

	my $sdkversion = "10.6";
	my $sdkPath = "$externalBuildDeps/MacBuildEnvironment/builds/MacOSX$sdkversion.sdk";
	if (! -d $sdkPath)
	{
		print("Unzipping mac build toolchain\n");
		system('unzip', '-qd', "$externalBuildDeps/MacBuildEnvironment", "$externalBuildDeps/MacBuildEnvironment/builds.zip");
	}
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
