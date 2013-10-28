// CS1503: Argument `#2' cannot convert `IContravariant<object>' expression to type `ICovariant<string>'
// Line: 23

interface IContravariant<in T>
{
}

interface ICovariant<out T>
{
}

class C
{
	public static void Test<T> (ICovariant<T> e1, ICovariant<T> e2)
	{
	}

	public static void Main ()
	{
		ICovariant<string> a_2 = null;
		IContravariant<object> b_2 = null;

		Test (a_2, b_2);
	}
}