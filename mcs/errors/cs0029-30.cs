// CS0029: Cannot implicitly convert type `U' to `int'
// Line: 13

abstract class A<T>
{
	public abstract void Foo<U> (U arg) where U : T;
}

class B : A<int>
{
	public override void Foo<U> (U arg)
	{
		int i = arg;
	}
}
