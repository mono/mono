// CS0619: `D' is obsolete: `Do not use it'
// Line: 12

using System;

[Obsolete("Do not use it", true)]
delegate void D();

class B {
    event D e;
}