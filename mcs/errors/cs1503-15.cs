// CS1503: Argument `#2' cannot convert `IFoo<object>' expression to type `IFoo<int>'
// Line: 18

interface IFoo<in T>
{
}

class C
{
	public static void Foo<T> (IFoo<T> e1, IFoo<T> e2)
	{
	}
	
	public static void Main ()
	{
		IFoo<int> a = null;
		IFoo<object> b = null;
		Foo (a, b);
	}
}
