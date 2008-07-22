// CS3006: Overloaded method `CLSClass.Test(ref int)' differing only in ref or out, or in array rank, is not CLS-compliant
// Line: 15
// Compiler options: -warnaserror -warn:1

using System;
[assembly: CLSCompliant(true)]

public class Base
{
   public void Test(int a) {
   }
}

public class CLSClass: Base {
   public void Test(ref int b) {
   }
}
