//
// Test instantiating a generic type
//

.assembly extern mscorlib { }


.namespace Mono.ILASM.Tests {

           .class public Set<T> extends [mscorlib]System.Object {

           }

           .class public GenericsB extends [mscorlib]System.Object {

                  .method public static void Main ()
                  {
                        .entrypoint
                        .locals init (class Mono.ILASM.Tests.Set<int32>)
                  
                        ldloc.0
                        call void [mscorlib]System.Console::WriteLine (object)

                        ret
                  }
                  
           }

}

