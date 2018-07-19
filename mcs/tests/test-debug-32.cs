using System.Collections.Generic;

class X
{
	internal static IEnumerable<int> EnumerateKind ()
	{
		yield return 1;

		int h = 3;
		try {
			yield return h;
		} finally {
			if (h != 1) {
			}
		}
	}

	public static void Main ()
	{
	}
}