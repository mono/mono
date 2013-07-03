.assembly extern mscorlib
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-21-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-21-lib.dll'


.class public auto ansi beforefieldinit B
       extends class ['CS0012-lib-missing']X`1/Y/Z/W<int32>
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void class ['CS0012-lib-missing']X`1/Y/Z/W<int32>::.ctor()
    IL_0006:  ret
  }

}
