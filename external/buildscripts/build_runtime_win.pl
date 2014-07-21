sub CompileVCProj;
use File::Spec;
use File::Basename;
use File::Copy;
use File::Path;
my $root = File::Spec->rel2abs( dirname($0) . '/../..' );

if ($ENV{UNITY_THISISABUILDMACHINE})
{
	print "rmtree-ing $root/builds because we're on a buildserver, and want to make sure we don't include old artifacts\n";
	rmtree("$root/builds");
} else {
	print "not rmtree-ing $root/builds, as we're not on a buildmachine";
}


CompileVCProj("$root/msvc/mono.sln","Release_eglib|Win32",0);
my $remove = "$root/builds/embedruntimes/win32/libmono.bsc";
if (-e $remove)
{
	unlink($remove) or die("can't delete libmono.bsc");
}


#have a duplicate for now...
copy("$root/builds/embedruntimes/win32/mono.dll","$root/builds/monodistribution/bin/mono.dll");
copy("$root/builds/embedruntimes/win32/mono.pdb","$root/builds/monodistribution/bin/mono.pdb");

if ($ENV{UNITY_THISISABUILDMACHINE})
{
	system("git log --pretty=format:\"mono-runtime-win32 = %H %d %ad\" --no-abbrev-commit --date=short -1 > $root\\builds\\versions.txt");
}

sub CompileVCProj
{
	my $sln = shift(@_);
	my $slnconfig = shift(@_);
	my $incremental = shift(@_);
	my $projectname = shift(@_);
	my @optional = @_;
	
	
	my @devenvlocations = ($ENV{"PROGRAMFILES(X86)"}."/Microsoft Visual Studio 10.0/Common7/IDE/devenv.com",
		       "$ENV{PROGRAMFILES}/Microsoft Visual Studio 10.0/Common7/IDE/devenv.com",
		       "$ENV{REALVSPATH}/Common7/IDE/devenv.com");
	
	my $devenv;
	foreach my $devenvoption (@devenvlocations)
	{
		if (-e $devenvoption) {
			$devenv = $devenvoption;
		}
	}
	
	my $buildcmd = $incremental ? "/build" : "/rebuild";
	
        if (defined $projectname)
        {
            print "devenv.exe $sln $buildcmd $slnconfig /project $projectname @optional \n\n";
            system($devenv, $sln, $buildcmd, $slnconfig, '/project', $projectname, @optional) eq 0
                    or die("VisualStudio failed to build $sln");
        } else {
            print "devenv.exe $sln $buildcmd $slnconfig\n\n";
            system($devenv, $sln, $buildcmd, $slnconfig) eq 0
                    or die("VisualStudio failed to build $sln");
        }
}













