// cs0647.cs: Error emitting 'MarshalAs' attribute because 'SizeParamIndex field is not valid for the specified unmanaged type'
// Line: 10

using System;
using System.Runtime.InteropServices;

public class main {

    [DllImport("libname", EntryPoint = "scumbag")]
    static extern int scumbag(ref int X, [MarshalAs(UnmanagedType.ByValArray)] ref byte[] fb);
}
