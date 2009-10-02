.assembly extern mscorlib
{
}

.assembly 'gtest-464-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'gtest-464-lib.dll'

.class public auto ansi beforefieldinit M extends [mscorlib]System.Object
{
  .method public hidebysig specialname rtspecialname instance void  .ctor() cil managed
  {
    ldarg.0
    call       instance void [mscorlib]System.Object::.ctor()
    ret
  }

  .method public hidebysig static object  Test([opt] object o) cil managed
  {
    .param [1] = int32(0x00000016)
    ldarg.0
    ret
  }
}
