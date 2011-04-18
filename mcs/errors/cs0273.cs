// CS0273: The accessibility modifier of the `Error0273.Message.get' accessor must be more restrictive than the modifier of the property or indexer `Error0273.Message'
// Line: 8
// Compiler options: -t:library

 class Error0273
 {
	 protected internal string Message {
		 public get {
			 return "Hi";
		 }
		 set {
		 }
	 }

 }

