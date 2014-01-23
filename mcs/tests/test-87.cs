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

	public class XA : A {
		public int get_one () { return 1; }
	}

	class XB : B {
		public int get_one () { return 1; }
		public int get_two () { return 2; }
	}
	
	public static int Main ()
	{
		XA x = new XA ();

		if (x.get_one () != 1)
			return 1;

		XB b = new XB ();
		if (x.get_one () != 1)
			return 2;
		if (b.get_two () != 2)
			return 3;

		XB [] xb = null;

		return 0;
	}
}

//
// The following tests that the compiler will actually properly
// find the types that are requested (they are nested types)
//
class Other {
	public void X ()
	{
		Top.XA xa = null;
		Top.XA [] xb = null;
	}
}	
		
