// cs3000.cs: Methods with variable arguments are not CLS-compliant
// Line: 10

using System;

[assembly: CLSCompliant (true)]

public class M
{
    public void Method (__arglist)
    {
    }
}