// CS0182: An attribute argument must be a constant expression, typeof expression or array creation expression
// Line: 13

using System;

class TestAttribute: Attribute
{
    public TestAttribute (int[,] i) {}
}

public class E
{
    [Test (null)]
    public void Method () {}
}