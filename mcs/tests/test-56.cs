//
// Stress test properties and the various modes of 
// declarations (virtual, overrides, abstract, new)
//
using System;

interface I {
	int P {
		get; set;
	}
}

abstract class A : I {
	public int p;
	public int q;
	
	public int P {
		get { return p; }
		set { p = value; }
	}

	public abstract int Q { get; set; }

	public int r;
	public virtual int R { get { return r; } set { r = value; } }
}

class B : A {
	public int bp;

	public new int P
	{
		get { return bp; }
		set { bp = value; }
	}

	public override int Q {
		get { return q; }
		set { q = value; }
	}
}

class C : A {
	public override int Q {
		get { return q; }
		set { q = value; }
	}

	public int rr;
	public override int R { get { return rr; } set { rr = value; } }
}

class M {

	public static int Main ()
	{
		B b = new B ();

		b.P = 1;
		b.R = 10;
		b.Q = 20;
				 
		if (b.P != 1)
			return 1;
		if (b.bp != 1)
			return 2;

		if (b.R != 10)
			return 3;
		if (b.r != 10)
			return 4;

		if (b.Q != 20)
			return 5;
		if (b.q != 20)
			return 6;

		C c = new C ();

		c.R = 10;
		c.Q = 20;
		c.P = 30;
		if (c.R != 10)
			return 7;
		if (c.rr != 10)
			return 8;
		if (c.P != 30)
			return 9;
		if (c.p != 30)
			return 10;

		Console.WriteLine ("Test passes");
		return 0;
	}
}
	
