//
// base and properties test
//
using System;

class X {
	int val;
	
	public virtual int prop {
		get {
			return val;
		}

		set {
			val = value;
		}
	}

	public int AAA {
		set { } 
	}
}

class Y : X {
	int val2 = 1;
	
	public override int prop {
		get {
			return val2;
		}

		set {
			val2 = value;
		}
	}
	
	int A () {
		if (base.prop != 0)
			return 3;
		base.prop = 10;

		if (base.prop != 10)
			return 2;

		return 0;
	}


	public static int Main ()
	{
		Y y = new Y ();

		return y.A ();
	}
	
}
