//
// This generates a warning
//
using System;

class A {
	public int a;

	public void X ()
	{
		a = 1;
	}
}

class B : A {
	void X ()
	{
		a = 2;
	}

	public void TestB ()
	{
		X ();
	}
}

class Ax {
	public int a;

	public virtual void A ()
	{
		a = 1;
	}

	public virtual void B ()
	{
		a = 3;
	}
}

class Bx : Ax {
	public override void A ()
	{
		a = 2;
	}
	public new void B ()
	{
		a = 4;
	}
}
class Test {
	public static int Main ()
	{
		B b = new B ();

		b.TestB ();
		if (b.a != 2)
			return 1;

		Bx bx = new Bx ();
		bx.A ();
		if (b.a != 2)
			return 2;
		bx.B ();
		Console.WriteLine ("a="+bx.a);
		if (bx.a != 4)
			return 3;
		return 0;
	}
}
