//
// Explicitly implement all the interface methods pending with the same name.
//
using System;

interface A {
	void X ();
}

interface B {
	void X ();
}

class C : A, B {
	int var;
	
	public void X ()
	{
		var++;
	}

	public static int Main ()
	{
		C c = new C ();

		A a = c;
		B b = c;

		if (c.var != 0)
			return 1;
		
		a.X ();
		if (c.var != 1)
			return 2;
		b.X ();
		if (c.var != 2)
			return 3;
		c.X ();
		if (c.var != 3)
			return 4;

		Console.WriteLine ("Test passes");
		return 0;
	}
}
