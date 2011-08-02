// CS0022: Wrong number of indexes `2' inside [], expected `1'
// Line: 9
using System;

class ErrorCS0022 {
	static void Main () {
		int[] integer_array = {0, 1};
		Console.WriteLine ("Test for Error CS0022: The compiler should say wrong number of fields inside the indexer");
		Console.WriteLine ("Trying to access integer_array[2, 3] in a one-dimensional array: {0}", integer_array[2,3]);
	}
}
