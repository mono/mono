#!/usr/bin/perl

package PrepareAndroidSDK;

use strict;
use warnings;
use Getopt::Long;
use Carp qw(croak carp);
use File::Path qw(make_path remove_tree);
use File::Spec::Functions;
use File::Copy;
use File::Basename;

require Exporter;
our @ISA = qw(Exporter);
our @EXPORT_OK=qw(GetAndroidSDK);

our $SDK_ROOT_ENV = "ANDROID_SDK_ROOT";
our $NDK_ROOT_ENV = "ANDROID_NDK_ROOT";

our $BASE_URL_SDK = "http://dl.google.com/android/repository/";
our $BASE_URL_NDK = "http://dl.google.com/android/ndk/";

our $sdks =
{
	"android-7"		=> "android-2.1_r03-linux.zip",
	"android-8"		=> "android-2.2_r03-linux.zip",
	"android-9"		=> "android-2.3.1_r02-linux.zip",
	"android-10"	=> "android-2.3.3_r02-linux.zip",
	"android-11"	=> "android-3.0_r02-linux.zip",
	"android-12"	=> "android-3.1_r03-linux.zip",
	"android-13"	=> "android-3.2_r01-linux.zip",
	"android-14"	=> "android-14_r01.zip"
};

our $sdk_tools =
{
	"windows"		=> "tools_r15-windows.zip",
	"linux"			=> "tools_r15-linux.zip",
	"macosx"		=> "tools_r15-macosx.zip",
};

our $platform_tools =
{
	"windows"		=> "platform-tools_r09-windows.zip",
	"linux"			=> "platform-tools_r09-linux.zip",
	"macosx"		=> "platform-tools_r09-macosx.zip",
};

our $ndks =
{
	"r5"		=>
					{
						"windows" => "android-ndk-r5-windows.zip",
						"macosx" => "android-ndk-r5-darwin-x86.tar.bz2",
						"linux" => "android-ndk-r5-linux-x86.tar.bz2",
					},
	"r5b"		=>
					{
						"windows" => "android-ndk-r5b-windows.zip",
						"macosx" => "android-ndk-r5b-darwin-x86.tar.bz2",
						"linux" => "android-ndk-r5b-linux-x86.tar.bz2",
					},
	"r5c"		=>
					{
						"windows" => "android-ndk-r5c-windows.zip",
						"macosx" => "android-ndk-r5c-darwin-x86.tar.bz2",
						"linux" => "android-ndk-r5c-linux-x86.tar.bz2",
					},
	"r6"		=>
					{
						"windows" => "android-ndk-r6-windows.zip",
						"macosx" => "android-ndk-r6-darwin-x86.tar.bz2",
						"linux" => "android-ndk-r6-linux-x86.tar.bz2",
					},
	"r6b"		=>
					{
						"windows" => "android-ndk-r6b-windows.zip",
						"macosx" => "android-ndk-r6b-darwin-x86.tar.bz2",
						"linux" => "android-ndk-r6b-linux-x86.tar.bz2",
					},
	"r7"		=>
					{
						"windows" => "android-ndk-r7-windows.zip",
						"macosx" => "android-ndk-r7-darwin-x86.tar.bz2",
						"linux" => "android-ndk-r7-linux-x86.tar.bz2",
					},
};

our ($HOST_ENV, $TMP, $HOME, $WINZIP);

