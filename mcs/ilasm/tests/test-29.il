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

           
        .class public Test_29<T> extends [mscorlib]System.Object {
        
               .constraint !0 is class [mscorlib]System.Collections.IList
               
               .method void meth<T> () {

               }
	}
}