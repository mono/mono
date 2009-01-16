using System;

class A
{
	public A (int v)
	{
		Values = new int [] { v, 1 };
	}
	
	public int[] Values;
}

class C
{
	public delegate void D ();
	
	public static int Main ()
	{
		new C ().Test ();
		return 0;
	}
	
	void SelectCommand (int v)
	{
	}
	
	void Test ()
	{
		A[] conflicts = new A []{ new A (1), new A (2), new A (3) };
		D d = delegate {
				foreach (A conf in conflicts) {
					foreach (int cmd in conf.Values) {
						int localCmd = cmd;
						D d2 = delegate {
							SelectCommand (localCmd);
						};
					}
				}
			};
	}
}
