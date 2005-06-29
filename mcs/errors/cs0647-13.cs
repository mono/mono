// cs0647-13.cs: Error during emitting `System.Runtime.InteropServices.DllImportAttribute' attribute. The reason is `Argument cannot be null
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
