//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//
.assembly extern corlib { }

//
// Define a method and execute that method
//

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_3 extends [corlib]System.Object {
	
		.method public static void check( ) cil managed {
			.entrypoint
			.maxstack 1	
			
			call void Mono.ILASM.Tests.Test_3::print_hello()

			ret
		}

		.method private static void print_hello() cil managed {
			ldstr "PASS"
			call void [corlib]System.Console::WriteLine(string)
		}
	
	}
}

