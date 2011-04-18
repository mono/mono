// CS0619: `I' is obsolete: `'
// Line: 13

using System;

[Obsolete("", true)]
interface I
{
}

class A
{
        int this [I index] {
                get {
                        return 15;
                }
        }    
}
