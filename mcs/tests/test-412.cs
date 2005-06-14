// Compiler options: -r:test-412-lib.dll

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
