using System;

public class Test
{
	delegate void D ();
	
	public static int Main ()
	{
		new Test ().Test_3<int> ();
		return 0;
	}
	
	public void Test_3<T> () where T : struct
	{
		D d = delegate () {
			T? tt = null;
		};
		d ();
	}
}
