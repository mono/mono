// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `object'
// Line: 11
// Compiler options: -unsafe

using System;
using System.Runtime.InteropServices;

class C
{
	[DllImport ("xml2")]
	unsafe static extern object* valuePop (IntPtr context);
	public static void Main ()
	{
	}
}
