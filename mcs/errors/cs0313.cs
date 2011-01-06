// CS0313: The type `S?' cannot be used as type parameter `T' in the generic type or method `S.Foo<T>(T)'. The nullable type `S?' never satisfies interface constraint
// Line: 16

interface I
{
}

struct S : I
{
	static void Foo<T> (T t) where T : I
	{
	}

	static void Test (S? s)
	{
		Foo (s);
	}
}
