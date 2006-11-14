using System;

public class Test
{
	public delegate void TestDelegate ();

	public static void Main ()
	{
		TestDelegate d = null;
		for (int i = 1; i <= 5; i++) {
			int k = i;
			TestDelegate temp = delegate {
				Console.WriteLine (i);
				Console.WriteLine (k);
				Console.WriteLine ();
			};
			temp ();
			d += temp;
		}
		d();
	}
}
