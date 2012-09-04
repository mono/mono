// CS0182: An attribute argument must be a constant expression, typeof expression or array creation expression
// Line: 10

using System.Runtime.InteropServices;

class X {
	static string dll = "some.dll";
	
	[DllImport (dll)]
	extern static void Blah ();
}
