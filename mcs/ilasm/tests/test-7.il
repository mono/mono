//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//
.assembly extern mscorlib { }
.assembly test_7 { }
.module test_7.exe

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_7 extends [mscorlib]System.Object {
	
		/// <summary>
		///   field that references an external type 
		/// </summary>
		.field public [Cscompmgd]Microsoft.CSharp.Compiler a_compiler
	}
}

