//
// This test is used to test the `base' implementation
//
using System;

class Base {
	public int b_int_field;
	string b_string_field;

	const  int b_const_three = 3;
	
	int    b_int_property {
		get {
			return b_int_field;
		}

		set {
			b_int_field = value;
		}
	}

	string b_get_id ()
	{
		return "Base";
	}

	public Base ()
	{
		b_int_field = 1;
		b_string_field = "string";
	}
}

class Derived : Base {
	int b_int_field;

	public Derived ()
	{
		b_int_field = 10;
	}
	
	void Test ()
	{
		Console.WriteLine ("     int field: " + b_int_field);
		Console.WriteLine ("base int field: " + base.b_int_field);
	}
}

class boot {
	static int Main ()
	{
		Derived d = new Derived ();
		return 0;
	}
}
