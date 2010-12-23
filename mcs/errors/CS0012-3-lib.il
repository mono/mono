.assembly extern mscorlib
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-3-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-3-lib.dll'


.class public auto ansi beforefieldinit BB
       extends ['CS0012-lib-missing']A1
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void ['CS0012-lib-missing']A1::.ctor()
    IL_0006:  ret
  }

}

.class public auto ansi beforefieldinit B
       extends [mscorlib]System.Object
{
  .field public class BB 'value'
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

}
