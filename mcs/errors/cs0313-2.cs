// CS0313: The type `S?' cannot be used as type parameter `T' in the generic type or method `C<I>.Foo<T>(T)'. The nullable type `S?' never satisfies interface constraint `I'
// Line: 20

struct S : I
{
}

interface I
{
}

class C<U>
{
	static void Foo<T> (T value) where T : U
	{
	}

	static void Bar (S? s)
	{
		C<I>.Foo (s);
	}
}
