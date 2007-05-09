using System;

public class Test
{
	public delegate void DelegateA (bool b);
	public delegate int DelegateB (int i);

	static DelegateA dt;
	static DelegateB dt2;

	public static int Main ()
	{
		bool b = DelegateMethod == dt;
		if (b)
			return 1;

		b = DelegateMethod != dt;
		if (!b)
			return 2;

		b = dt2 == DelegateMethod;
		if (b)
			return 3;

		Console.WriteLine ("OK");
		return 0;
	}

	static void DelegateMethod (bool b)
	{
	}

	static int DelegateMethod (int b)
	{
		return 4;
	}
}


