// CS0619-36: `Error.member' is obsolete: `Obsolete member'
// Line: 8
// Compiler options: -reference:CS0619-36-lib.dll

class A {
    public A () {
        Error e = new Error ();
        ++e.member;
    }
}