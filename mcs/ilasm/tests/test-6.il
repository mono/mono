//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//
.assembly extern mscorlib { }
.assembly test_6 { }
.module test_6.exe

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_6 extends [mscorlib]System.Object {
	
		/// <summary>
		///  Field that references a type in this assembly
		/// </summary>
		.field public Mono.ILASM.A a_field
	}
}

