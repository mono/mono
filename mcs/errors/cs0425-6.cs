// CS0425: The constraints for type parameter `T' of method `CA.Foo<T>()' must match the constraints for type parameter `U' of interface method `IA.Foo<U>()'. Consider using an explicit interface implementation instead
// Line: 16

interface IA
{
	void Foo<U> ();
}

class CA
{
	public void Foo<T> () where T : class
	{
	}
}

class CB : CA, IA
{
	public static void Main ()
	{
	}
}
