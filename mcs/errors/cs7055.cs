// CS7055: Unmanaged type `ByValArray' is only valid for fields
// Line: 10

using System;
using System.Runtime.InteropServices;

public class main {

    [DllImport("libname", EntryPoint = "scumbag")]
    static extern int scumbag(ref int X, [MarshalAs(UnmanagedType.ByValArray)] ref byte[] fb);
}
