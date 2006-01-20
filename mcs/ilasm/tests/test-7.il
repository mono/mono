//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//
.assembly extern mscorlib { }
.assembly extern Cscompmgd { }


//
// Define a method that references a type in another assembly (that is
// not corlib)
//

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_7 extends [mscorlib]System.Object {
	
		/// <summary>
		///   field that references an external type 
		/// </summary>
		.field public [Cscompmgd]Microsoft.CSharp.Compiler a_compiler
	}
}

