// Compiler options: -t:library

using System;
using System.Runtime.InteropServices;

namespace Foo {
  public class Bar {
    public const CallingConvention CConv = CallingConvention.Cdecl;
  }
}
