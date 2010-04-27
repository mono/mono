// CS3005: Identifier `CLSClass.Method<T>(int)' differing only in case is not CLS-compliant
// Line: 12
// Compiler options: -warnaserror

[assembly:System.CLSCompliant (true)]

public class BaseClass {
        public int method;
}

public class CLSClass : BaseClass {
        public static void Method<T>(int arg) {}
}
