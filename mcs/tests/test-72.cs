//
// Compile test for referencing types on nested types
//

using System;

public class outer {
        public class inner {
                public void meth(Object o) {
                        inner inst = (inner)o;
                }
        }
	
	public static int Main ()
	{
		// We only test that this compiles.
		
		return 0;
	}
  }

