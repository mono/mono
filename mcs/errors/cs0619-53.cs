// CS0619: `C.explicit operator byte(C)' is obsolete: `gg'
// Line: 17

using System;

class C
{
	[Obsolete ("gg", true)]
	public static explicit operator byte (C x)
	{
		return 1;
	}

	static void Main ()
	{
		C x = null;
		var y = (int) x;
	}
}