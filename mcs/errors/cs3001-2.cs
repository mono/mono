// cs3001.cs: Argument type 'ref ulong' is not CLS-compliant
// Line: 9

using System;
[assembly:CLSCompliant (true)]

public class CLSClass {
        public CLSClass (long a) {}
        public CLSClass (ref ulong a) {}
}