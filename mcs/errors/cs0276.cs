// cs0276.cs: Accessibility modifiers can only be used if both get and set accessors exist
// Line: 7
// Compiler options: -t:library

 class Error0276 
 {
	 protected internal string Message {
		 internal set {
		 }
	 }

 }

