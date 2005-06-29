// cs0415.cs: The `IndexerName' attribute is valid only on an indexer that is not an explicit interface member declaration
// Line: 11

using System.Runtime.CompilerServices;

interface A {
	int this [int a] { set; }
}

class X : A {
        [IndexerName("Error")]
	int A.this [int a] { set {} }
}

