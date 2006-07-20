//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//
// Test for a abstract global method, ilasm makes it
// non-abstract static method

.assembly extern corlib { }

.namespace Mono.ILASM.Tests {

        .method public abstract void global_method () {
                ldstr "PASS"
                call void [corlib]System.Console::WriteLine (string)
                ret
        }
        
	.class public auto ansi Test_20 extends [corlib]System.Object {

               .method public static void check( ) cil managed {
                       .entrypoint
                       call void global_method ()
                       ret
               }
                
	}

}
