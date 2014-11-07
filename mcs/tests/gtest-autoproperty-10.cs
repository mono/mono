// Compiler options: -langversion:experimental
struct S
{
	public decimal P { get; } = -3;
}

class X
{
	public static int Main ()
	{
		var s = new S ();
		if (s.P != -3)
			return 1;

		return 0;
	}
}