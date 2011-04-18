// CS1540: Cannot access protected member `A.Test' via a qualifier of type `B'. The qualifier must be of type `C' or derived from it
// Line: 17

class A
{
    protected object[] Test { get { return null; } }
}

class B : A
{
}

class C: A
{
    public void Test2 (B b)
    {
        foreach (object o in b.Test)
        {
        }
    }

}
