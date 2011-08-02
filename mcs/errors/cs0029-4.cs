// CS0029: Cannot implicitly convert type `string' to `System.Runtime.InteropServices.CallingConvention'
// Line: 10

using System;
using System.Runtime.InteropServices;

namespace Tsunami {
  public sealed class Gl {

    [DllImport("libGL.so", EntryPoint="glCopyTexSubImage3D", CallingConvention="cdecl", ExactSpelling=true)]
    public static extern void CopyTexSubImage3D ();

  }
}

