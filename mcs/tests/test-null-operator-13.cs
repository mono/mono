using System;

static class Crash
{
	static X GetFoo ()
	{
		return null;
	}

	static int Main ()
	{
		int res = (GetFoo ()?.ToLower ()).ToUpper ();
		if (res != 0)
			return 1;

		return 0;
	}
}

class X
{
	public Y ToLower ()
	{
		throw new ApplicationException ("should not be called");
	}
}

class Y
{
}

static class SS
{
	public static int ToUpper (this Y y)
	{
		if (y != null)
			return 1;

		return 0;
	}
}