using System;

class X
{
	const int Value = 1000;

	static void Main ()
	{ 
		unchecked { 
			Func<byte> b = () => (byte)X.Value;
		} 
	}
}