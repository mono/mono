// cs3014.cs: 'C.field' cannot be marked as CLS-compliant because the assembly is not marked as compliant
// Line: 8

using System;
[assembly:CLSCompliant (false)]

public class C {
        [CLSCompliant (true)]
        public long field;
}