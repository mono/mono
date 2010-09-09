using System;

class ClassMain
{
	delegate void D (int i);
	
	public static void Main ()
	{
		if (true) {
			const bool test = false;
		} else {
			test = false;
		}
		
		D d = delegate (int test) { };
	}
	
	static bool test { 
		set {
		}
	}
}

