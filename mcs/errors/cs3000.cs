// CS3000: Methods with variable arguments are not CLS-compliant
// Line: 11
// Compiler options: -warnaserror -warn:1

using System;

[assembly: CLSCompliant (true)]

public class M
{
    public void Method (__arglist)
    {
    }
}
