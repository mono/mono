// cs0579.cs : Duplicate 'Obsolete' attribute
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
