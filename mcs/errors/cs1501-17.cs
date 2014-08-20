// CS1501: No overload for method `Foo' takes `0' arguments
// Line: 12

class C
{
    static void Foo (string foo, params object [] moreFoo)
    {
    }

    static void Main ()
    {
        Foo ();
    }
}