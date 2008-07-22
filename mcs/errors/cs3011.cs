// CS3011: `CLSClass.Error(System.IComparable)': only CLS-compliant members can be abstract
// Line: 10
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant (true)]

public abstract class CLSClass {
        [CLSCompliant (false)]
        protected abstract void Error (IComparable arg);
}

