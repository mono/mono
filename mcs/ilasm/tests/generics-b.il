//
// Test instantiating a generic type
//

.assembly extern mscorlib { }


.namespace Mono.ILASM.Tests {

           .class public Set<T> extends [mscorlib]System.Object {

		.method public instance void .ctor () cil managed
		{
			ldarg.0
			call instance void class [mscorlib]System.Object::.ctor()
			ret
		}
           }

           .class public GenericsB extends [mscorlib]System.Object {

                  .method public static void Main ()
                  {
                        .entrypoint
                        .locals init (class Mono.ILASM.Tests.Set<int32>)
                  
                        newobj instance void class Mono.ILASM.Tests.Set<int32>::.ctor ()
                        stloc.0
                        
                        ldloc.0
                        call void [mscorlib]System.Console::WriteLine (object)

                        ret
                  }
                  
           }

}

