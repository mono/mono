// CS0023: The `?' operator cannot be applied to operand of type `T'
// Line: 8

class X
{
	static void Bug<T>(System.Func<T> func)
	{
		var r = func?.Invoke ();
	}
}