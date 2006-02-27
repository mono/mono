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
// Create some bound arrays
//

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_10 extends [mscorlib]System.Object {
               
		.method public static void check(string) cil managed {
			.entrypoint
                        .locals init (int32[0...,0...] local_0,
                                int32[...,...] local_1, int32[2...5],
                                int32[0,0], int32 [0...2,1...4,1...])
			ret
		}		
	}
}