.assembly extern mscorlib
{
}

.assembly extern 'missing-lib'
{
}

.assembly 'gtest-583-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'gtest-583-lib.dll'

.class public auto ansi beforefieldinit C`1<T>
       extends [mscorlib]System.Object
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    ldarg.0
    call       instance void [mscorlib]System.Object::.ctor()
    ret
  }

  .method public hidebysig instance int32 
          Join(class [mscorlib]System.Collections.Generic.List`1<class ['missing-lib']N/M> arg) cil managed
  {
    ldc.i4.1
    ret
  }
}
