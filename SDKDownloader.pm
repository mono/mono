#!/usr/bin/perl

package SDKDownloader;

use strict;
use warnings;

use Cwd;
use File::Basename qw(dirname basename);
use File::Path qw(mkpath rmtree);
use File::Spec::Functions;
use File::Copy;
use File::Basename;
use Getopt::Long;
use lib File::Spec->rel2abs(dirname(__FILE__)).'/.';

# Override this value to point to a local directory for local testing
my $base_url = "https://rhodecode.unity3d.com/unity-extra/";
my $base_url_mirror = "http://mercurial-mirror.hq.unity3d.com/unity-extra/";

# SDKDownloader fetches the default branch of a remote repository located at $base_url/$repo_name.
# The default branch contains an SDK.pm which specifies the version of the SDK along with branch information.
# It also has methods to install the sdk and check if it is installed.
# Named branches of the repository contain different versions of the sdk for different platforms.
# If necessary, this script clones the appropriate branch for the given version and installs the SDK.
# Version number is set in SDK.pm but can be overriden via $sdk_override passed to PepareSDK.
# By default the install location is set to HOME but each SDK can override this, and it can be overriden
# by setting the environment variable UNITY_SDK_LOCATION.

# https://rhodecode.unity3d.com/unity-extra/stv-sdk
#  - branch: default
#   - SDK.pm contains the following:
#
#     our %default_values = (
#      	# Current version of the sdk that everything will build with unless sdk-override is specified.
#      	# Change this to update SDK globally.
#      	version => "r03",
#
#      	# Format of the branches for updating
#      	#  \$version is replaced with the version number
#      	#  \$platform is replaced with the platform name
#      	branch => "stv-ndk-\$version-\$platform",
#
#      	# Format of the archive that exists after updating to the above branch.
#      	# The same substition applies as above.
#      	archive => "stv-ndk-\$version-\$platform.tar.bz2",
#
#      	# The format of the above archive.
#      	# Valid values:  zip, gzip, bz2
#      	compress_format => "bz2",
#
#      	# The following are filled out by the SDKDownloader.pm
#      	install_path => "",
#     );
#
#    sub IsCorrectVersionInstalled
#       - Determine if the SDK for $config->{version} is installed.
#
#    sub InstallSDK
#       - Install the SDK for $config->{version}.
#       - SDK has already been cloned to $sdk_download->{clone_path}
#       - SDK has already been extracted to $sdk_download->{unpack_path}
#       - This should install it to $config->{install_path} if necessary, or put it wherever it goes.
#
#    sub SetupSDK
#       - This is called on each build after the SDK is confirmed to be installed.
#       - This can do whatever per build setup you need to do here such as setting ENV vars.
#
# - branch: stv-ndk-r03-linux
#  - stv-ndk-r03-linux.tar.bz2
#
# The named branches have one platform / revision archive each.  They are discovered via the patterns
# in SDK.pm's default_values' branch / archive strings.

# Main entry point to SDKDownloader system:
# repo_name: name of the repo at $base_url
# sdk_override: SDK revision to install.  Leave blank to install the "default" (from SDK.pm)
# artifacts_folder: Place to pull and update the inividual SDK repos.
sub PrepareSDK
{
	my ($repo_name, $sdk_override, $artifacts_folder) = @_;

	# let the user decide to use the SDK that's locally installed.
	# Handy if the connection to the repo is really slow or unavailable.
	if( $sdk_override eq "local" )
	{
		print "[SDKDownloader] using local SDK.\n";
		return;
	}
	
	$repo_name || die ("ERROR: repo_name not set");

	my $cwd = getcwd;

	my $dir = File::Spec->rel2abs (dirname ($0));
	if ($artifacts_folder)
	{
		$dir = catfile($artifacts_folder, "SDKDownloader");
		mkpath ($dir);
	}
	chdir ($dir);

	# Obtain platform specific information
	my $host_config = GetHostDetails();

	print "[SDKDownloader] Begin SDK check: $repo_name ...\n";
	my $vcs_url = GetBaseURL() . $repo_name;

	# Clones / Updates the specific SDK repo which contains the sdk perl module
	UpdateSDKRepo($repo_name, $vcs_url, $dir);

	# Load in the SDK specific perl module
	my $module = "SDK.pm";
	require $module;

	# Obtain default SDK configuration so we can fill out values
	\%SDK::default_values || die ("ERROR: SDK.pm does not contain default values.");
	my $sdk_config = \%SDK::default_values;

	# apply sdk override if necessary
	$sdk_config->{version} = $sdk_override if $sdk_override;

	# set the branch/archive name given our current version and platform
	$sdk_config->{branch} =~ s/\$version/$sdk_config->{version}/;
	$sdk_config->{branch} =~ s/\$platform/$host_config->{platform}/;

	$sdk_config->{archive} =~ s/\$version/$sdk_config->{version}/;
	$sdk_config->{archive} =~ s/\$platform/$host_config->{platform}/;

	$sdk_config->{install_path} = $host_config->{install_path};
	$sdk_config->{host_config} = $host_config;
	$sdk_config->{vcs_url} = $vcs_url;

	# Determine if the SDK for $version is installed.
	if (SDK::IsCorrectVersionInstalled ($sdk_config))
	{
		print "[SDKDownloader] SDK $sdk_config->{version} is already installed ...\n";
	}
	else
	{
		my $sdk_download = DownloadAndExtractSDK ($sdk_config, $repo_name);

		# Install the SDK for $version.
		# SDK has already been cloned to $sdk_download->{clone_path}
		# SDK has already been extracted to $sdk_download->{unpack_path}
		# This should install it to $sdk_config->{install_path} if necessary, or put it wherever it goes.
		SDK::InstallSDK ($sdk_config, $sdk_download);

		CleanupSDKDownload ($sdk_download);

		if (!SDK::IsCorrectVersionInstalled($sdk_config))
		{
			die ("Failed to install SDK.  Something is wrong with $vcs_url SDK.pm?");
		}
	}

	# This is called on each build after the SDK is confirmed to be installed.
	# This can do whatever per build setup you need to do here such as setting ENV vars.
	print "[SDKDownloader] Setup SDK ...\n";
	SDK::SetupSDK ($sdk_config);

	print "[SDKDownloader] Setup complete for $repo_name at version $sdk_config->{version}.\n";
	chdir ($cwd);
}

