using System;

public class Test<T>
{
	public delegate void TestDelegate ();

	public static void Hello<S> (T t, S s)
	{
		TestDelegate d = null;
		for (int i = 1; i <= 5; i++) {
			int k = i;
			TestDelegate temp = delegate {
				Console.WriteLine (k);
				Console.WriteLine (i);
				Console.WriteLine ();
			};
			temp ();
			d += temp;
		}
		d();
	}
}

class X
{
	static void Main ()
	{ }
}
