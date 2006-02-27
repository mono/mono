//
// Test catch and finally blocks
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

.assembly extern mscorlib { }
.assembly extern System { }
.assembly 'test-seh-a' { }


.class public T {

        .field private static int32 exc_caught
        .field private static int32 fin_caught
  
        .method public static specialname void .cctor ()
        {
                ldc.i4.0
                stsfld int32 T::exc_caught

                ldc.i4.0
                stsfld int32 T::fin_caught

                ret
        }

        .method public static int32 Main ()
        {
                .entrypoint

                .try {
                        .try {
                                ldstr "#jibba!!jabba"
                                newobj void [System]System.Uri::.ctor (string)
                                leave outer
                        } catch [mscorlib]System.Exception {
                                ldc.i4.1
                                stsfld int32 T::exc_caught
                                leave outer
                        }
                        leave outer
                 } finally {
                        ldc.i4.1
                        stsfld int32 T::fin_caught
                        endfinally
                 }
           

        outer:
                ldsfld int32 T::exc_caught
                brfalse fail

                ldsfld int32 T::fin_caught
                brfalse fail

        pass:
                ldstr "PASS"
                call void [mscorlib]System.Console::WriteLine (string)
                ldc.i4.0
                ret

        fail:
                ldstr "FAIL"
                call void [mscorlib]System.Console::WriteLine (string)
                ldc.i4.1
                ret

        }

}
                

