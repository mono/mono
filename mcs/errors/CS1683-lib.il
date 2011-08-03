.assembly extern mscorlib
{
}

.assembly extern 'cs1683'
{
  .ver 0:0:0:0
}

.assembly 'CS1683-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS1683-lib.dll'

.class public auto ansi beforefieldinit A
       extends ['cs1683']C
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

}
