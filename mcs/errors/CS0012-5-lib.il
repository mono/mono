.assembly extern mscorlib
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-5-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-5-lib.dll'

.class public auto ansi beforefieldinit B
       extends [mscorlib]System.Object
{
  .class sequential ansi sealed nested public beforefieldinit N
         extends [mscorlib]System.ValueType
         implements ['CS0012-lib-missing']I1
  {
    .pack 0
    .size 1
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

}
