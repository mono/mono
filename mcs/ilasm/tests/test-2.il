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
// Declare a local object instantiate it and print it to stdout
//

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_2 extends [mscorlib]System.Object {
	
		.method public static void main () cil managed {
			.entrypoint
			.maxstack 1	
			.locals init (object obj)

			newobj instance void [mscorlib]System.Object::.ctor()
			stloc.0 
			ldloc.0 
			call void [mscorlib]System.Console::WriteLine(object)
		
			ret
		}		
	}
}

