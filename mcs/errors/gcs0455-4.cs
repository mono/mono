// CS0455: Type parameter `Y' inherits conflicting constraints `long' and `long?'
// Line: 11

abstract class A<T1, T2>
{
	public abstract void Foo<U> () where U : T1, T2;
}

class B : A<long, long?>
{
	public override void Foo<Y> ()
	{
	}
}
