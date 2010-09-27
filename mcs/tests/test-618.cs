using System;

class C
{
	//
	// All the operations should be reduced
	//
	public static void ZeroBasedReductions ()
	{
		int a = 1;
		
		a = a + 0;
		a = a - 0;
		a = a | 0;
		a = 0 + a;
		a = 0 - a;
		a = 0 | a;
		
		a = a >> 0x40;
	}
	
	public static void ZeroBasedReductionsWithConversion ()
	{
		byte b = 0;
		b |= 0;
		b += 0;
		b -= 0;
		b *= 1;
	}
	
	public static void Main ()
	{
		ZeroBasedReductions ();
		ZeroBasedReductionsWithConversion ();
	}
}


