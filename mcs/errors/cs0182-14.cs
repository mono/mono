// CS0182: An attribute argument must be a constant expression, typeof expression or array creation expression
// Line: 8

using System.Runtime.InteropServices;

class X {
	[DllImport ("1" + 9)]
	extern static void Blah ();
}
