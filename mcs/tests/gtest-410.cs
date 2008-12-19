// Compiler options: -unsafe

using System;

// Unsafe default expression, verifier checks IL

class Program
{
	static void Main()
	{
		unsafe {
			int* a = (int*)null;
			ulong* o = default (ulong*);
		}
    }
}
