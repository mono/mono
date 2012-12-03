
// Tests variable type inference with the var keyword when assigning to user-defined types
using System;

public class Class1
{
	public bool Method()
	{
		return true;
	}
	public int Property = 16;
}

public class Test
{
	private class Class2
	{
		public bool Method()
		{
			return true;
		}
		public int Property = 42;
	}	
	public static int Main ()
	{
		var class1 = new Class1 ();
		
		if (class1.GetType () != typeof (Class1))
			return 1;
		if (!class1.Method ())
			return 2;
		if (class1.Property != 16)
			return 3;
		
		var class2 = new Class2();
		
		if (class2.GetType () != typeof (Class2))
			return 4;
		if (!class2.Method ())
			return 5;
		if (class2.Property != 42)
			return 6;
		
		return 0;
	}
}
