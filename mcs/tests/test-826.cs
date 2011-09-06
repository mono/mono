using System;
using System.Reflection;

public interface I
{
	int Foo ();
}

public class A : I
{
	int I.Foo ()
	{
		Console.WriteLine ("a");
		return 1;
	}
}

public class AA : A, I
{
	public int Foo ()
	{
		Console.WriteLine ("aa");
		return 2;
	}
}

public class B : A
{
	public int Foo ()
	{
		Console.WriteLine ("b");
		return 3;
	}
}

public class Test
{
	public static int Main ()
	{
		I i = new AA ();
		if (i.Foo () != 2)
			return 1;
		
		i = new B ();
		if (i.Foo () != 1)
			return 2;
		
		var m = typeof (B).GetMethod ("Foo");
		Console.WriteLine (m.Attributes);
		if (m.Attributes != (MethodAttributes.Public | MethodAttributes.HideBySig))
			return 3;
		
		return 0;
	}
}