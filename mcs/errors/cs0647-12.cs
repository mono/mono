// CS0647: Error during emitting `System.Runtime.InteropServices.DllImportAttribute' attribute. The reason is `DllName cannot be empty or null'
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
