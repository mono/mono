// CS1503: Argument `#1' cannot convert `out A' expression to type `out B'
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
