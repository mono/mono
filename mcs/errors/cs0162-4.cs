// CS0162: Unreachable code detected
// Line: 13
// Compiler options: -warnaserror -warn:2

using System;

class C {
	bool T () { return true; }

	void Close()
	{	
		if (T () && false)
			Console.WriteLine ("error");
	}
}

class XXXX { static void Main () {} }
