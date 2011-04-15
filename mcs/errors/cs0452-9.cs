// CS0452: The type `U' must be a reference type in order to use it as type parameter `UU' in the generic type or method `B.Test<UU>(UU)'
// Line: 13

abstract class A<T>
{
	public abstract void Foo<U> (U arg) where U : T;
}

class B : A<int>
{
	public override void Foo<U> (U arg)
	{
		Test (arg);
	}
	
	void Test<UU> (UU a) where UU : class
	{
	}
}