sub GetAndroidSDK
{
if(lc $^O eq 'darwin')
{
		$HOST_ENV = "macosx";
		$TMP = $ENV{"TMPDIR"};
		$HOME = $ENV{"HOME"};
}
elsif(lc $^O eq 'linux')
{
	$HOST_ENV = "linux";
	$TMP = "/tmp";
	$HOME = $ENV{"HOME"};
}
elsif(lc $^O eq 'mswin32')
{
	$HOST_ENV = "windows";
	$TMP = $ENV{"TMP"};
	$HOME = $ENV{"USERPROFILE"};
	if (-e "Tools/WinUtils/7z/7z.exe")
	{
		$WINZIP = "Tools/WinUtils/7z/7z.exe";
	}
}
elsif(lc $^O eq 'cygwin')
{
	$HOST_ENV = "windows";
	$TMP = $ENV{"TMP"};
	$HOME = $ENV{"HOME"};
}
else
{
	die "UNKNOWN " . $^O;
}

	print "Environment:\n";
	print "\tHost      = $HOST_ENV\n";
	print "\tTemporary = $TMP\n";
	print "\tHome      = $HOME\n";
	print "\n";
	print "\t\$$SDK_ROOT_ENV = $ENV{$SDK_ROOT_ENV}\n" if ($ENV{$SDK_ROOT_ENV});
	print "\t\$$NDK_ROOT_ENV = $ENV{$NDK_ROOT_ENV}\n" if ($ENV{$NDK_ROOT_ENV});
	print "\n";

my ($sdk, $ndk, $setenv) = @_;

#	Getopt::Long::GetOptions("sdk=s"=>\$sdk, "ndk=s"=>\$ndk) or die ("Illegal cmdline options");


if ($sdk)
{
	print "Installing SDK '$sdk':\n";
	if (!$ENV{$SDK_ROOT_ENV})
	{
		$ENV{$SDK_ROOT_ENV} = catfile($HOME, "android-sdk_auto");
		print "\t\$$SDK_ROOT_ENV not set; using $ENV{$SDK_ROOT_ENV} instead\n";
	}

	PrepareSDKTools();
	PrepareSDK($sdk);
	print "\n";
}

if ($ndk)
{
	print "Installing NDK '$ndk':\n";
	if (!$ENV{$NDK_ROOT_ENV})
	{
		$ENV{$NDK_ROOT_ENV} = catfile($HOME, "android-ndk_auto-" . $ndk);
		print "\t\$$NDK_ROOT_ENV not set; using $ENV{$NDK_ROOT_ENV} instead\n";
	}
	PrepareNDK($ndk);
	print "\n";
}

	my $export = "export";
	if (lc $^O eq 'mswin32')
	{
		$export = "set";
	}

	if ($setenv and ($ENV{$SDK_ROOT_ENV} or $ENV{$SDK_ROOT_ENV}))
	{
		print "Outputing updated environment:\n";
		print "\t'$setenv'\n";
		open (SETENV, '>' . $setenv);
		print SETENV "$export $SDK_ROOT_ENV=$ENV{$SDK_ROOT_ENV}\n" if ($ENV{$SDK_ROOT_ENV});
		print SETENV "$export $NDK_ROOT_ENV=$ENV{$NDK_ROOT_ENV}\n" if ($ENV{$NDK_ROOT_ENV});
		close (SETENV);
		print "\n";
	}

	print "Environment:\n" if ($ENV{$SDK_ROOT_ENV} or $ENV{$SDK_ROOT_ENV});
	print "\t\$$SDK_ROOT_ENV = $ENV{$SDK_ROOT_ENV}\n" if ($ENV{$SDK_ROOT_ENV});
	print "\t\$$NDK_ROOT_ENV = $ENV{$NDK_ROOT_ENV}\n" if ($ENV{$NDK_ROOT_ENV});
	print "\n";
}

sub PrepareSDKTools
{
	my $sdk_root = $ENV{$SDK_ROOT_ENV};
	my $sdk_tool_path = catfile($sdk_root, "tools");
	my $platform_tool_path = catfile($sdk_root, "platform-tools");
	if (-e catfile($sdk_tool_path, "NOTICE.txt") and -e catfile($platform_tool_path, "NOTICE.txt") )
	{
		print "\tSDK tools are already installed.\n";
		return;
	}
	my $sdk_tool = $sdk_tools->{$HOST_ENV};
	my $platform_tool = $platform_tools->{$HOST_ENV};
	die ("Unknown host environment '$HOST_ENV'") if (!$sdk_tool or !$platform_tool);

	print "\tDownloading '$sdk_tool' to '$sdk_tool_path'\n";
	DownloadAndUnpackArchive($BASE_URL_SDK . $sdk_tool, $sdk_tool_path);

	print "\tDownloading '$platform_tool' to '$platform_tool_path'\n";
	DownloadAndUnpackArchive($BASE_URL_SDK . $platform_tool, $platform_tool_path);
}

sub PrepareSDK
{
	my $sdk_root = $ENV{$SDK_ROOT_ENV};

	my ($sdk) = @_;

	if (IsPlatformInstalled($sdk))
	{
		print "\tPlatform '$sdk' is already installed\n";
		return;
	}

	my $platform = $sdks->{$sdk};
	die ("Unknown platform API '$sdk'") if (!$platform);

	my $output = catfile($sdk_root, "platforms", $sdk);
	print "\tDownloading '$platform' to '$output'\n";
	DownloadAndUnpackArchive($BASE_URL_SDK . $platform, $output);
}

