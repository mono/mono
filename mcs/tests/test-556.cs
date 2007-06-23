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

	static void Main ()
	{
	}	
}
