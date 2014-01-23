
// Tests object initialization
using System;
using System.Collections;

public class MyClass
{
	public string Foo = "Bar";
	private int answer;
	public int Answer {
		get { return answer; }
		set { answer = value; }
	}
}

public class Test
{
	public static int Main ()
	{
		MyClass mc = new MyClass() { Foo = "Baz", Answer = 42 };
		if (mc.Foo != "Baz")
			return 1;
		if (mc.Answer != 42)
			return 2;
		
		return 0;
	}
}
