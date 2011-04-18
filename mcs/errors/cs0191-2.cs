// CS0191: A readonly field `Foo.i' cannot be assigned to (except in a constructor or a variable initializer)
// Line: 10 

class Foo {
	readonly int i;
	Foo () { }
	Foo (int i)
	{
		Foo x = new Foo ();
		x.i = i;
	}
	static void Main () { Foo y = new Foo (0); }
}
