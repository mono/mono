// CS0029: Cannot implicitly convert type `T[]' to `I[]'
// Line: 12

interface I
{
}

class C
{
	static void Foo<T> (T [] t) where T : I
	{
		I [] i = t;
	}
}
