//
// This test verifies that we resolve the source expression in a compound
// expression before we attempt to use it.
//

using System.Collections.Specialized;

class D {
	static void A (NameValueCollection n)
        {
                n ["a"] += ";";
        }

	static int Main ()
	{
		return 0;
	}
}

