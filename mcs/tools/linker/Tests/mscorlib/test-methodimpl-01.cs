using System;
using System.Runtime.CompilerServices;

public class C
{
	public static int Main ()
	{
		Sync ();
		return 0;
	}

	[MethodImplAttribute(MethodImplOptions.Synchronized)]
	static void Sync ()
	{
	}
}