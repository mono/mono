using System;

class C
{
	static bool Throw ()
	{
		throw new ApplicationException ("error");
	}
	
	static bool Return (bool value)
	{
		return value;
	}
	
	public static int Main ()
	{
		dynamic d = 4;
		
		if (Return (false) && d)
			return 1;

		if (Return (true) || d) {
		} else {
			return 2;
		}

		d = false;
		if (d && Throw ())
			return 3;
		
		d = true;
		if (d || Throw ()) {
		} else {
			return 4;
		}
		
		Console.WriteLine ("ok");
		return 0;
	}
	
}