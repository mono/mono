// CS3005: Identifier `II.compareto()' differing only in case is not CLS-compliant
// Line: 11
// Compiler options: -t:library -warnaserror


[assembly:System.CLSCompliant(true)]

public interface I {
}

public interface II: I, System.IComparable {
        bool compareto();
}
