// cs3005.cs: Identifier 'X.Index' differing only in case is not CLS-compliant
// Line: 8

[assembly:System.CLSCompliant(true)]

public class X {
        public int index { get { return 0; } }
        public int Index { set {} }
        
}