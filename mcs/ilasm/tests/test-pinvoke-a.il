//
// Test pinvoking unmanaged code
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Ximian Inc, (http://www.ximian.com)
//

.assembly extern mscorlib { }
.assembly 'test-pinvoke-a' { }


.class public T {

        .method public static pinvokeimpl ("test-pinvoke" as "ret_one" cdecl)
                int32 RetOne () cil managed { }

        .method public static int32 test ()
        {
                .entrypoint

                call int32 T::RetOne ()
                brfalse FAIL

        PASS:
                ldstr "PASS"
                call void [mscorlib]System.Console::WriteLine (string)
                ldc.i4.0
                ret

        FAIL:
                ldstr "FAIL"
                call void [mscorlib]System.Console::WriteLine (string)
                ldc.i4.1
                ret
        }

}

