// CS8087: A `}' character may only be escaped by doubling `}}' in an interpolated string
// Line: 8

public class Test
{
	public static int Main ()
	{
		var x = $" \u007D ";
	}
}