// cs3014-1.cs: 'C.Error(bool)' cannot be marked as CLS compliant because the assembly is not marked as compliant
// Line: 8

using System;
[assembly:CLSCompliant (false)]

public class C {
        [CLSCompliant (true)]
        protected void Error (bool arg) {}

}