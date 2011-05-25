// Compiler options: -r:test-823-lib.dll

class Test
{
	public static int Main ()
	{
		var a = new A ();
		if (a.Prop != 1)
			return 1;
		
		return 0;
	}
}
