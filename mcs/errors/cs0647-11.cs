// CS0647: Error during emitting `System.Runtime.InteropServices.MarshalAsAttribute' attribute. The reason is `Specified unmanaged type is only valid on fields'
// Line: 10

using System;
using System.Runtime.InteropServices;

public class main {

    [DllImport("libname", EntryPoint = "scumbag")]
    static extern int scumbag(ref int X, [MarshalAs(UnmanagedType.ByValArray)] ref byte[] fb);
}
