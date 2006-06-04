// gcs1910.cs: Argument of type `System.Type' is not applicable for the DefaultValue attribute
// Line: 7

using System.Runtime.InteropServices;

class Test {
	void f ([DefaultParameterValue (typeof (object))] object x)
	{
	}
}
