//
// Mono.ILASM.Tests
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

.assembly extern corlib { }

.namespace Mono.ILASM.Tests {

        .field private static string global_string

        .method private static void .cctor ()
        {
                ldstr "PASS"
                stsfld string global_string
                ret
        }

        .method public static void Main ()
        {
                .entrypoint

                ldsfld string global_string

                call void [mscorlib]System.Console::WriteLine (string)

                ret
        }
        
}
