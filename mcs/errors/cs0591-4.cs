// CS0591: Invalid value for argument to `System.Runtime.InteropServices.DllImportAttribute' attribute
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
