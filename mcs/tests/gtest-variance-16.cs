using System;

struct S
{
	public static implicit operator string (S s)
	{
		return "s";
	}
}

interface I<in T>
{
}

class C : I<string>
{
	static T Foo<T> (T a, I<T> b)
	{
		return a;
	}
	
	public static int Main ()
	{
		S s = new S ();
		I<string> i = new C ();
		if (Foo (s, i) != "s")
			return 1;

		return 0;
	}
}
