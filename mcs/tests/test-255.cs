using System;

[AttributeUsage(AttributeTargets.All)]
public class TestAttribute: Attribute
{
}

[type: Obsolete]

public class C {
    [return: Test]
    [Test]
    void Method () {}
    
    public static void Main () {}
}