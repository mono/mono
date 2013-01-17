// CS3006: Overloaded method `CLSClass.Test(ref int)' differing only in ref or out, or in array rank, is not CLS-compliant
// Line: 12
// Compiler options: -warnaserror -warn:1

using System;
[assembly: CLSCompliant(true)]

public class CLSClass {
   public void Test(int a) {
   }

   public void Test(ref int b) {
   }
}
