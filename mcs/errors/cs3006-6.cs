// cs3006-1.cs: Overloaded method 'CLSClass.CLSClass(int[,,])' differing only in ref or out, or in array rank, is not CLS-compliant
// Line: 11

using System;
[assembly: CLSCompliant(true)]

public class CLSClass {
    public CLSClass(int[,,] b) {
    }

    public CLSClass(int[,] b) {
    }

}
