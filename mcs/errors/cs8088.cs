// CS8088: A interpolated expression format specifier may not contain trailing whitespace
// Line: 9

public class Test
{
	public static int Main ()
	{
		int v = 0;
		var s = $"{v:X }";
	}
}