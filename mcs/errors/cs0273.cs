// cs0273.cs: Accessibility modifier must be more restrictive than the property access
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

