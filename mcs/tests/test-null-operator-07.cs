using System;

public static class C
{
	static int Main()
	{
		int[] a = null;
		var r = a?.EM ().EM ().EM () ?? "N";
		if (r != "N")
			return 1;

		a?.EM ().EM ();

		return 0;
	}

	static string EM (this object arg)
	{
		if (arg == null)
			throw new ApplicationException ();

		return "";
	}
}