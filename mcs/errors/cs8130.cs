// CS8130: Cannot infer the type of implicitly-typed deconstruction variable `yy'
// Line: 8

class X
{
	public static void Main ()
	{
		var (xx, yy) = (1, Main);
	}
}