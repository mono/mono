//
// Define a generic method
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
        
.assembly extern mscorlib { }
.assembly 'test-gen-meth-a' { }

.class public T {

        .method public static int32 Main ()
        {
                .entrypoint

                ldc.i4.1
                ret
        }

        .method private static int32 M<I> ()
        {
                ldc.i4.1
                ret
        }
}

