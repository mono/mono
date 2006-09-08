// CS0416: `C<T>': an attribute argument cannot use type parameters
// Line: 15

using System;

public class TestAttribute : Attribute
{
    public TestAttribute(Type type)
    {
    }
}

class C<T>
{
    [Test(typeof(C<T>))]
    public static void Foo()
    {
    }
}