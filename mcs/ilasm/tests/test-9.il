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
// Call a constructor for a type defined in this assembly
//

.namespace Mono.ILASM.Tests {

	.class public auto ansi A extends [mscorlib]System.Object 
	{
		
		.method public instance void .ctor () cil managed
		{
			ldstr "In Constructor"
			call void [mscorlib]System.Console::WriteLine (string)
			ret		
		}
		
	}

	.class public auto ansi Test_9 extends [mscorlib]System.Object 
	{
	
		.method public static void main (string) cil managed {
			.entrypoint
			newobj void Mono.ILASM.Tests.A::.ctor ()
			pop
			ret
		}
	}
	
}

