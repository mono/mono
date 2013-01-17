using System;
using System.Collections;

public class S 
{
	public void Frobnikator() 
	{
		const UInt32 SMALL_MASK = (1U << (24)) - 1; 
		const ulong BIG_MASK = ~((ulong)SMALL_MASK);
		
	}
	
	public void CharToX ()
	{
		const char c = 'a';
		
		const ushort us = c;
		const int i = c;
		const uint ui = c;
		const long l = c;
		const ulong ul= c;
		const float fl = c;
		const double d = c;
		const decimal dec = c;
	}

	public static int Main ()
	{
		long i = 1;
		int i2 = 0xA0;
		long b = i << i2;
		
		if (b != 4294967296)
			return 1;
			
		return 0;
	}	
}
