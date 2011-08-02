// CS0579: The attribute `System.ObsoleteAttribute' cannot be applied multiple times
// Line : 17

using System;

[AttributeUsage (AttributeTargets.ReturnValue)]
public class ReturnAttribute : Attribute
{
        public ReturnAttribute ()
        {
	}
}

class MainClass {
        [Obsolete]
        [return: Return]
        [Obsolete]
        static void Main()
        {
        }
}
