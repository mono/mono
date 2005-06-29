// cs0619-6.cs: `A._value' is obsolete: `Do not use it'
// Line: 9

class A {
    [System.Obsolete("Do not use it", true)]
    int _value;
    
    public A () {
        _value = 4;
    }
}