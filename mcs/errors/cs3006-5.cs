// cs3006-5.cs: Overloaded method `CLSInterface.Test(bool)' differing only in ref or out, or in array rank, is not CLS-compliant
// Line: 9

using System;
[assembly: CLSCompliant(true)]

public interface CLSInterface {
        void Test(bool a);
        void Test(out bool b);
}
