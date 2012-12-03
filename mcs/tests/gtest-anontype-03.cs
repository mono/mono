
// Tests anonymous types initialized with object members
using System;
using System.Collections;

public class MyClass
{
	public string Foo = "Bar";
	public int Baz {
		get { return 42; }
	}
}

public class Test
{
	public static int Main ()
	{
		MyClass mc = new MyClass();
		var v = new { mc.Foo, mc.Baz };
		
		if (v.Foo != "Bar")
			return 1;
		if (v.Baz != 42)
			return 2;
			
		return 0;
	}
}
