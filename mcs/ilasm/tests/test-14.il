//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//
.assembly extern corlib { }


.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_14 extends [corlib]System.Object {
	
		.method public static void check (string[] args) cil managed {
			ldarg 0
                        ldarg 1

                        pop
                        pop
		}

	}
}
