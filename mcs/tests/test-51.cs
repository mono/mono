//
// This test is used to test the `base' implementation
//
using System;

class Base {
	public int b_int_field;
	public string b_string_field;

	public const  int b_const_three = 3;
	
	public int    b_int_property {
		get {
			return b_int_field;
		}

		set {
			b_int_field = value;
		}
	}

	public string b_get_id ()
	{
		return "Base";
	}

	public Base ()
	{
		b_int_field = 1;
		b_string_field = "base";
	}
}

class Derived : Base {
	new int b_int_field;
	new string b_string_field;
	new const int b_const_three = 4;

	new int b_int_property {
			get {
				return b_int_field;
			}


			set {
				b_int_field = value;
			}

		}
	
	public Derived ()
	{
		b_int_field = 10;
		b_string_field = "derived";
	}
	
	public int Test ()
	{
		if (b_int_field != 10)
			return 1;
		if (base.b_int_field != 1)
			return 2;
		if (base.b_string_field != "base")
			return 3;
		if (b_string_field != "derived")
			return 4;
		base.b_int_property = 4;
		if (b_int_property != 10)
			return 5;
		if (b_int_property != 10)
			return 6;
		if (base.b_int_property != 4)
			return 7;
		if (b_const_three != 4)
			return 8;
		if (Base.b_const_three != 3)
			return 9;
		System.Console.WriteLine ("All tests pass");
		return 0;
	}
}

class boot {
	public static int Main ()
	{
		Derived d = new Derived ();
		return d.Test ();
	}
}
