// CS3005: Identifier `CLSClass.Method(int)' differing only in case is not CLS-compliant
// Line: 8
// Compiler options: -warnaserror

[assembly:System.CLSCompliant (true)]

public class BaseClass {
        public int method;
}

public class CLSClass : BaseClass {
        public static void Method(int arg) {}
}
