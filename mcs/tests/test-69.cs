using System;
using System.Runtime.CompilerServices;

public class Blah {

	[MethodImpl (MethodImplOptions.InternalCall)]
	private extern void Start_internal(IntPtr handle);

	public static int Main ()
	{
		return 0;
	}
}
