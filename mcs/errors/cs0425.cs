// CS0425: The constraints for type parameter `V' of method `Foo<T>.Test<V>()' must match the constraints for type parameter `U' of interface method `IFoo<T>.Test<U>()'. Consider using an explicit interface implementation instead
// Line: 12
interface IFoo<T>
{
	void Test<U> ()
		where U : T;
}

class Foo<T> : IFoo<T>
{
	public void Test<V> ()
	{ }
}

class X
{
	static void Main ()
	{ }
}
