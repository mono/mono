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

	.class public auto ansi Test_17 extends [corlib]System.Object {

               .method public static void check( ) cil managed {
                       br IL_TRUE
                       
                       IL_LABEL: nop
                       IL_FALSE: nop
                       IL_TRUE: nop
                        
			ret
		}
                
	}

}
