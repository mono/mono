using System;

class EnumWrapperCtor<T>
{
	public enum Test
	{
		Wrong,
		MyDefault
	}

	readonly Test myVal;

	public EnumWrapperCtor (Test value = Test.MyDefault)
	{
		myVal = value;
	}

	public Test getValue ()
	{
		return myVal;
	}
}

public class C
{
	public static int Main ()
	{
		var ew = new EnumWrapperCtor<int> ();
		if ((int) ew.getValue () != 1)
			return 1;

		return 0;
	}
}
