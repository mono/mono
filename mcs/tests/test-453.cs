using System;

class C {
	internal enum Flags {
		Removed	= 0,
		Public	= 1
	}

	static Flags	_enumFlags;
		
	public static void Main()
	{
		if ((Flags.Removed | 0).ToString () != "Removed")
			throw new ApplicationException ();
	}
}

