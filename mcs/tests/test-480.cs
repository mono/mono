using System;

class A : IDisposable
{
	public A (int v)
	{
	}
	
	public void Dispose ()
	{
	}
}

class C
{
	int b;
	
	delegate void D (int i);
	
	static void Test (object arg)
	{
		const int a2= 9, a3 = a2, a4 = a3, a5 = a4;
		Console.WriteLine (a5);
		
		if (a2 > 0) {
			bool a = false;
		} else {
			const bool a = false;
		}

		for (int i = 0; i < 10; ++i) {
			Console.WriteLine (i);
		}
		
		for (int i = 0; i < 10; ++i) {
			Console.WriteLine (i);
		}
		
		foreach (var i in new int[] { 9, 8 }) {
			Console.WriteLine (i);
		}

		using (A i = new A (1), i2 = new A (2), i3 = new A (3)) {
		}
		
		using (A i = new A (3)) {
		}

		try {
		}
		catch (Exception o) {
			o = null;
		}

		D action = delegate (int i) {
		};
	}

	public static int Main ()
	{
		Test (1);
		return 0;
	}
}