// cs1667.cs: 'System.CLSCompliant' is not valid on property or event accessors. It is valid on 'assembly, module, class, struct, enum, constructor, method, property, indexer, field, event, interface, param, delegate, return, type parameter' declarations only.
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

