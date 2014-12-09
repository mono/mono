// CS0021: Cannot apply indexing with [] to an expression of type `object'
// Line: 9

class C
{
	public static void Main ()
	{
		var d = new object {
			["a"] = 1
		};
	}
}