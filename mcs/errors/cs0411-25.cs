// CS0411: The type arguments for method `C.Foo<T>(System.Func<T>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 10

using System;

public class C
{
    public static void Main ()
    {
        Foo (() => throw null);
    }

    static void Foo<T> (Func<T> arg)
    {
    }
}