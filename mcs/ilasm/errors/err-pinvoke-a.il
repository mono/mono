// Test for a pinvoke method with a body.

.assembly extern mscorlib { }
.assembly 'err-pinvoke-a' { }


.class public T {

        .method public static pinvokeimpl ("test-pinvoke" as "ret_one" cdecl)
                int32 RetOne () cil managed 
	{ 
		ret 
	}

        .method public static int32 test ()
        {
                .entrypoint

                ret
        }

}

