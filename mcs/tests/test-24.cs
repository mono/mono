//
// Properties intermixed in assignments
//

using System;

class X {

	static string v;

	static string S {
		get {
			return v;
		}
		set {
			v = value;
		}
	}

	static string x, b;
	
	public static int Main ()
	{

		x = S = b = "hlo";
		if (x != "hlo")
			return 1;
		if (S != "hlo")
			return 2;
		if (b != "hlo")
			return 3;
		return 0;
	}
}
		
