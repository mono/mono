// CS0119: Expression denotes a `variable', where a `type' or `method group' was expected
// Line: 10

delegate void D ();

class C
{
    public void Foo (int i)
    {
        D d = new D (i);
    }
}
