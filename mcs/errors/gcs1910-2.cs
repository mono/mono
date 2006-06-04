// gcs1910.cs: Argument of type `int[]' is not applicable for the DefaultValue attribute
// Line: 7

using System.Runtime.InteropServices;
using System;

class Test {
	void f ([DefaultParameterValue (new int[0])] object x)
	{
	}
}
