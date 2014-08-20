using System;

class X
{
	static int Foo ()
	{
		throw new ApplicationException ();
	}

	public static int Main ()
	{
		try {
			var b = Foo () is object;
			return 1;
		} catch (ApplicationException) {
		}

		try {
			var b = Foo () as object;
			return 2;
		} catch (ApplicationException) {
		}

		return 0;
	}
}