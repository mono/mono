//
// This tests member lookups on inherited interfaces.
//
// The bug was exposed because FindMembers in MemberLookup
// would not return all the members on interfaces, but only
// the members from the most close type.
//

using System;
using System.Collections;

namespace N1
{	
	interface A
	{
		void method1 ();
	}
	
	interface B:A
	{
		void method2 ();
	}

	public class C
	{
		void method (ref B p)
		{
			p.method2();//<- works declared in 'B'
			p.method1();//<- fails declared in 'A'
		}
	}
}


class Test {
        public static int Main () {
                IList list = new ArrayList ();
                int n = list.Count;

		return 0;
        }
}





