//
using System;

delegate void D ();

class X {
	static D r;
	
	public static void Main ()
	{
		D d = T ();

		d ();
		r ();
		r ();
	}

	static D T ()
	{
		int var1 = 0;
		
		D d = delegate () {
			int var2 = 1;
			
			r = delegate {
				Console.WriteLine ("var1: {0} var2: {1}", var1, var2);
				var2 = var2 + 1;
			};

			var1 = var1 + 1;
		};

		return d;
	}
}