# Pulls / Updates the SDK repo $vcs_url to $dir (artifacts) if necessary.
# We avoid calling hg update every build by caching the parent revision and only updating when it changes.
sub UpdateSDKRepo
{
	my ($repo_name, $vcs_url, $dir) = @_;

	print "[SDKDownloader] Checking if we need to update $vcs_url ...\n";

	# We want to avoid calling hg update every time we build, so cache the parent revision
	# and only update if it changes

	my $parent_rev = "parent";

	# check if we're in a mercurial repo
	if (system("hg parent --template \"{node}\"") == 0)
	{
		$parent_rev = `hg parent --template "{node}"`;
	}
	# check if we're in a git repo
	elsif (system("git rev-parse HEAD") == 0)
	{
		$parent_rev = `git rev-parse HEAD`;
	}
	# else we'll just pull every time

	my $old_rev = "";

	if (open (REV, "<$repo_name-rev.txt"))
	{
		$old_rev = <REV>;
		close (REV);
	}

	# If the sdk repo already exists just update it
	my $updated = 0;
	if (-d $repo_name)
	{
		print "[SDKDownloader] \tOLD REV: $old_rev\n";
		print "[SDKDownloader] \tPARENT_REV: $parent_rev\n";
		chdir ($repo_name);
		if ($old_rev ne $parent_rev)
		{
			print "[SDKDownloader] Updating $vcs_url (branch default) at $dir ...\n\n";
			system ("hg pull $vcs_url") && die ("ERROR: can't hg pull $vcs_url");
			system ("hg update") && die ("ERROR: can't hg update $vcs_url");
			$updated = 1;
		}
	}
	else
	{
		print "[SDKDownloader] Cloning $vcs_url (branch default) to $dir ...\n";
		system ("hg clone $vcs_url") && die ("ERROR: can't hg clone $vcs_url");
		chdir ($repo_name);
		$updated = 1;
	}

	if ($updated)
	{
		open (REV, ">../$repo_name-rev.txt") or die ("ERROR: Couldn't open $repo_name-rev.txt for writing.");
		print REV $parent_rev;
		close (REV);
	}
	else
	{
		print "[SDKDownloader] Repo has not changed parent since last build, no need to get latest $repo_name.\n";
	}
}

