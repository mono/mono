using System;

public class X
{
	public delegate void TestDelegate ();

	static long sum_i, sum_k, sum_p;

	public static int Test (int p)
	{
		TestDelegate d = null;
		for (int i = 1; i <= 5; i++) {
			int k = i;
			TestDelegate temp = delegate {
				Console.WriteLine ("i = {0}, k = {1}, p = {2}", i, k, p);
				sum_i += 1 << i;
				sum_k += 1 << k;
				sum_p += 1 << p;
				p += k;
			};
			temp ();
			d += temp;
		}
		Console.WriteLine ("SUM i = {0}, k = {1}, p = {2}", sum_i, sum_k, sum_p);
		Console.WriteLine ();
		if (sum_i != 62)
			return 1;
		if (sum_k != 62)
			return 2;
		if (sum_p != 35168)
			return 3;
		sum_i = sum_k = sum_p = 0;
		d();
		Console.WriteLine ("SUM i = {0}, k = {1}, p = {2}", sum_i, sum_k, sum_p);
		Console.WriteLine ();
		if (sum_i != 320)
			return 4;
		if (sum_k != 62)
			return 5;
		if (sum_p != 1152385024)
			return 6;
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
