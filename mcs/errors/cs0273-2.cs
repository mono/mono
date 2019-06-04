// CS0273: The accessibility modifier of the `C.S2.set' accessor must be more restrictive than the modifier of the property or indexer `C.S2'
// Line: 7
// Compiler options: -langversion:7.2

 class C
 {
	private string S2 { get; private protected set; }
 }

