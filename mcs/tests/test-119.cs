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
	static void Main ()
	{
		Value v = new Value ();

		Derived d = (Derived) v;
	}
}
