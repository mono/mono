// CS0274: `Error0274.Message': Cannot specify accessibility modifiers for both accessors of the property or indexer
// Line: 7
// Compiler options: -t:library

 class Error0274 
 {
	 protected internal string Message {
		 protected get {
			 return "Hi";
		 }
		 internal set {
		 }
	 }

 }

