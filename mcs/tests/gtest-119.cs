// Compiler options: -unsafe
using System;

public class Tests {

	public unsafe static void Main () {
		Console.WriteLine (typeof (void).Name);
		Console.WriteLine (typeof (void*).Name);
		Console.WriteLine (typeof (void**).Name);
	}
}
