//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//
.assembly extern mscorlib { }
.assembly test_9 { }
.module test_9.exe

.namespace Mono.ILASM.Tests {

	.class public auto ansi A extends [mscorlib]System.Object 
	{
		
		.method public void .ctor () cil managed
		{
			ret		
		}
		
	}

	.class public auto ansi Test_9 extends [mscorlib]System.Object 
	{
	
		.method public static void main (string) cil managed {
			.entrypoint
			newobj void Mono.ILASM.Tests.A::.ctor ()
			ret
		}
	}
	
}

