// Compiler options: -r:test-tuple-04-lib.dll

class Test
{
	public static int Main ()
	{
		var x = X.Test1 ();
		if (x.b != true)
			return 1;

		var z = X.Field;
		if (z.z != false)
			return 2;

		return 0;
	}
}