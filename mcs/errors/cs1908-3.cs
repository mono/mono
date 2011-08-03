// CS1908: The type of the default value should match the type of the parameter
// Line: 

class Test<T> where T : class {
	internal void f ([System.Runtime.InteropServices.DefaultParameterValue (null)] T x)
	{
	}
}
