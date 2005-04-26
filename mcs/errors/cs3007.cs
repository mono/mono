// cs3007.cs: Overloaded method 'CLSClass.(bool[])' differing only by unnamed array types is not CLS-compliant
// Line: 11

using System;
[assembly: CLSCompliant(true)]

public class CLSClass {
    public CLSClass(int[,,][] i) {
    }

    public CLSClass(bool[] b) {
    }
}
