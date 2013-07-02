class Value {
	public static explicit operator int (Value val)
	{
		return 1;
	}

	public static explicit operator MyObject (Value val)
	{
		return new MyObject (1);
	}	

	public static explicit operator uint (Value val)
	{
		return 1;
	}
}

class MyObject {
	public MyObject (int i) {}
}

class Derived : MyObject {
	public Derived (int i) : base (i) { }

	Derived Blah ()
	{
		Value val = new Value ();

		return (Derived) val;
	}
}

class Test {
	public static int Main ()
	{
		Value v = new Value ();

		v = null;

		try {
			// This will throw an exception.
			// This test is more of a compile test, we need a real
			// good test that does not require this lame catch.
			Derived d = (Derived) v;
		} catch {
		}

		return 0;
	}
}
