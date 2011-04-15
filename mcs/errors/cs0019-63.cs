// CS0019: Operator `==' cannot be applied to operands of type `U' and `null'
// Line: 13

abstract class A<T>
{
	public abstract bool Foo<U> (U arg) where U : T;
}

class B : A<byte>
{
	public override bool Foo<U> (U arg)
	{
		return arg == null;
	}
}
