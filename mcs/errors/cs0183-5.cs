// CS0183: The given expression is always of the provided (`U') type
// Line: 14
// Compiler options: -warnaserror

abstract class A<T>
{
	public abstract bool Foo<U> (U arg) where U : T;
}

class B : A<int>
{
	public override bool Foo<U> (U arg)
	{
		return arg is U;
	}
}
