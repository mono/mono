using System;

public class C
{
	public static int Main ()
	{
		var res1 = new string (new char[] { 'a', 'b', 'c'});
		var res2 = new string (new char[] { 'a', 'b', 'c'}, 1, 1);
		var res3 = new string ('x', 5);

		return 0;
	}
}