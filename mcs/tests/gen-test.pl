#!/usr/bin/perl -w

my $gmcs = "gmcs";
my $monodis = "monodis";
my $mono = "mono";

my @normal = qw[gen-1 gen-2 gen-3 gen-4 gen-5 gen-6 gen-7 gen-8 gen-9 gen-10 gen-11 gen-12
		gen-14 gen-15 gen-16 gen-18 gen-19 gen-20 gen-21 gen-22 gen-23 gen-24 gen-25
		gen-26 gen-27 gen-28 gen-29 gen-30 gen-32 gen-33 gen-34 gen-35 gen-36 gen-37
		gen-38 gen-39 gen-40 gen-41 gen-42 gen-43 gen-44 gen-45 gen-46 gen-48 gen-49
		gen-50 gen-51 gen-52 gen-53 gen-54 gen-55 gen-56 gen-58 gen-59 gen-60 gen-62
		gen-63 gen-64];
my @compileonly = qw[];
my @library = qw[gen-13 gen-17 gen-31 gen-47];

sub RunTest
{
    my ($quiet,@args) = @_;
    my $cmdline = join ' ', @args;

    $cmdline .= " > /dev/null" if $quiet;

    print "Running $cmdline\n";

    my $exitcode = system $cmdline;
    if ($exitcode != 0) {
	print "Command failed!\n";
	return 0;
    }

    return 1;
}

sub NormalTest
{
    my ($file) = @_;

    my $cs = qq[$file.cs];
    my $exe = qq[$file.exe];

    RunTest (0, $gmcs, $cs) or return 0;
    RunTest (1, $monodis, $exe) or return 0;
    RunTest (1, $mono, $exe) or return 0;

    return 1;
}

sub CompileOnlyTest
{
    my ($file) = @_;

    my $cs = qq[$file.cs];
    my $exe = qq[$file.exe];

    RunTest (0, $gmcs, $cs) or return 0;

    return 1;
}

sub LibraryTest
{
    my ($file) = @_;

    my $cs_dll = qq[$file-dll.cs];
    my $dll = qq[$file-dll.dll];
    my $cs_exe = qq[$file-exe.cs];
    my $exe = qq[$file-exe.exe];

    RunTest (0, $gmcs, "/target:library", $cs_dll) or return 0;
    RunTest (1, $monodis, $dll) or return 0;
    RunTest (0, $gmcs, "/r:$dll", $cs_exe) or return 0;
    RunTest (1, $monodis, $exe) or return 0;
    RunTest (0, $mono, $exe) or return 0;
}

my @verify;
push @verify, "cologne";
push @verify, 'bin/peverify.sh';

foreach my $file (@normal) {
    print "RUNNING TEST: $file\n";
    if (NormalTest ($file)) {
	print STDERR "TEST SUCCEEDED: $file\n";
	push @verify, qq[$file.exe];
    } else {
	print STDERR "TEST FAILED: $file\n";
    }
}

foreach my $file (@compileonly) {
    print "RUNNING COMPILATION ONLY TEST: $file\n";
    if (CompileOnlyTest ($file)) {
	print STDERR "TEST SUCCEEDED: $file\n";
	push @verify, qq[$file.exe];
    } else {
	print STDERR "TEST FAILED: $file\n";
    }
}

foreach my $file (@library) {
    print "RUNNING LIBRARY TEST: $file\n";
    if (LibraryTest ($file)) {
	print STDERR "TEST SUCCEEDED: $file\n";
	push @verify, qq[$file-dll.dll];
	push @verify, qq[$file-exe.exe];
    } else {
	print STDERR "TEST FAILED: $file\n";
    }
}

my $hostname = `hostname --fqdn`;
chop $hostname;

if ($hostname eq 'gondor.boston.ximian.com') {
    print STDERR "VERIFYING TESTS\n";
    RunTest (0, "ssh", @verify);
}
