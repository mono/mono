.assembly extern mscorlib
{
}

.assembly 'CS1684-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS1684-lib.dll'

.class public auto ansi beforefieldinit A
       extends [mscorlib]TYWSF$
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

}
