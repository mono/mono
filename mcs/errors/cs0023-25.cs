// CS0023: The `?' operator cannot be applied to operand of type `T'
// Line: 8

class C
{
	static void Foo<T> (T t) where T : struct
	{
		var r = t?.ToString ();
	}
}