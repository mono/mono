// cs3005.cs: Identifier 'II.compareto()' differing only in case is not CLS-compliant
// Line: 10

[assembly:System.CLSCompliant(true)]

public interface I {
}

public interface II: I, System.IComparable {
        bool compareto();
}
