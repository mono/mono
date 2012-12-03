
// Tests anonymous types initialized with a mix of literals, local variables and object members
using System;
using System.Collections;

public class MyClass
{
	public string Foo = "Bar";
	public int Baz {
		get { return 16; }
	}
}

public class Test
{
	public static int Main ()
	{
		string Hello = "World";
		MyClass mc = new MyClass();
		var v = new { mc.Foo, mc.Baz, Hello, Answer = 42 };
		
		if (v.Foo != "Bar")
			return 1;
		if (v.Baz != 16)
			return 2;
		if (v.Hello != "World")
			return 3;
		if (v.Answer != 42)
			return 4;
		
		return 0;
	}
}
