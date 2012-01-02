package DependencyVersion;

use File::Spec;
use Cwd;

sub GetCurrentVersion($)
{
	my $workingCopy = shift;
	my $vcs = GetVCS($workingCopy);
	my %vcsMethod = (
		'hg' => \&GetCurrentVersionMercurial,
		'git' => \&GetCurrentVersionGit,
		'svn' => \&GetCurrentVersionSubversion
	);

	
	return $vcsMethod{$vcs}->($workingCopy) if (exists ($vcsMethod{$vcs}));
	return '';
}

sub GetCurrentVersionGit($)
{
	my $workingCopy = shift;
	if (-d $workingCopy) {
		my $cwd = cwd();
		chdir($workingCopy);
		$revision = `git rev-parse HEAD`;
		chdir($cwd);
		return $revision;
	}
	return '';
}

sub GetCurrentVersionMercurial($)
{
	my $workingCopy = shift;
	return `hg --cwd "$workingCopy" identify --id` if (-d $workingCopy);
	return '';
}

sub GetCurrentVersionSubversion($)
{
	my $workingCopy = shift;
	return `svnversion "$workingCopy"` if (-d $workingCopy);
	return '';
}

sub GetVCS($)
{
	my $workingCopy = shift;
	my %VCSmap = (
		'.hg' => 'hg',
		'.git' => 'git',
		'.svn' => 'svn'
	);

	for my $dir (keys %VCSmap) {
		if (-d File::Spec->catdir($workingCopy, $dir)) {
			return $VCSmap{$dir};
		}
	}

	return '';
}

1;
