// CS0668: Two indexers have different names; the IndexerName attribute must be used with the same name on every indexer within a type
// Line: 11

using System.Runtime.CompilerServices;
class A {
	[IndexerName ("Blah")]
	int this [int a] {
	get { return 1; }
	}
	
	int this [string b] {
	get { return 2; }
	}
}



