// cs1667.cs: Attribute 'Conditional' is not valid on property or event accessors. It is valid on 'class, method' declarations only.
// Line: 10

using System;
using System.Diagnostics;

class Class1 
{
        public int G {
            [Conditional("DEBUG")]
            get {
                    return 1;
            }
	}
}
