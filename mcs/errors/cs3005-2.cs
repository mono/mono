// CS3005: Identifier `CLSClass.Index.get' differing only in case is not CLS-compliant
// Line: 14
// Compiler options: -warnaserror

[assembly:System.CLSCompliant(true)]

public class X {
        public int index { get { return 0; } }
}

public class Y: X {
}
    
public class CLSClass: Y {
        public long Index { get { return 3; } }
}
