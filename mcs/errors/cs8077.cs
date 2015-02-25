// CS8077: A single-line comment may not be used in an interpolated string
// Line: 8

public class Test
{
	public static int Main ()
	{
		var s = $"test { arg // comment }";
	}
}