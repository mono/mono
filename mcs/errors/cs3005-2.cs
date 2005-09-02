// cs3005-2.cs: Identifier `CLSClass.Index' differing only in case is not CLS-compliant
// Compiler options: -warnaserror
// Line: 14

[assembly:System.CLSCompliant(true)]

public class X {
        public int index { get { return 0; } }
}

public class Y: X {
}
    
public class CLSClass: Y {
        public long Index { get { return 3; } }
}
