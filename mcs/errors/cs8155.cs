// CS8155: Lambda expressions that return by reference cannot be converted to expression trees
// Line: 14

using System.Linq.Expressions;

class TestClass
{
    static int x;

    delegate ref int D ();

    static void Main ()
    {
        Expression<D> e = () => ref x;
    }
}