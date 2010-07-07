// Compiler options: -warnaserror

// No CS1720 warning

using System;

class C
{
	static T M<T> () where T : struct
	{
		return ((Nullable<T>)null).GetValueOrDefault ();
	}
	
	public static int Main ()
	{
		if (M<int> () != 0)
			return 1;
		
		return 0;
	}
}
