// cs3005-23.cs: Identifier `CLSClass.get_A()' differing only in case is not CLS-compliant
// Compiler options: -warnaserror
// Line: 8

[assembly:System.CLSCompliant(true)]

public class CLSClass {
        public int get_A () { return 3; }
        public int a { get { return 2; } }
}
