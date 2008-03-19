using System;

class C
{
	//
	// All the operations should be reduced
	//
	public void ZeroBasedReductions ()
	{
		int a = 1;
		
		a = a + 0;
		a = a - 0;
		
		a = a >> 0x40;
	}
	
	public static void Main ()
	{
	}
}


