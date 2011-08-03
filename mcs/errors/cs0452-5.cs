// CS0452: The type `int' must be a reference type in order to use it as type parameter `T' in the generic type or method `C<T>'
// Line: 10

public class C<T> where T : class
{
}

class A
{
	public A (ref C<int> args)
	{
	}
}

