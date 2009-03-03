// Compiler options: -unsafe

using System;

class C
{
	static unsafe void CopyTo (int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		string s = "test string-";
		fixed (char* dest = destination, src = s)
		{
			CharCopy (dest + destinationIndex, src + sourceIndex, count);
		}
	}
	
	static unsafe void CharCopy (char *dest, char *src, int count)
	{
		for (int i = 0; i < count; i++) {
			*dest++ = *src++;
		}
	}
	
	public static int Main ()
	{
		CopyTo (5, null, 0, 0);

		char[] output = new char [6];
		CopyTo (5, output, 0, 6);
		string s = new string (output);
		Console.WriteLine (s);
		if (s != "string")
			return 1;
		
		return 0;
	}
}