
.namespace Mono.ILASM.Test {

        .class public GenComp<T> extends [mscorlib]System.Object {

                .constraint !0 is class [mscorlib]System.IComparable

                .method public void .ctor ()
                {

                }
                
                .method int32 Compare (!0 a, !0 b)
                {
                        ldarg.0
                        ldarg.1
                        ldarg.2
                        callvirt int32 [mscorlib]System.IComparable::Compare (!0, !0)
                        ret
                }
        }

        .class public Driver extends [mscorlib]System.Object {

                .method public static int32 Main ()
                {
                        .entrypoint
                        .locals init (class Mono.ILASM.Test.GenComp<int32>)

                        newobj instance void class Mono.ILASM.Test.GenComp<int32>::.ctor ()
                        stloc.0

                        ldloc.0
                        ldc.i4.1
                        ldc.i4.1
                        callvirt instance int32 class Mono.ILASM.Test.GenComp<int32>::Compare (int32, int32)

                        ret
                }
        }

}

