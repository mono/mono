//
// Test emitting data and the field mapping with the data
// defined in another class. (Actually all .data is global,
// even if it is defined in a class)
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

.assembly extern mscorlib { }
.assembly 'test-data-b' { }

.class public Stub extends [mscorlib]System.Object {
       .data some_data = int32 (1)
}

.class public T extends [mscorlib]System.Object {

        .field private static int32 some_field at some_data

        .method public static int32 main ()
        {
                .entrypoint

                ldsfld int32 T::some_field
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
