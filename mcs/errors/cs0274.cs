// cs0274.cs: Accessibility modifiers for both accesors is not allowed
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

