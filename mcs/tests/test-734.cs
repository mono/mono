// Compiler options: -r:test-734-lib.dll

using System;

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
		return 0;
	}
}
