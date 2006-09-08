// CS0403: `T': parameter name is the same as method type parameter name
// Line: 8

struct S
{
	public void Foo<T> () where T : struct
	{
		const T t = null;
	}
}