# Pulls and extracts a specific revision of the SDK.
# sdk_config:
#  version: version of sdk to install
#  branch: branch name of repo to clone
#  archive: archive name in repo to extract
#  compress_format: format of archive
# temp_folder_name: basename of folder to use
# returns:
#   sdk_download:
#     clone_path: location the sdk repo is cloned to
#     unpack_path: location the archive is extracted to
#
# NOTE: must call CleanupSDKDownload to remove clone_path and unpack_path when done.
sub DownloadAndExtractSDK
{
	my ($sdk_config, $temp_folder_name) = @_;
	my $host_config = $sdk_config->{host_config};
	my $vcs_url = $sdk_config->{vcs_url};

	# Download and extract SDK
	my $temp_clone_path = catfile ($host_config->{tmp}, $temp_folder_name);
	my $temp_archive_file = catfile ($temp_clone_path, $sdk_config->{archive});
	my $temp_unpack_path = catfile ($host_config->{tmp}, $temp_folder_name . "_unpack");

	print "[SDKDownloader]\tInstalling SDK $sdk_config->{version} ...\n";
	print "[SDKDownloader]\t\tTmp DL: " . $temp_clone_path . "\n";
	print "[SDKDownloader]\t\tTmp DL File: " . $temp_archive_file . "\n";
	print "[SDKDownloader]\t\tTmp unpack: " . $temp_unpack_path . "\n";

	rmtree ($temp_clone_path);
	rmtree ($temp_unpack_path);
	mkpath ($temp_clone_path);
	mkpath ($temp_unpack_path);

	# obtain sdk
	print "\n[SDKDownloader]\t\tCloning SDK $vcs_url (branch $sdk_config->{branch}) ...\n";
	system ("hg clone -b $sdk_config->{branch} $vcs_url $temp_clone_path") && die ("ERROR: can't hg clone -b $sdk_config->{branch} $vcs_url $temp_clone_path");

	# extract sdk
	my $sdk_compressed_format = $sdk_config->{compress_format};
	print "[SDKDownloader]\t\tCompress format: $sdk_compressed_format\n";
	my $uncompress = $host_config->{uncompressors}{$sdk_compressed_format};
	$uncompress =~ s/\$ARCHIVE/$temp_archive_file/;
	$uncompress =~ s/\$OUT_DIR/$temp_unpack_path/;

	print "[SDKDownloader]\t\tExtracting $temp_archive_file => $temp_unpack_path ...\n";
	print "[SDKDownloader]\t\t\t$uncompress\n";
	system ($uncompress);

	unlink ($temp_archive_file);

	my %sdk_download = (
		clone_path => $temp_clone_path,
		unpack_path => $temp_unpack_path,
	);

	return \%sdk_download;
}

# Removes temporary files from DownloadAndExtractSDK.
sub CleanupSDKDownload
{
	my ($sdk_download) = @_;
	rmtree ($sdk_download->{unpack_path});
	rmtree ($sdk_download->{clone_path});
}

# Returns information about the host environment.
sub GetHostDetails
{
	my $HOST_ENV;
	my $TMP;
	my $HOME;
	my %UNCOMPRESSORS = (
		"zip" => "unzip -d \$OUT_DIR \$ARCHIVE",
		"gzip" => "tar -C \$OUT_DIR -xf \$ARCHIVE",
		"bz2" => "tar -C \$OUT_DIR -xf \$ARCHIVE",
		"7z" => "7za x \$ARCHIVE -o\$OUT_DIR",
	);

	if (lc $^O eq 'darwin')
	{
		$HOST_ENV = "macosx";
		$TMP = $ENV{"TMPDIR"};
		$HOME = $ENV{"HOME"};
	}
	elsif (lc $^O eq 'linux')
	{
		$HOST_ENV = "linux";
		$TMP = "/tmp";
		$HOME = $ENV{"HOME"};
	}
	elsif (lc $^O eq 'mswin32')
	{
		$HOST_ENV = "windows";
		$TMP = $ENV{"TMP"};
		$HOME = $ENV{"USERPROFILE"};
		my $WINZIP = "7z.exe";
		if (-e "Tools/WinUtils/7z/7z.exe")
		{
			$WINZIP = "Tools/WinUtils/7z/7z.exe";
		}
		%UNCOMPRESSORS = (
			"zip" => "$WINZIP x \$ARCHIVE -o\$OUT_DIR",
			"gzip" => "$WINZIP x -so -tgzip \$ARCHIVE | $WINZIP x -si -ttar -o\$OUT_DIR",
			"bz2" => "$WINZIP x -so -tbzip2 \$ARCHIVE | $WINZIP x -si -ttar -o\$OUT_DIR",
			"7z" => "$WINZIP x \$ARCHIVE -o\$OUT_DIR",
		);
	}
	elsif (lc $^O eq 'cygwin')
	{
		$HOST_ENV = "windows";
		$TMP = $ENV{"TMP"};
		$HOME = $ENV{"HOME"};
	}
	else
	{
		die "UNKNOWN " . $^O;
	}

	# override home location
	if ($ENV{UNITY_SDK_LOCATION})
	{
		$HOME = $ENV{UNITY_SDK_LOCATION};
	}

	my %host_config = (platform => $HOST_ENV,
		tmp => $TMP,
		install_path => $HOME,
		uncompressors => \%UNCOMPRESSORS);

	return \%host_config;
}

# Returns HG url
sub GetBaseURL
{
	if ($ENV{UNITY_THISISABUILDMACHINE})
	{
		return $base_url_mirror;
	}
	return $base_url;
}

sub ParseCmdline
{
	my (@ARGV) = @_;
	my ($repo_name, $sdk_override, $artifacts_folder);

	GetOptions (
		"repo_name=s" => \$repo_name,
		"sdk_override=s" => \$sdk_override,
		"artifacts_folder=s" => \$artifacts_folder,
	) or die ("could not parse commandline");

	PrepareSDK($repo_name, $sdk_override, $artifacts_folder);
}

__PACKAGE__->ParseCmdline(@ARGV) unless caller;

1;
