// Compiler options: -r:dll-2.dll

using System;
using System.Runtime.InteropServices;

namespace Foo {
  public class Baz {
    [DllImport("foo.so", CallingConvention=Bar.CConv)]
    public static extern void doFoo();

	  public static void Main ()
	  { }
  }
}
