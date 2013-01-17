// Compiler options: -langversion:future
using System;
using System.Threading.Tasks;
using System.Threading;

class Tester
{
	async Task<int> Lambda_1 ()
	{
		int res = 1;
		{
			int a = 8;
			Func<int> f = () => a;
			res = await Task.Factory.StartNew (f).ConfigureAwait (false);
			res += f ();
		}
		
		return res - 16;
	}
	
	async Task<int> Lambda_2 ()
	{
		int res = 1;
		{
			int a = 8;
			Func<int> f = () => a + res;
			res = await Task.Factory.StartNew (f).ConfigureAwait (false);
			res += f ();
		}
		
		return res - 26;
	}
	
	async Task<int> Lambda_3<T> ()
	{
		int res = 1;
		{
			int a = 8;
			Func<int> f = () => a;
			res = await Task.Factory.StartNew (f).ConfigureAwait (false);
			res += f ();
		}
		
		return res - 16;
	}	

	public static int Main ()
	{
		var t = new Tester ().Lambda_1 ();
		if (!Task.WaitAll (new [] { t }, 1000))
			return 1;
		
		if (t.Result != 0)
			return 2;
		
		t = new Tester ().Lambda_2 ();
		if (!Task.WaitAll (new [] { t }, 1000))
			return 3;
		
		if (t.Result != 0)
			return 4;

		t = new Tester ().Lambda_3<ulong>();
		if (!Task.WaitAll (new [] { t }, 1000))
			return 5;
		
		if (t.Result != 0)
			return 6;
		
		Console.WriteLine ("ok");
		return 0;
	}
}
