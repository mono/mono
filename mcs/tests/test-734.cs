// Compiler options: -optimize -r:test-734-lib.dll

using System;
using System.Reflection;

class M : C
{
	public void Run ()
	{
		run = false;
		Console.WriteLine (run);
	}
	
	public static int Main ()
	{
		new M ().Run ();
		
		var body = typeof (M).GetMethod ("Run").GetMethodBody ();

		// Check for volatile. (0xFE1E)
		var array = body.GetILAsByteArray ();
		if (array[2] != 0xFE)
			return 1;

		if (array[3] != 0x13)
			return 1;
		
		return 0;
	}
}
