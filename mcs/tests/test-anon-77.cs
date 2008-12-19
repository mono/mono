using System;

delegate void D ();

public class y
{
	bool [] xs;
	public bool this [int n]
	{
		get {
			D d = delegate () { n = 1; };
			return true;
		}

		set { xs [n] = value; }
	}
	
	public static void Main ()
	{
	}
}
