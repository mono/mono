// CS0619: `A._value' is obsolete: `Do not use it'
// Line: 9

class A {
    [System.Obsolete("Do not use it", true)]
    int _value;
    
    public A () {
        _value = 4;
    }
}