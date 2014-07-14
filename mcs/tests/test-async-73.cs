using System.Threading.Tasks;
using System;

class X
{
	static async Task<int> Foo ()
	{
		var v = Throws ();

		try {
			await v;   
		} catch (Exception e) {
			return 0;
		}

		return 1;
	}

	static async Task<int> Throws ()
	{
		throw new Exception ();
	}

	static int Main ()
	{
		if (Foo ().Result != 0)
			return 1;

		return 0;
	}
}