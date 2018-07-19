using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class X
{
	public static void Main ()
	{
		var x = new X ();
		x.Test ().Wait ();
	}

	int a, b;

	async Task Test ()
	{
		(a, b) = await Waiting ();
	}

	Task<(int, int)> Waiting ()
	{
		return Task.FromResult ((1, 3));
	}
}