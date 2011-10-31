// CS1061: Type `U2' does not contain a definition for `Test' and no extension method `Test' of type `U2' could be found (are you missing a using directive or an assembly reference?)
// Line: 20

interface I<T>
{
	void Foo<U> (U u) where U : T;
}

struct S
{
	public void Test ()
	{
	}
}

class Test : I<S>
{
	void I<S>.Foo<U2> (U2 u2)
	{
		u2.Test ();
	}
}
