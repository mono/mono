// CS8200: Out variable and pattern variable declarations are not allowed within constructor initializers, field initializers, or property initializers
// Line: 6

class X
{
    public static bool Test { get; } = Foo () is bool x;

    static object Foo ()
    {
        return false;
    }
}