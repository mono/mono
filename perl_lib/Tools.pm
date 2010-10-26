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
our @EXPORT_OK = qw(InstallNameTool);


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
