using System;

struct A
{
	public readonly int i;

	public A (int i)
	{
		this.i = i;
	}
}

class X
{
	int i;

	public int Foo {
		get {
			return 2 * i;
		}

		set {
			i = value;
		}
	}

	public int this [int a] {
		get {
			return (int) Foo;
		}

		set {
			Foo = a;
		}
	}

	public string this [string a] {
		set {
			Console.WriteLine (a);
		}
	}

	public string Bar {
		set {
			Console.WriteLine (value);
		}
	}

	public A A {
		get {
			return new A (5);
		}

		set {
			Console.WriteLine (value);
		}
	}

	public X (int i)
	{
		this.i = i;
	}

	public static int Main ()
	{
		X x = new X (9);
		int a = x.Foo = 16;
		int b = x [8] = 32;
		x ["Test"] = "Hello";
		x.Bar = "World";
		x.A = new A (9);
		// Compilation-only test.
		return 0;
	}
}

