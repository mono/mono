using System;

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
	delegate void D ();
	
	public static int Main ()
	{
		MyClass mc = null;
		
		D d = delegate () {
			mc = new MyClass() { Foo = "Baz", Answer = 42 };
		};
		
		d ();
		
		if (mc.Foo != "Baz")
			return 1;
		
		if (mc.Answer != 42)
			return 2;
		
		return 0;
	}
}
