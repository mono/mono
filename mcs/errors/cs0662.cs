// CS0662: Cannot specify only `Out' attribute on a ref parameter. Use both `In' and `Out' attributes or neither
// Line: 8

using System.Runtime.InteropServices;

class C
{
   void Test(int i1, [Out, Int] ref int i2) {}
}
