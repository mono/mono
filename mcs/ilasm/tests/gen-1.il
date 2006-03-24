//Tests generic method reference

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly pp
{
  .ver 0:0:0:0
}

.class private auto ansi beforefieldinit 'A`1'<T>
       extends [mscorlib]System.Object
{
  .method public hidebysig static !T  foo<X,Y>(!T _t,
                                               !!X _x) cil managed
  {
    .maxstack  1
    .locals init (!T V_0)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  stloc.0
    IL_0003:  br.s       IL_0005

    IL_0005:  ldloc.0
    IL_0006:  ret
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

}

.class private auto ansi beforefieldinit 'B`1'<U>
       extends [mscorlib]System.Object
{
  .method public hidebysig static void  bar<Z>() cil managed
  {
    // Code size       18 (0x12)
    .maxstack  2
    .locals init (!U V_0)
    IL_0000:  nop
    IL_0001:  ldloca.s   V_0
    IL_0003:  initobj    !U
    IL_0009:  ldloc.0
    IL_000a:  ldc.i4.5
    IL_000b:  call       !0 class 'A`1'<!U>::foo<int32,!U>(!0,
                                                         !!Z)
    IL_0010:  pop
    IL_0011:  ret
  } // end of method B`1::bar

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  } // end of method B`1::.ctor

}
