using System;

class C
{
	static void M<T> () where T : Exception, new ()
	{
		try {
			throw new T ();
		} catch (T ex) {
		}
	}

	public static int Main ()
	{
		M<ApplicationException> ();
		return 0;
	}
}