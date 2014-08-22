// CS0030: Cannot convert type `T2[]' to `T1[]'
// Line: 8

class X
{
	static void Foo<T1,T2> (T2[] array) where T1 : class where T2 : struct
	{
		T1[] a = (T1[])array;
	}
}
