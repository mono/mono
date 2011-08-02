// CS0021: Cannot apply indexing with [] to an expression of type `int'
// Line: 9 
using System;

class ErrorCS0021 {
	public static void Main () {
		int i = 0;
		Console.WriteLine ("Test for ERROR CS0021: You can't use the indexer operator with a type that doesn't support it");
		Console.WriteLine ("Get i[2]: {0}", i[2]);
	}
}

