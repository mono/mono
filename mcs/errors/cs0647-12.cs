// cs0647-12.cs: Error during emitting `System.Runtime.InteropServices.DllImportAttribute' attribute. The reason is `Empty name is not legal
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
