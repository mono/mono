using System;
using System.Threading;
using System.Threading.Tasks;

class MainClass
{
	async static Task AsyncTest ()
	{
		var b = await Task.Factory.StartNew (() => 13);
	}

	public static void Main (string[] args)
	{
		for (int i = 0; i < 100; ++i)
			AsyncTest ().Wait ();
	}
}
