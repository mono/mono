using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class C : B
{
}

class B
{
}

class X
{
	public static void Main ()
	{
		var x = new X ();
		x.AlignTwoItems ().Wait ();
	}

	public async Task AlignTwoItems ()
	{
		var items = new [] {
			(C) await AddItemAt (20, 20),
			(C) await AddItemAt (40, 40)
		};
		await MoveItemBy (items, 1, 1);

		Console.WriteLine ((C) items [0]);
		Console.WriteLine ((C) items [1]);
	}

	Task MoveItemBy (object o, int a, int b)
	{
		return Task.FromResult (2);
	}

	async Task<B> AddItemAt (int a, int b)
	{
		return new C ();
	}
}