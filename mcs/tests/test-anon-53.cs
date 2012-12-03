using System;

public delegate void Foo ();

class Test
{
	public Test (int a)
	{
		Foo foo = delegate {
			Console.WriteLine (a);
		};
		foo ();
	}

	static Test ()
	{
		int a = 5;
		Foo foo = delegate {
			Console.WriteLine (a);
		};
		foo ();
	}
}

class X
{
	public static void Main ()
	{
		Test test = new Test (9);
		Console.WriteLine (test);
	}
}
