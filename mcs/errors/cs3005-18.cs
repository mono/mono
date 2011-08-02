// CS3005: Identifier `B.TEST()' differing only in case is not CLS-compliant
// Line: 15
// Compiler options: -warnaserror

using System;

[assembly: CLSCompliantAttribute (true)]

public class A
{
    [CLSCompliant (false)]
    public void Test () {}
        
    public void test () {}
}

public class B: A
{
    public void TEST () {} 
}
