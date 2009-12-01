using System;

struct S
{
	public int Foo;
}

class Program
{
	static void Foo2 (int a, ref int b)
	{
		Console.WriteLine (a);
		Console.WriteLine (b);
		
		if (a != 0 || b != 0)
			throw new ApplicationException ();

		b = 500;
	}

	public static int Main ()
	{
		var a = new S [] { new S (), new S (), new S () };
		int i = 1;
		Foo2 (b: ref a[i].Foo, a: a[++i].Foo++);

		Console.WriteLine (a[0].Foo);
		Console.WriteLine (a[1].Foo);
		Console.WriteLine (a[2].Foo);
		
		if (a [0].Foo != 0)
			return 1;
		
		if (a [1].Foo != 500)
			return 2;
		
		if (a [2].Foo != 1)
			return 3;
		
		return 0;
	}
}
