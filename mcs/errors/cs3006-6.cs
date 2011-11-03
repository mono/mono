// CS3006: Overloaded method `CLSClass.CLSClass(int[,])' differing only in ref or out, or in array rank, is not CLS-compliant
// Line: 12
// Compiler options: -warnaserror -warn:1

using System;
[assembly: CLSCompliant(true)]

public class CLSClass {
    public CLSClass(int[,,] b) {
    }

    public CLSClass(int[,] b) {
    }

}
