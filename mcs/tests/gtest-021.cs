// Testing the default value expressions (14.5.13)

using System;

class Foo<T>
{
	T[] t;

	public Foo (int n)
	{
		t = new T [n];
		for (int i = 0; i < n; i++)
			t [i] = default (T);
	}

	public void Test ()
	{
		X.Print (t [0]);
	}
}

class Bar<T>
{
	public void Test ()
	{
		X.Print (default (X));
		X.Print (default (T));
		X.Print (default (S));
	}
}

struct S
{
	public readonly string Hello;

	S (string hello)
	{
		this.Hello = hello;
	}

	public override string ToString ()
	{
		return String.Format ("S({0})", Hello);
	}

}

class X
{
	public static void Print (object obj)
	{
		if (obj == null)
			Console.WriteLine ("NULL");
		else
			Console.WriteLine ("OBJECT: {0} {1}", obj, obj.GetType ());
	}

	public static void Main ()
	{
		Foo<string> a = new Foo<string> (4);
		a.Test ();

		Bar<int> b = new Bar<int> ();
		b.Test ();
		Bar<X> c = new Bar<X> ();
		c.Test ();
	}
}
