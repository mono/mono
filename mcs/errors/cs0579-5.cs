// CS0579: The attribute `ReturnAttribute' cannot be applied multiple times
// Line : 17

using System;
using System.Reflection;

[AttributeUsage (AttributeTargets.ReturnValue)]
public class ReturnAttribute : Attribute
{
        public ReturnAttribute ()
        {
	}
}

public class Blah {
        [return: Return ()]
        [return: Return ()]
	public static void Main () { }
}

