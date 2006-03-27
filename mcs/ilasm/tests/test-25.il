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

           .class public Test_25  {

                .method public static void Main ()
                {

                        .entrypoint
                        .locals init (char[] cha)
                        
                        ldc.i4 10
                        newobj instance void valuetype
                        [mscorlib]System.Char[]::.ctor (int32)
                        pop
                        
                        ret
                }
           }
}

