//
// Mono.ILASM.Tests
//
// Author(s):
//  Ben Maurer
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Ben Maurer, All rights reserved
// (C) 2003 Jackson Harper, All rights reserved
//

.class public Bug {
.method public static   int32 main(class [mscorlib]System.String[]) {
.entrypoint
.maxstack               8
        ldc.i4          0
        ldc.i4          0
        ceq
        brfalse         FAIL
        
try_start:
        leave           try_end
try_end:

handler:
        leave           FAIL
end_handler:
        br              PASS

PASS:
        ldc.i4          0
        ret
FAIL:
        ldc.i4          1
        ret
.try try_start to try_end catch [mscorlib]System.Exception handler
handler to end_handler
}
}


