// cs0208-4.cs: Cannot declare a pointer to a managed type ('System.Object')
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
