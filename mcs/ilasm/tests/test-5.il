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
// Define a field
//

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_5 extends [mscorlib]System.Object {
		
		/// <summary>
		///  field that references a primative type
		/// </summary>
		.field public string int_field
	}
}

