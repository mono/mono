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
// Declare a namespace, a class, and a method
//

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_1 extends [mscorlib]System.Object {

		.method public static void check(string) cil managed {
			.entrypoint
			.maxstack 1	

                        nop			
		}		
	}
}

