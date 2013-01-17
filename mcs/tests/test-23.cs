//
// Tests properties
//
using System;

class X {
	static int v;

	static X ()
	{
		v = 10;
	}

	public static int Value {
		get {
			return v;
		}

		set {
			v = value;
		}
	}

	public static int Main ()
	{
		if (Value != 10)
			return 1;

		Value = 4;

		if (Value != 4)
			return 2;

		Y y = new Y ("hello");

		if (y.Value != "hello")
			return 3;

		y.Value = "goodbye";
		if (y.Value != "goodbye")
			return 4;

		Z z = new Z ();

		if (Z.IVal != 4)
			return 5;
		Z.IVal = 10;
		if (Z.IVal != 10)
			return 6;

		z.XVal = 23;
		if (z.XVal != 23)
			return 7;

		return 0;
	}
}
	
class Y {
	string init;
	
	public Y (string s)
	{
		init = s;
	}

	public string Value {
		get {
			return init;
		}

		set {
			init = value;
		}
	}
}

struct Z {
	static int val;
	int xval;
	
	static Z ()
	{
		val = 4;
	}

	static public int IVal {
		get {
			return val;
		}

		set {
			val= value;
		}
	}

	public int XVal {
		get {
			return xval;
		}

		set {
			xval = value;
		}
	}
}
