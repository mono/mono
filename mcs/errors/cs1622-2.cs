// CS1622: Cannot return a value from iterators. Use the yield return statement to return a value, or yield break to end the iteration
// Line: 14

using System.Collections;

public class C
{
	internal static IEnumerable PrivateBinPath
	{
		get
		{
			string a = "a";
			if (a == null)
				return false;
			yield return a;
		}
	}
}
