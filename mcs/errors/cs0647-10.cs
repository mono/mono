// cs0647.cs: Error emitting 'MarshalAs' attribute because 'Specified unmanaged type is only valid on fields'
// Line: 10

using System;
using System.Runtime.InteropServices;

public class main {

    [DllImport("libname", EntryPoint = "scumbag")]
    static extern int scumbag(ref int X, [MarshalAs(UnmanagedType.ByValArray)] ref byte[] fb);
}
