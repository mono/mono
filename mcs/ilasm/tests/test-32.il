//
// Mono.ILASM.Tests.Test32
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

.assembly extern mscorlib { }

.namespace Mono.ILASM.Tests {

	.class public Test32 {


		//
		// Test using explicit offsets for branch labels
		//
		.method public static int32 main ()
		{
			.entrypoint

			ldc.i4.1 
			brtrue 15
			
			ldstr "FAIL"
			call void [mscorlib]System.Console::WriteLine (string)
			ldc.i4.1
			ret
			
			ldstr "PASS"
			call void [mscorlib]System.Console::WriteLine (string)
			ldc.i4.0

			ret
		}

	}

}

