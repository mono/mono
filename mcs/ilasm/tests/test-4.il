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
// Pass a param to a method
//

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_4 extends [corlib]System.Object {
	
		.method public static void check( ) cil managed {
			.entrypoint
			.maxstack 1	
			
			ldstr "PASS"
			call void Mono.ILASM.Tests.Test_4::print_string(string)

			ret
		}

		.method private static void print_string(string) cil managed {
			ldarg.0
			call void [corlib]System.Console::WriteLine(string)
		}
	
	}
}

