// CS0455: Type parameter `Y' inherits conflicting constraints `class' and `long'
// Line: 11

abstract class A<T>
{
	public abstract void Foo<U> () where U : class, T;
}

class B : A<long>
{
	public override void Foo<Y> ()
	{
	}
}
