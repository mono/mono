// cs3006.cs: Overloaded method 'CLSClass.Test(ref int)' differing only in ref or out, or in array rank, is not CLS-compliant
// Line: 11

using System;
[assembly: CLSCompliant(true)]

public class CLSClass {
   public void Test(int a) {
   }

   public void Test(ref int b) {
   }
}
