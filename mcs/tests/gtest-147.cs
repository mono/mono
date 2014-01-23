namespace A
{
    interface IFoo
    {
        void Hello (IFoo foo);
    }
}

namespace B
{
  partial class Test <T> : IDoo, A.IFoo where T : A.IFoo
    { }
}

namespace B
{
    using A;

    partial class Test <T> : Y, IFoo where T : IFoo
    {
        void IFoo.Hello (IFoo foo)
        { }
    }
}

interface IDoo { }

class Y { }

class X
{
    public static void Main ()
    { }
}




