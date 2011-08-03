// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;


class C
{
	static async Task<string> TestCompositionCall_1 ()
	{
		return await Task.Factory.StartNew (() => { Thread.Sleep (10); return "a"; }) +
			await Task.Factory.StartNew (() => "b") + "c";
	}

	static async Task<string> TestCompositionCall_2 ()
	{
		return "a" + 1.ToString () +
			await Task.Factory.StartNew (() => "b") + "c";
	}

	static async Task<int> TestCompositionCall_3 ()
	{
		return await M (await Task.Factory.StartNew (() => (byte) 4));
	}

	static async Task<int> TestCompositionPair_1 ()
	{
		return await Task.Factory.StartNew (() => 3) + 6;
	}

	static async Task<int> TestCompositionPair_2 ()
	{
		return await Task.Factory.StartNew (() => { Thread.Sleep (10); return 3; }) - 
			await Task.Factory.StartNew (() => 4);
	}

	static async Task<int> TestCompositionPair_3 ()
	{
		return -8 * 
			await Task.Factory.StartNew (() => 4);
	}

	static async Task<int> TestCompositionPair_4 ()
	{
		return await Task.Factory.StartNew (() => 3) + 
			await Task.Factory.StartNew (() => 4) +
			await Task.Factory.StartNew (() => 7);
	}
	
	static Task<byte> M (byte value)
	{
		return Task.Factory.StartNew (() => value);
	}
	
	public static int Main ()
	{
		var t1 = TestCompositionCall_1 ();
		if (!Task.WaitAll (new[] { t1 }, 1000))
			return 1;
		
		if (t1.Result != "abc")
			return 2;
		
		var t2 = TestCompositionCall_2 ();
		if (!Task.WaitAll (new[] { t2 }, 1000))
			return 3;
		
		if (t2.Result != "a1bc")
			return 4;
		
		var t3 = TestCompositionCall_3 ();
		if (!Task.WaitAll (new[] { t3 }, 1000))
			return 5;
		
		if (t3.Result != 4)
			return 6;

		var t5 = TestCompositionPair_1 ();
		if (!Task.WaitAll (new[] { t5 }, 1000))
			return 7;
		
		if (t5.Result != 9)
			return 8;
		
		var t6 = TestCompositionPair_2 ();
		if (!Task.WaitAll (new[] { t6 }, 1000))
			return 9;
		
		if (t6.Result != -1)
			return 10;

		var t7 = TestCompositionPair_3 ();
		if (!Task.WaitAll (new[] { t7 }, 1000))
			return 11;
		
		if (t7.Result != -32)
			return 12;

		var t8 = TestCompositionPair_4 ();
		if (!Task.WaitAll (new[] { t8 }, 1000))
			return 13;
		
		if (t8.Result != 14)
			return 14;
		
		Console.WriteLine ("ok");
		return 0;
	}
}
