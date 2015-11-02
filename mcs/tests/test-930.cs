using System;

class X
{
	public static int Main ()
	{
		try {
			Test1 ();
			return 1;
		} catch (ApplicationException) {
		}

		try {
			Test2 ();
			return 2;
		} catch (ApplicationException) {
		}

		try {
			Test3 ();
			return 3;
		} catch (ApplicationException) {
		}

		return 0;
	}

	static void Test1 ()
	{
		try
		{
		}
		finally
		{
			throw new ApplicationException ();
		}
	}

	static void Test2 ()
	{
		try
		{
		}
		catch
		{
		}
		finally
		{
			throw new ApplicationException ();
		}
	}

	static void Test3 ()
	{
		try
		{
			throw new ApplicationException ();
		}
		finally
		{
		}
	}
}