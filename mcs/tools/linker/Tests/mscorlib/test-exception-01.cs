using System;

class C
{
	public static int Main ()
	{
		try {
			Foo ();
		} catch (ApplicationException) {
			return 0;
		} catch {
			return 1;		
		}
		
		return 2;
	}

	static void Foo ()
	{
		throw new ApplicationException ();
	}
}