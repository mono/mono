// Compiler options: -r:test-858-lib.dll

using System;

class X
{
	public static int Main ()
	{
		Bar bar = new Bar ();
		if (bar.Message != "bar")
			return 1;

		Foo foo = new Foo ();
		if (foo.Message != "foo")
			return 2;

		C c = new C ();
		c.get_Value ();
		c.add_Arg (false);
        
		Console.WriteLine ("Test ok");
		return 0;
	}
}
