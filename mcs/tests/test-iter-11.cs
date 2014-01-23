using System;
using System.Collections;

class X {
	public event EventHandler Hook;

	public IEnumerator Pipeline ()
	{
		if (Hook == null)
			throw new Exception ("error");

		Hook (this, EventArgs.Empty);
		
		yield return 0;
	}

	static void M (object sender, EventArgs args)
	{
		Console.WriteLine ("Hook invoked");
	}
	
	public static void Main ()
	{
		X x = new X ();
		x.Hook += M;
		IEnumerator y = x.Pipeline ();
		y.MoveNext ();
	}
}

