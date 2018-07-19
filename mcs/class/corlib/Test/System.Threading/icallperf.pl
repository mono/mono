# Attempt to analyze the performance before and after collecting
# performance counters for ThreadPool.
#
# The added costs are an icall per queued item.
# The icall takes/returns nothing -- no marshaling -- and
# internally does an atomic increment. Theoretically the
# bulk of the cost is the increment, not the call/ret.
#
# Doing the work on the managed side would have about the same motivation
# to do the same atomic increment, but would save transition to/from native.
# We could do it thread local and occasionally sweep.
#
# optbase is optimized baseline
# opt is optimized local changes

sub run
{
	my $cmd = shift;
	open(my $pipe, "-|", $cmd) || die("unable to run $cmd");
	while (my $line = <$pipe>)
	{
		if ($line =~ / items in (\d+)/)
		{
			return $1;
		}
	}
}

sub runloop
{
	my $cmd = shift;
	my $count = shift;
	my @data;
	#run($cmd); # throw out first
	for (my $i = 0; $i < $count; ++$i)
	{
		push(@data, run($cmd));
	}
	#run($cmd); # and last
	# Later we throw out slowest/fastest.
	return \@data;
}

sub report
{
	my $name = shift;
	my $data = shift;

	@{$data} = sort { $a <=> $b } @{$data}; # sort numerically
	shift($data); # remove first and last
	pop($data);

	my $n = scalar(@{$data});

	print("data $name:$n:");
	my $sum = 0;
	my $a = 0;
	for my $i (@{$data})
	{
		print("$i ");
		$sum += $i;
	}
	my $avg = $sum / $n;
	print("avg:$avg ");
	for my $i (@{$data})
	{
		my $b = abs($i - $avg);
		$a += $b * $b;
	}
	my $dev = sqrt($a) / $n;
	print("stddev: $dev");
	print("\n");
}

for (@ARGV)
{
	if (/^-?help/i || /^-?h/i || /^-?\?/ || /^-?usage/)
	{
		print("usage: perl $0 private_command baseline_command iterations\n");
		print(" e.g. perl $0"
			  . " /inst/monoopt/bin/mono /dev2/monoopt/mcs/class/corlib/Test/System.Threading/icallperf.exe"
		      . " /inst/monooptbase/bin/mono /dev2/monoopt/mcs/class/corlib/Test/System.Threading/icallperf.exe"
		      . " 20\n");
		exit(0);
	}
}

my $optcmd = shift || "/inst/monoopt/bin/mono /dev2/monoopt/mcs/class/corlib/Test/System.Threading/icallperf.exe";
my $optbasecmd = shift || "/inst/monooptbase/bin/mono /dev2/monoopt/mcs/class/corlib/Test/System.Threading/icallperf.exe";
my $n = shift || 20;

my $optdata = runloop($optcmd, $n);
my $optbasedata = runloop($optbasecmd, $n);

report("opt", $optdata);
report("base", $optbasedata);
