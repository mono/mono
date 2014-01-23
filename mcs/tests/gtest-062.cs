using System.Collections.Generic;

class X
{
	public IEnumerable<int> Test (int a, long b)
	{
		while (a < b) {
			a++;
			yield return a;
		}
        }

	public static int Main ()
	{
		X x = new X ();
		int sum = 0;
		foreach (int i in x.Test (3, 8L))
			sum += i;

		return sum == 30 ? 0 : 1;
	}
}
