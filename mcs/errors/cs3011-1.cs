// cs3011-1.cs: 'CLSClass.this[long]': only CLS-compliant members can be abstract
// Line: 9

using System;
[assembly:CLSCompliant (true)]

public abstract class CLSClass {
        [CLSCompliant (false)]
        public abstract int this[long index] { set; }
}

