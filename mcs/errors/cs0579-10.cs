// CS0579: The attribute `SimpleAttribute' cannot be applied multiple times
// Line : 18

using System;

[AttributeUsage (AttributeTargets.All, AllowMultiple = false)]
public class SimpleAttribute : Attribute {

	public SimpleAttribute ()
	{
	}
	
}

[Simple]
public partial class Blah { }

[Simple]
public partial class Blah { }

