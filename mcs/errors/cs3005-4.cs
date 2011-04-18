// CS3005: Identifier `CLSClass.constant' differing only in case is not CLS-compliant
// Line: 8
// Compiler options: -warnaserror

[assembly:System.CLSCompliant(true)]

public class CLSClass {
        protected int Constant = 1;
        protected const bool constant = false;
}
