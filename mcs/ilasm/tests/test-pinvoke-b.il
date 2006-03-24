//
// Test pinvoking unmanaged code
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Ximian Inc, (http://www.ximian.com)
//

.assembly extern mscorlib { }
.assembly 'test-pinvoke-b' { }


.class public T {

        .method public static pinvokeimpl ("test-pinvoke" cdecl)
                int32 ret_one () cil managed { }

        .method public static int32 test ()
        {
                .entrypoint

                call int32 T::ret_one ()
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

