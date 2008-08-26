// CS0119: Expression denotes a `type parameter', where a `variable', `value' or `type' was expected
// Line: 8

class C
{
	static void Foo<T> ()
	{
		T.ToString ();
	}
}
