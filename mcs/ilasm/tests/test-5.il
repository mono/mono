//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//
.assembly extern mscorlib { }
.assembly test_5 { }
.module test_5.exe

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_5 extends [mscorlib]System.Object {
		
		/// <summary>
		///  field that references a primative type
		/// </summary>
		.field public string int_field
	}
}

