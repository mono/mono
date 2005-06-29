// cs0619-5.cs: `A' is obsolete: `Do not use it'
// Line: 12

using System;

[Obsolete("Do not use it", true)]
class A {
}

class B {
    static A _a = new A ();
}