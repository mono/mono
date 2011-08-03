// CS0668: Two indexers have different names; the IndexerName attribute must be used with the same name on every indexer within a type
// Line: 

using System.Runtime.CompilerServices;
class A {
	[IndexerName ("Blah")]
	int this [int a] {
	get { return 1; }
	}
	
	[IndexerName ("Foo")]
	int this [string b] {
	get { return 2; }
	}
	
        public static int Main ()
        {
		int a = 5;
		
		if (!(a is object))
			return 3;

		return 0;
        }
}



