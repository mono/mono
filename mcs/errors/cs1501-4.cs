// cs1501-4.cs : No overload for method `IgnoreAttribute' takes `0' arguments
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
