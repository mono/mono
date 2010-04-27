// CS1910: Argument of type `System.Type' is not applicable for the DefaultParameterValue attribute
// Line: 7

using System.Runtime.InteropServices;

class Test {
	void f ([DefaultParameterValue (typeof (object))] object x)
	{
	}
}
