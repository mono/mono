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

	.class public auto ansi Test_18 extends [corlib]System.Object {

               .method public static void check( ) cil managed {

                       switch (label1, label2, label3)
                       label1:
                                nop
                       label2:
                                nop
                       label3:
                                nop
                        
			ret
		}
                
	}

}
