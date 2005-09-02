// cs3005-1.cs: Identifier `CLSClass.Method(int)' differing only in case is not CLS-compliant
// Compiler options: -warnaserror
// Line: 8

[assembly:System.CLSCompliant (true)]

public class BaseClass {
        public int method;
}

public class CLSClass : BaseClass {
        public static void Method(int arg) {}
}
