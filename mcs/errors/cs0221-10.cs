// CS0221: Constant value `-200' cannot be converted to a `byte' (use `unchecked' syntax to override)
// Line: 6

enum AA : byte { a, b = 200 }

public class C
{
	public static void Main ()
	{
		const int b = AA.a - AA.b;
	}
}
