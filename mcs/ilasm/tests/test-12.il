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
// Create a generic type and a generic method
//

.namespace Mono.ILASM.Tests {

        .class public Test_12<int32> extends [mscorlib]System.Object {

               .method void meth<int32> () {

               }
	}
}