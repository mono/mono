//
// Test multiple catch blocks
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

.assembly extern mscorlib { }
.assembly extern System { }
.assembly 'test-seh-b' { }


.class public T {

        .field private static int32 exc_not_thrown
        .field private static int32 catch_1
        .field private static int32 catch_2
        .field private static int32 catch_3
  
        .method public static specialname void .cctor ()
        {
                ldc.i4.0
                stsfld int32 T::exc_not_thrown

                ldc.i4.0
                stsfld int32 T::catch_1

                ldc.i4.0
                stsfld int32 T::catch_2

                ldc.i4.0
                stsfld int32 T::catch_3

                ret
        }

        .method public static int32 Main ()
        {
                .entrypoint

                //
                // Only the second catch block should be executed
                //
                
                .try {
                        ldc.i4.0
                        ldc.i4.0
                        div

                        ldc.i4.1
                        stsfld int32 T::exc_not_thrown

                        leave outer
                } catch [mscorlib]System.InvalidCastException {
                        ldc.i4.1
                        stsfld int32 T::catch_1
                        leave outer
                } catch [mscorlib]System.DivideByZeroException {
                        ldc.i4.1
                        stsfld int32 T::catch_2
                        leave outer
                } catch [mscorlib]System.Exception {
                        ldc.i4.1
                        stsfld int32 T::catch_3
                        leave outer
                }
           

        outer:
                ldsfld int32 T::exc_not_thrown
                brtrue fail

                ldsfld int32 T::catch_1
                brtrue fail

                ldsfld int32 T::catch_2
                brfalse fail

                ldsfld int32 T::catch_3
                brtrue fail

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
                

