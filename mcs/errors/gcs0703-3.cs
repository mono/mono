// CS0703: Inconsistent accessibility: constraint type `C.I' is less accessible than `C.Foo<T>()'
// Line: 10

public class C
{
	interface I
	{
	}

	public void Foo<T>()  where T : I
	{
	}
}
