//
// Test a generic type type param field
//

.assembly extern mscorlib { }


.namespace Mono.ILASM.Tests {

           .class public Set<T> extends [mscorlib]System.Object {

                  .field public !0 val

		.method public instance void .ctor () {
			ret
		}
           }

           .class public Driver extends [mscorlib]System.Object {

                  .method public static void Main ()
                  {
                        .entrypoint
                        .locals init (class Mono.ILASM.Tests.Set<int32>)

                        newobj instance void class Mono.ILASM.Tests.Set<int32>::.ctor ()
                        stloc.0
                        
                        ldloc.0
                        ldc.i4.1
                        stfld int32 class Mono.ILASM.Tests.Set<int32>::val

                        ldloc.0
                        ldfld int32 class Mono.ILASM.Tests.Set<int32>::val
                        call void [mscorlib]System.Console::WriteLine (int32)

                        ret
                  }

           }
}

