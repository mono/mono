// cs3001.cs: Argument type 'ulong' is not CLS-compliant
// Line: 8

using System;
[assembly:CLSCompliant (true)]

public class CLSClass {
        public long this [ulong index] {
                get {
                        return 2;
                }
        }
}