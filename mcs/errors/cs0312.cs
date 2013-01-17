// CS0312: The type `S?' cannot be used as type parameter `T' in the generic type or method `C<S>.Foo<T>(T)'. The nullable type `S?' does not satisfy constraint `S'
// Line: 16

struct S
{
}

class C<U>
{
	static void Foo<T> (T value) where T : U
	{
	}

	static void Test (S? s)
	{
		C<S>.Foo (s);
	}
}
