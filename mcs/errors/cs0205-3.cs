// CS0205: Cannot call an abstract base member `A.Foobar.get'
// Line: 18
// Compiler options: -r:CS0205-3-lib.dll

using System;

public class B: A1
{
	protected override int Foobar  {
		get {
			return base.Foobar;
		}
	}

	static void Main ()
	{
		B b = new B ();
		if (b.Foobar == 1) {
		}
	}
}

