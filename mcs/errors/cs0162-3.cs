// CS0162: Unreachable code detected
// Line: 18
// Compiler options: -warnaserror -warn:2

using System;

class C {
	public enum Flags {
		Removed	= 0,
		Public	= 1
	}

	public Flags	_enumFlags;
		
	internal void Close()
	{	
		if ((Flags.Removed & _enumFlags) != (Flags.Removed & _enumFlags))
			Console.WriteLine ("error");
	}

	static void Main () {}
}
