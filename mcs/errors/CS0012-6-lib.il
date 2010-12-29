.assembly extern mscorlib
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-6-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-6-lib.dll'

.class public auto ansi beforefieldinit A
       extends ['CS0012-lib-missing']A1
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void ['CS0012-lib-1']A1::.ctor()
    IL_0006:  ret
  }

}