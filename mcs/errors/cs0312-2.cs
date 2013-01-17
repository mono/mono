// CS0312: The type `E?' cannot be used as type parameter `T' in the generic type or method `C<E>.Foo<T>(T)'. The nullable type `E?' does not satisfy constraint `E'
// Line: 16

enum E
{
}

class C<U>
{
	static void Foo<T> (T value) where T : U
	{
	}

	static void Test (E? e)
	{
		C<E>.Foo (e);
	}
}
