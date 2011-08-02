// CS0271: The property or indexer `P.Prop' cannot be used in this context because the get accessor is inaccessible
// Line: 19

class P
{
    public static int Prop
    {
	private get {
	    return 4;
	}
	set {}
    }
}

public class C
{
    public static int Main ()
    {
	return P.Prop;
    }
}