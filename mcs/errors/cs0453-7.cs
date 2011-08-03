// CS0453: The type `string' must be a non-nullable value type in order to use it as type parameter `T' in the generic type or method `C<T>'
// Line: 10

public class C<T> where T : struct
{
}

class A
{
	public void Foo (C<string>[] args)
	{
	}
}

