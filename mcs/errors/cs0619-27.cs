// CS0619: `I' is obsolete: `Do not use it'
// Line: 12

using System;

[Obsolete("Do not use it", true)]
interface I {
}

class B {
    I i;
}