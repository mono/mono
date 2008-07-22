// CS3011: `CLSClass.this[long]': only CLS-compliant members can be abstract
// Line: 10
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant (true)]

public abstract class CLSClass {
        [CLSCompliant (false)]
        public abstract int this[long index] { set; }
}