sub IsPlatformInstalled
{
	my $sdk_root = $ENV{$SDK_ROOT_ENV};
	my ($sdk) = @_;
	if (! $sdk_root)
	{
		return 0;
	}
	unless (grep {$_ eq $sdk} GetCurrentSDKPlatforms($sdk_root))
	{
		return 0;
	}
	return 1;
}

sub GetCurrentSDKPlatforms
{
	my ($sdk_root) = @_;
	my $platform_root = $sdk_root . "/platforms";
	opendir(my $dh, $platform_root) || return;
	my @platforms = grep { !/^\.\.?$/ && -e catfile($platform_root, $_, "android.jar") } readdir($dh);
	closedir $dh;

	return @platforms;
}

sub DownloadAndUnpackArchive
{
	my ($url, $output) = @_;
	my ($base,$base_url,$suffix) = fileparse($url, qr/\.[^.]*/);
	my ($dest_name,$dest_path) = fileparse($output);

	my $temporary_download_path = catfile($TMP, $base . $suffix);
	my $temporary_unpack_path = catfile($TMP, $base . "_unpack");

	print "\t\tURL: " . $url . "\n";
	print "\t\tOutput: " . $output . "\n";
	print "\t\tBase: " . $base . "\n";
	print "\t\tURL base: " . $base_url . "\n";
	print "\t\tSuffix: " . $suffix . "\n";
	print "\t\tTmp DL: " . $temporary_download_path . "\n";
	print "\t\tTmp unpack: " . $temporary_unpack_path . "\n";
	print "\t\tDest path: " . $dest_path . "\n";
	print "\t\tDest name: " . $dest_name . "\n";

	# remove old output
	remove_tree($output);
	make_path($dest_path);

	# create temporary locations
	unlink($temporary_download_path);
	remove_tree($temporary_unpack_path);
	make_path($temporary_unpack_path);

	system("lwp-download", $url, $temporary_download_path);

	if ($WINZIP)
	{
		system($WINZIP, "x", $temporary_download_path, "-o" . $temporary_unpack_path);
	}
	else
	{
		if (lc $suffix eq '.zip')
		{
			system("unzip", "-q", $temporary_download_path, "-d", $temporary_unpack_path);
		}
		elsif (lc $suffix eq '.bz2')
		{
			system("tar", "-xf", $temporary_download_path, "-C", $temporary_unpack_path);
		}
		else
		{
			die "Unknown file extension '" . $suffix . "'\n";
		}
	}

	opendir(my $dh, $temporary_unpack_path);
	my @dirs = grep { !/^\.\.?$/ && -d catfile($temporary_unpack_path, $_) } readdir($dh);
	closedir $dh;
	my $unpacked_subdir = catfile($temporary_unpack_path, $dirs[0]);

	move($unpacked_subdir, $output);

	# clean up
	unlink($temporary_download_path);
	remove_tree($temporary_unpack_path);
}


sub PrepareNDK
{
	my ($ndk) = @_;
	my $ndk_root = $ENV{$NDK_ROOT_ENV};
	$ndk_root = $1 if($ndk_root=~/(.*)\/$/);

	if (-e $ndk_root and open RELEASE, "<", catfile("$ndk_root", "RELEASE.TXT"))
	{
		my @content = <RELEASE>;
		close RELEASE;
		chomp(@content);
		my $current = $content[0];
		print "\tCurrently installed = " . $current . "\n";

		if ($ndk eq $current)
		{
			print "\tNDK '$ndk' is already installed\n";
			return;
		}
		else
		{
			my ($current_name,$path) = fileparse($ndk_root);

			$ENV{$NDK_ROOT_ENV} = catfile($path, "android-ndk-" . $ndk);
			print "\t\$$NDK_ROOT_ENV is pointing to a mismatching NDK; using $ENV{$NDK_ROOT_ENV} instead\n";
			PrepareNDK($ndk);
			return;
		}
	}

	remove_tree($ndk_root);

	my $archive = $ndks->{$ndk}->{$HOST_ENV};
	die ("Unknown NDK release '$ndk' (for $HOST_ENV)") if (!$archive);

	print "\tDownloading '$ndk' to '$ndk_root'\n";
	DownloadAndUnpackArchive($BASE_URL_NDK . $archive, $ndk_root);
}

1;
