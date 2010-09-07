// CS1024: Wrong preprocessor directive
// Line: 12

using System;

class C
{
	static void Main ()
	{
#if AA
		Console.WriteLine ("DEBUG mode");
# something not valid here
		Console.WriteLine ("NON-DEBUG mode");
#endif
	}
}
