// CS0425: The constraints for type parameter `T' of method `C.Foo<T>()' must match the constraints for type parameter `T' of interface method `I.Foo<T>()'. Consider using an explicit interface implementation instead
// Line: 11

interface I
{
	void Foo<T> ();
}

class C : I
{
	public void Foo<T> () where T : struct
	{
	}
}
