// cs3005.cs: Identifier 'CLSClass.a.get' differing only in case is not CLS-compliant
// Line: 8

[assembly:System.CLSCompliant(true)]

public class CLSClass {
        public int get_A () { return 3; }
        public int a { get { return 2; } }
}
