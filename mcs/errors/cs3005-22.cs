// CS3005: Identifier `X.Index' differing only in case is not CLS-compliant
// Line: 8
// Compiler options: -warnaserror

[assembly:System.CLSCompliant(true)]

public class X {
        public int index { get { return 0; } }
        public int Index { set {} }
        
}
