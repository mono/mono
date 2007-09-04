using System;

public class X
{
	public delegate void TestDelegate ();

	static long sum_i, sum_j, sum_k;
	static ulong sum_p;

	public static int Test (int p)
	{
		TestDelegate d = null;
		for (int i = 1; i <= 5; i++) {
			for (int j = i; j <= 8; j++) {
				int k = i;
				TestDelegate temp = delegate {
					Console.WriteLine ("i = {0}, j = {1}, k = {2}, p = {3}",
							   i, j, k, p);
					sum_i += 1 << i;
					sum_j += 1 << j;
					sum_k += 1 << k;
					sum_p += (ulong) (1 << p);
					p += k;
				};
				temp ();
				d += temp;
			}
		}
		Console.WriteLine ("SUM i = {0}, j = {1}, k = {2}, p = {3}",
				   sum_i, sum_j, sum_k, sum_p);
		Console.WriteLine ();
		if (sum_i != 300)
			return 1;
		if (sum_j != 2498)
			return 2;
		if (sum_k != 300)
			return 3;
		if (sum_p != 1825434804)
			return 4;
		sum_i = sum_j = sum_k = 0;
		sum_p = 0;
		d();
		Console.WriteLine ("SUM i = {0}, j = {1}, k = {2}, p = {3}",
				   sum_i, sum_j, sum_k, sum_p);
		Console.WriteLine ();
		if (sum_i != 1920)
			return 5;
		if (sum_j != 15360)
			return 6;
		if (sum_k != 300)
			return 7;
		if (sum_p != 18446744073385831629)
			return 8;
		return 0;
	}

	public static int Main ()
	{
		int result = Test (5);
		if (result != 0)
			Console.WriteLine ("ERROR: {0}", result);
		else
			Console.WriteLine ("OK");
		return result;
	}
}
