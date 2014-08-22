// CS0181: Attribute constructor parameter has type `int[,]', which is not a valid attribute parameter type
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