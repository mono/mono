//
// This test verifies that we resolve the source expression in a compound
// expression before we attempt to use it.
//
// The test also attempts 
//

using System.Collections.Specialized;

public class MyClass
{
	public Container this [ string s ]
	{
		get { return null; }
		set { ; }
	}		
}


public class Container
{
	public static Container operator + ( Container c, object o )
	{
		return c;
	}	
}

class D {
	static void A (NameValueCollection n, MyClass m, object o)
        {
		//
		// Tests that ";" is a StringLiteral, *and* it has been resolved.  Triggered
		// by indexers, as indexers trigger an OverloadResolve.
		//
                n ["a"] += ";";

		//
		// A different, but similar beast.  A bug existed in the compiler that
		// prevented the following from working (bug 36505)
		//
		m["apple"] += o;
        }

	
	public static int Main ()
	{
		return 0;
	}
}

