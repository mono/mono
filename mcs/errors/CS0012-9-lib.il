.assembly extern mscorlib
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-9-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-9-lib.dll'

.class public auto ansi beforefieldinit A
       extends [mscorlib]System.Object
{
  .method public hidebysig static void  M<(['CS0012-lib-missing']A1) T>() cil managed
  {
    IL_0000:  ret
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

}
