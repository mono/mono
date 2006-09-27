// CS1667: Attribute `System.Diagnostics.ConditionalAttribute' is not valid on property or event accessors. It is valid on `method' declarations only
// GMCS1667: Attribute `System.Diagnostics.ConditionalAttribute' is not valid on property or event accessors. It is valid on `class, method' declarations only
// Line: 11

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
