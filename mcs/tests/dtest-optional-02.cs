using System.Runtime.InteropServices;

public class Test
{
	public static object Foo ([Optional, DefaultParameterValue (1)] dynamic i)
	{
		return i;
	}

	public static int Main ()
	{
		var res = (int) Foo ();
		if (res != 1)
			return 1;

		return 0;
	}
}