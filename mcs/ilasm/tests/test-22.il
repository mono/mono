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

        .field public static string global_field = "jackson"

	.class public auto ansi Test_22 extends [corlib]System.Object {

               .method public static void check( ) cil managed {
                       .entrypoint
                       
                       ldfld string global_field
                       call void [corlib]System.Console::WriteLine (string)
                       
                       ret
               }
                
	}

}
