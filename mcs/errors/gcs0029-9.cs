// CS0029: Cannot implicitly convert type `T2' to `T1'
// Line: 8

class Test
{
	static void Foo<T1, T2> (T1 t1, T2 t2)
	{
		T1 a = default (T2);
	}
}
