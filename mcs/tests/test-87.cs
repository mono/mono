//
// Tests the lookup of names on nested classes.
//
// Tests nested interfaces
//
class Top {

	class X {

	}

	class Y : X {
	}

	interface A {
		int get_one ();
	}

	interface B : A {
		int get_two ();
	}

	class XA : A {
		public int get_one () { return 1; }
	}

	class XB : B {
		public int get_one () { return 1; }
		public int get_two () { return 2; }
	}
	
	static int Main ()
	{
		XA x = new XA ();

		if (x.get_one () != 1)
			return 1;

		XB b = new XB ();
		if (x.get_one () != 1)
			return 2;
		if (b.get_two () != 2)
			return 3;
		return 0;
	}
}
		
