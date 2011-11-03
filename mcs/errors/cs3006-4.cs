// CS3006: Overloaded method `CLSInterface.Test(int[,])' differing only in ref or out, or in array rank, is not CLS-compliant
// Line: 10
// Compiler options: -warnaserror -warn:1

using System;
[assembly: CLSCompliant(true)]

public interface CLSInterface {
        void Test(int[,,] b);
        void Test(int[,] b);
}
