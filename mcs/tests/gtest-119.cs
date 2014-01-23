// Compiler options: -unsafe
using System;

public class Tests {

	unsafe public static void Main () {
		Console.WriteLine (typeof (void).Name);
		Console.WriteLine (typeof (void*).Name);
		Console.WriteLine (typeof (void**).Name);
	}
}
