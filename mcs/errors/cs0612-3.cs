// CS0612: `A.Value' is obsolete
// Line: 13
// Compiler options: -warnaserror

class A {
    [System.Obsolete ("")]
    int Value {
        set {
        }
    }
    
    public A () {
        Value = 4;
    }
}