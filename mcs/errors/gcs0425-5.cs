// CS0425: The constraints for type parameter `V' of method `Foo<T,X>.Test<V>()' must match the constraints for type parameter `U' of interface method `IFoo<T>.Test<U>()'. Consider using an explicit interface implementation instead
// Line: 11

interface IFoo<T>
{
	void Test<U> () where U : T;
}

class Foo<T, X> : IFoo<T>
{
	public void Test<V> () where V : X
	{
	}
}
