//
// fixed
//
using System;

class X {

	void A ()
	{
	}
				
	public static void Main ()
	{
		int loop = 0;
		
		goto a;
	b:
		loop++;
		return;
	a:
		Console.WriteLine ("Hello");
		for (;;){
			goto b;
		}
	}
}
