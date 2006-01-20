//
// Test to make sure comments are being handled properly
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

.assembly extern mscorlib { }
.assembly 'test-comment-a' { }


.class public T {

        .method public static int32 Main ()
        {
                .entrypoint

                ldc.i4.0

                // ldc.i4.1
                /**/ 
                /* ldc.i4.1     */
                /*
                ldc.i4.1
                */

                /*
                //
                */
                
                ret
        }

}


