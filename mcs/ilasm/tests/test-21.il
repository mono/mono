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


	.class public auto ansi Test_21 extends [corlib]System.Object {

               .method public static void check( ) cil managed {
                       .entrypoint
                       ldsflda string [corlib]System.String::Empty
                       
                       call void [corlib]System.Console::WriteLine (string)
                       
                       ret
               }
                
	}

}
