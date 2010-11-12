using System.Collections.Generic;

public class c
{
	public static IEnumerable<char> func ()
	{
		yield return '0';
		yield break;
		foreach (char c in "1") {
			yield return c;
		}
	}

	public static void Main ()
	{
		foreach (char a in func ()) {
		}
	}
}