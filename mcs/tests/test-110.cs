//
// Special test case for the Compound Assignment for the
// second case (not the obvious one, but the one with 
// implicit casts)

using System;

namespace test
{
        public class test
        {
                static int test_method(int vv)
                {
			byte b = 45;

			// The cast below will force the expression into being
			// a byte, and we basically make an explicit cast from
			// the return of "<<" from int to byte (the right-side type
			// of the compound assignemtn)
                        b |= (byte)(vv << 1);

                        return b;
                }

                public static int Main ()
                {
			if (test_method (1) != 47)
				return 1;
			return 0;
                }
        }
}
