// cs3011.cs: 'CLSClass.Error(System.IComparable)': only CLS-compliant members can be abstract
// Line: 12

using System;
[assembly:CLSCompliant (true)]

public abstract class CLSClass {
        [CLSCompliant (false)]
        internal abstract int Valid ();
    
        [CLSCompliant (false)]
        protected abstract void Error (IComparable arg);
}

