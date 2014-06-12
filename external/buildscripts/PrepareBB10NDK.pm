#!/usr/bin/perl

package PrepareBB10NDK;

use strict;
use warnings;
use Getopt::Long;
use Carp qw(croak carp);
use File::Path qw(mkpath rmtree);
use File::Spec::Functions;
use File::Copy;
use File::Basename;

require Exporter;
our @ISA = qw(Exporter);
our @EXPORT_OK=qw(GetBB10NDK);

our $NDK_ROOT_ENV = "BB10_NDK_ROOT";

our $BASE_URL_NDK = "https://rhodecode.unity3d.com/unity-extra/BB10-SDK";
our $BASE_URL_NDK_MIRROR = "http://mercurial-mirror.hq.unity3d.com/unity-extra/BB10-SDK";

our $ndks =
{
	"r09"	=>
			{
				"windows" => "bb10-0-09-windows.zip",
				"macosx" => "bb10-0-09-macosx.zip",
				"linux" => "bb10-0-09-linux.tar.bz2",
			},
};

our ($HOST_ENV, $TMP, $HOME, $WINZIP);

sub GetBB10NDK
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
	print "\t\$$NDK_ROOT_ENV = $ENV{$NDK_ROOT_ENV}\n" if ($ENV{$NDK_ROOT_ENV});
	print "\n";

my ($ndk, $setenv) = @_;

if ($ndk)
{
	print "Installing NDK '$ndk':\n";
	if (!$ENV{$NDK_ROOT_ENV})
	{
		$ENV{$NDK_ROOT_ENV} = catfile($HOME, "bbndk-auto-" . $ndk);
		print "\t\$$NDK_ROOT_ENV not set; using $ENV{$NDK_ROOT_ENV} instead\n";
	}
	PrepareNDK($ndk);
	PrepareNDKEnv($ndk);
	print "\n";
}

	my $export = "export";
	if (lc $^O eq 'mswin32')
	{
		$export = "set";
	}

	if ($setenv and ($ENV{$NDK_ROOT_ENV}))
	{
		print "Outputing updated environment:\n";
		print "\t'$setenv'\n";
		open (SETENV, '>' . $setenv);
		print SETENV "$export $NDK_ROOT_ENV=$ENV{$NDK_ROOT_ENV}\n" if ($ENV{$NDK_ROOT_ENV});
		close (SETENV);
		print "\n";
	}

	print "Environment:\n" if ($ENV{$NDK_ROOT_ENV});
	print "\t\$$NDK_ROOT_ENV = $ENV{$NDK_ROOT_ENV}\n" if ($ENV{$NDK_ROOT_ENV});
	print "\n";
}

sub DownloadAndUnpackArchive
{
	my ($url, $output, $compressed_file, $ndk) = @_;
	my ($base,$base_url) = fileparse($url, qr/\.[^.]*/);
	my ($dest_name,$dest_path) = fileparse($output);
	#my ($branch,$notUsed2,$suffix) = fileparse($compressed_file, qr/\.[^.]*/);
	my ($branch,$notUsed2,$suffix) = fileparse($compressed_file,qr/\.\D.*/);
	my $temporary_download_path = catfile($TMP, $base);
	my $temporary_download_file = catfile($temporary_download_path, $compressed_file);
	my $temporary_unpack_path = catfile($TMP, $base . "_unpack");
	
	print "\t\tURL: " . $url . "\n";
	print "\t\tBranch: " . $branch . "\n";
	print "\t\tOutput: " . $output . "\n";
	print "\t\tBase: " . $base . "\n";
	print "\t\tURL base: " . $base_url . "\n";
	print "\t\tSuffix: " . $suffix . "\n";
	print "\t\tTmp DL: " . $temporary_download_path . "\n";
	print "\t\tTmp DL File: " . $temporary_download_file . "\n";
	print "\t\tTmp unpack: " . $temporary_unpack_path . "\n";
	print "\t\tDest path: " . $dest_path . "\n";
	print "\t\tDest name: " . $dest_name . "\n";
	print "\t\tFile: " . $compressed_file . "\n";
	
	# remove old output
	rmtree($output);
	mkpath($dest_path);

	# create temporary locations
	rmtree($temporary_download_path);
	rmtree($temporary_unpack_path);
	mkpath($temporary_download_path);
	mkpath($temporary_unpack_path);
	print "\t\t Cloning Mercurial Repository.\n";
	system ("hg clone -b $branch $url $temporary_download_path");

	if ($WINZIP)
	{
		system($WINZIP, "x", $temporary_download_file, "-o" . $temporary_unpack_path);
	}
	else
	{
		if (lc $suffix eq '.zip')
		{
			system("unzip", "-q", $temporary_download_file, "-d", $temporary_unpack_path);
		}
		elsif (lc $suffix eq '.tar.bz2')
		{
			system("tar", "-xf", $temporary_download_file, "-C", $temporary_unpack_path);
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
	rmtree($temporary_download_path);
	rmtree($temporary_unpack_path);

	# Write out file to tag this directory as a paticular NDK version.
	my $release_file = catfile($output,"RELEASE.TXT");
	system("echo $ndk > $release_file");
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

			$ENV{$NDK_ROOT_ENV} = catfile($path, "bbndk-" . $ndk);
			print "\t\$$NDK_ROOT_ENV is pointing to a mismatching NDK; using $ENV{$NDK_ROOT_ENV} instead\n";
			PrepareNDK($ndk);
			return;
		}
	}

	rmtree($ndk_root);

	my $archive = $ndks->{$ndk}->{$HOST_ENV};

	die ("Unknown NDK release '$ndk' (for $HOST_ENV)") if (!$archive);

	print "\tDownloading '$ndk' to '$ndk_root'\n";
	if ($ENV{UNITY_THISISABUILDMACHINE}) 	
	{
		DownloadAndUnpackArchive($BASE_URL_NDK_MIRROR, $ndk_root, $archive, $ndk);
	}
	else
	{
		DownloadAndUnpackArchive($BASE_URL_NDK, $ndk_root, $archive, $ndk);
	}	
}

sub PrepareNDKEnv
{
        my ($ndk) = @_;
        my $ndk_root = $ENV{$NDK_ROOT_ENV};

	if (-e $ndk_root and open CONFIG, "<", catfile("$ndk_root", "bbndk-env.sh"))
        {
		my $i = 0;
		my $tmp = catfile("$ndk_root", "bbndk-env.sh");
		my @variables = ("QNX_TARGET","QNX_HOST","MAKEFLAGS","DYLD_LIBRARY_PATH","LD_LIBRARY_PATH","PATH"); 
		open(CONFIG, ". $tmp; echo \$QNX_TARGET; echo \$QNX_HOST; echo \$MAKEFLAGS; echo \$DYLD_LIBRARY_PATH; echo \$LD_LIBRARY_PATH; echo \$PATH |");
		while(<CONFIG>) {
			chomp($_);
			$ENV{$variables[$i]} = $_; 
			print "\t$variables[$i] = $ENV{$variables[$i]}\n";
			$i++;
		}
                close CONFIG;
	}
}
1;
