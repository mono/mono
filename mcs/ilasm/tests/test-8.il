//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//
.assembly extern mscorlib { }

//
// Implement an interface
//

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_8
		extends [mscorlib]System.Collections.ArrayList 
		implements [mscorlib]System.Collections.IList {

	}
}

