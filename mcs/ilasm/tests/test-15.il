//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//
.assembly extern corlib { }

.namespace Mono.ILASM.Tests {

	.class public auto ansi Test_14 extends [corlib]System.Object {

	}

        
        .class public auto ansi A extends [corlib]System.Object {

               .field public class Mono.ILASM.Tests.B b
        }

        .class public auto ansi B extends Mono.ILASM.Tests.A {

               .field public class Mono.ILASM.Tests.A a
        }
        
}
