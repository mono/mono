//
// Tests for bug # 52427 -- property inhertance stuff.
// these tests are problems that cropped up while
// making the patch. We dont want to regress on these.
//

class A { public virtual int Blah { get { return 1; } set {} } }

class B : A {
	public override int Blah { get { return 2; } }
	
	public static bool Test ()
	{
		// Make sure we see that set in A
		
		B b = new B ();
		
		if (b.Blah != 2) return false;
		if (b.Blah ++ != 2) return false;
		b.Blah = 0;
		
		return true;
	}
}

abstract class C { public abstract int Blah { get; set; } }
class D : C { public override int Blah { get { return 2; } set {} } }

class E : D {
	// Make sure we see that there is actually a base
	// which we can call
	public override int Blah { get { return base.Blah; } }
	
	public static bool Test ()
	{	
		E e = new E ();
		
		if (e.Blah != 2) return false;
		if (e.Blah ++ != 2) return false;
		e.Blah = 2;
		
		return true;
	}
}

interface IBlah {
	int this [int i] { get; set; }
	int Blah { get; set; }
}

class F : IBlah {
	int IBlah.this [int i] { get { return 1; } set {} }
	int IBlah.Blah { get { return 1; } set {} }
	
	public int this [int i] { get { return 2; } set {} }
	public int Blah { get { return 2; } set {} }
	
	public static bool Test ()
	{
		// Make sure we dont see a conflict between
		// the explicit impl and the non interface version
		F f = new F ();
		
		if (f.Blah != 2) return false;
		if (f.Blah ++ != 2) return false;
		f.Blah = 2;
		
		
		if (f [1] != 2) return false;
		if (f [1] ++ != 2) return false;
		f [1] = 2;
		
		return true;
	}
}

class Driver {
	public static int Main ()
	{
		if (! B.Test ()) return 1;
		if (! E.Test ()) return 2;
		if (! F.Test ()) return 3;
		
		return 0;
	}
}