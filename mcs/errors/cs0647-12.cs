// cs0647.cs: Error emitting 'DllImport' attribute because 'dllName
// Line: 8

using System.Runtime.InteropServices;
using System;

class X {
	[DllImport ("")]
	extern static void Blah ();

    static void Main (string [] args)
    {
    }

}
