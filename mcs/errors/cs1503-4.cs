// CS1503: Argument 1: Cannot convert type `A' to `B'
// Line: 17

class A { }
class B : A { }

class Test
{
	static void Foo (out B b)
	{
		b = new B ();
	}

	static void Main ()
	{
		A a;
		Foo (out a);
	}
}
