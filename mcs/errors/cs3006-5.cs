// CS3006: Overloaded method `CLSInterface.Test(out bool)' differing only in ref or out, or in array rank, is not CLS-compliant
// Line: 10
// Compiler options: -warnaserror -warn:1

using System;
[assembly: CLSCompliant(true)]

public interface CLSInterface {
        void Test(bool a);
        void Test(out bool b);
}
