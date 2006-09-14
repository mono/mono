// CS0283: The type `T' cannot be declared const
// Line: 8

struct S
{
	public void Foo<T> () where T : struct
	{
		const T t = null;
	}
}