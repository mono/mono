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
// Define classes outside of namespaces, also define a field that
// references a type in this assembly
//

.class public auto ansi Test_6 extends [mscorlib]System.Object {
	
	/// <summary>
	///  Field that references a type in this assembly
	/// </summary>
       .field public class A a_field
}

.class public auto ansi A extends [mscorlib]System.Object {

}
