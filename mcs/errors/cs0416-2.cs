// CS0416: `N.C<T>': an attribute argument cannot use type parameters
// Line: 15

using System;

public class TestAttribute : Attribute
{
    object type;

    public object Type
    {
        get
        {
            return type;
        }
        set
        {
            type = value;
        }
    }
}

namespace N
{
    class C<T>
    {
        [Test(Type=typeof(C<T>))]
        public void Foo()
        {
        }
    }
}