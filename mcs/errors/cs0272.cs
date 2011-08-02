// CS0272: The property or indexer `P.Prop' cannot be used in this context because the set accessor is inaccessible
// Line: 19

class P
{
    public static int Prop
    {
	get {
	    return 4;
	}
	private set {}
    }
}

public class C
{
    public static void Main ()
    {
	P.Prop = 453422;
    }
}