// cs0647.cs: Error emitting 'DllImport' attribute because 'Argument cannot be null.
// Line: 8

using System.Runtime.InteropServices;
using System;

class X {
	[DllImport (null)]
	extern static void Blah ();

    static void Main (string [] args)
    {
    }

}
