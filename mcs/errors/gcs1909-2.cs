// CS1909: The DefaultParameterValue attribute is not applicable on parameters of type `System.Type'
// Line: 7

using System.Runtime.InteropServices;

interface ITest {
	void f ([DefaultParameterValue (typeof (ITest))] System.Type x);
}
