//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//
.assembly extern mscorlib { }
.assembly HelloWorld { }
.module HelloWorld.exe

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_1 extends [mscorlib]System.Object {

		.method public instance void .ctor () cil managed {
			ldstr "In Constructor"
		}
	
		.method public static void check(string) cil managed {
			.entrypoint
			.maxstack 1	
			.locals init ([mscorlib]System.Object V_0)
			
			ldnull
			pop
			
			nop
			nop
			
			newobj void Mono.ILASM.Tests.Test_1::.ctor ()

			ldstr "Hello World."
			call void Mono.ILASM.Tests.Test_1::check (string)
			ldstr "Hello World"
			call instance void Mono.ILASM.Tests.Test_1::a ()

			ldfld void [mscorlib]System.Object::a

			initobj Mono.ILASM.Tests.Test_1

			ret
		}		

		.method public static void a ()
		{

		}
	}
}

