// CS3007: Overloaded method `CLSClass.CLSClass(bool[])' differing only by unnamed array types is not CLS-compliant
// Line: 12
// Compiler options: -warnaserror -warn:1

using System;
[assembly: CLSCompliant(true)]

public class CLSClass {
    public CLSClass(int[,,][] i) {
    }

    public CLSClass(bool[] b) {
    }

    static void Main() {}
}
