
.assembly extern mscorlib { }
.assembly 'error-label-a' { }

.class public T {

       .method public static void main ()
       {
                .entrypoint

                ldc.i4.1
                brfalse label_no_exist

                ret
       }

}

