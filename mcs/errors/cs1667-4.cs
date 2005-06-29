// cs1667-4.cs: Attribute `System.CLSCompliantAttribute' is not valid on property or event accessors. It is valid on `assembly, module, class, struct, enum, constructor, method, property, indexer, field, event, interface, parameter, delegate, return' declarations only
// Line: 10

using System;
using System.Diagnostics;

class Class1 
{
        public event ResolveEventHandler G {
            [CLSCompliant(false)]
            add {}
            remove {}
	}
}

