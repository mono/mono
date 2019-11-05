package Tools;

use warnings;
use strict;
use File::Basename qw(dirname basename );
use File::Spec;
use Cwd qw(realpath);
use Carp qw(croak carp);
use File::stat;

require Exporter;
our @ISA = qw(Exporter);
our @EXPORT_OK = qw(InstallNameTool GitClone);


sub InstallNameTool
{
  my ($target,$pathtoburnin) = @_;
  print "running otool before:\n";
  system("otool","-L",$target);
  print "running install_name_tool\n";
  system("install_name_tool -id $pathtoburnin $target") eq 0 or die("Failed running install_name_tool");
  print "running otool after:\n";
  system("otool","-L",$target);
}

sub GitClone
{
	my $repo = shift;
	my $localFolder = shift;
	my $branch = shift;
	$branch = defined($branch)?$branch:"master";

	if (-d $localFolder) {
		return;
	}
	print "running git clone --branch $branch $repo $localFolder\n";
	system("git clone --branch $branch $repo $localFolder") eq 0 or die("git clone $repo $localFolder failed!");
}

