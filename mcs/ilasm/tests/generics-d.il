//
// Test a generic type type param field
//

.assembly extern mscorlib { }


.namespace Mono.ILASM.Tests {

           .class public Set<T> extends [mscorlib]System.Object {

                  .field public !0 val

		.method public instance void .ctor (!0 arg) {
			ldarg.0
			ldarg.1
			stfld !0 class Mono.ILASM.Tests.Set::val
			ret
		}

		.method public instance !0 access () {
                        .locals init (!0)
			ldarg.0
			ldfld !0 class Mono.ILASM.Tests.Set::val
			// just to make sure locals work, too
			stloc.0
			ldloc.0
			ret
		}
           }

           .class public Driver extends [mscorlib]System.Object {

                  .method public static void Main ()
                  {
                        .entrypoint
                        .locals init ( class Mono.ILASM.Tests.Set<int32>,
				class Mono.ILASM.Tests.Set<float64>)

			ldc.i4.4
                        newobj instance void class Mono.ILASM.Tests.Set<int32>::.ctor (int32)
                        stloc.0
                        
                        ldloc.0
                        call instance int32 class Mono.ILASM.Tests.Set<int32>::access ()
                        call void [mscorlib]System.Console::WriteLine (int32)

			ldc.r8 35.6
                        newobj instance void class Mono.ILASM.Tests.Set<float64>::.ctor (float64)
                        stloc.1
                        
                        ldloc.1
                        call instance float64 class Mono.ILASM.Tests.Set<float64>::access ()
                        call void [mscorlib]System.Console::WriteLine (float64)                        ret
                  }

           }
}

