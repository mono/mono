//
// Test catch and filter blocks
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

.assembly extern mscorlib { }
.assembly 'test-seh-b' { }


.class public T {

        .field private static int32 exc_not_thrown
        .field private static int32 flt_caught
  
        .method public static specialname void .cctor ()
        {
                ldc.i4.0
                stsfld int32 T::exc_not_thrown

                ldc.i4.0
                stsfld int32 T::flt_caught

                ret
        }

        .method public static int32 Main ()
        {
                .entrypoint

                .try {
                        ldc.i4.0
                        ldc.i4.0
                        div

                        ldc.i4.1
                        stsfld int32 T::exc_not_thrown

                        leave outer
                } filter {
                        pop
                        ldc.i4.1
                        endfilter
                } {
                        ldc.i4.1
                        stsfld int32 T::flt_caught
                        leave outer
                }

        outer:
                ldsfld int32 T::exc_not_thrown
                brtrue fail

                ldsfld int32 T::flt_caught
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
                

