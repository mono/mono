// CS1909: The DefaultParameterValue attribute is not applicable on parameters of type `int[]'
// Line: 7

using System.Runtime.InteropServices;

class Test {
	void f ([DefaultParameterValue (new int[0])] int[] x)
	{
	}
}
