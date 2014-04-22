// CS1985: The `await' operator cannot be used in a catch clause
// Line: 18

using System;
using System.Threading.Tasks;

class X
{
	public static void Main ()
	{
	}

	static async Task Test ()
	{
		int x = 4;
		try {
			throw null;
		} catch (NullReferenceException) if (await Foo ()) {
			return;
		}
	}

	static Task<bool> Foo ()
	{
		throw new NotImplementedException ();
	}
}