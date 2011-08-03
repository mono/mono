// CS1649: Members of readonly field `B.a' cannot be passed ref or out (except in a constructor)
// Line: 13

class B
{
	public struct A
	{
	    public int val;
	}

	public readonly A a = new A ();
}

class C
{
    static void f (ref int i)
    {
	i = 44;
    }

    static void Main ()
    {
	B b = new B (); 
	f (ref b.a.val);
    }
}
