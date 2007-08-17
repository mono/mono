// CS1729: The type `IgnoreAttribute' does not contain a constructor that takes `0' arguments
// Line: 14

using System;

public class IgnoreAttribute : Attribute {

        public IgnoreAttribute (String name) { }
}

class C {
        
	[Ignore]
	public void Method ()
	{
	}
}
