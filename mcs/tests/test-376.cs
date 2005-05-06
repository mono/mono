using System;

class C {
	internal enum Flags {
		Removed	= 0,
		Public	= 1
	}

	static Flags	_enumFlags;
		
	public static int Main()
	{	
		bool xx = Flags.Public != 0;
		bool xx2 = 0 < Flags.Public;
	    
		if ((_enumFlags & Flags.Removed) != 0)
			return 3;

		if ((Flags.Public & 0).ToString () != "Removed")
		    return 1;
		
		if ((0 & Flags.Public).ToString () != "Removed")
		    return 1;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
