.assembly extern mscorlib
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-lib.dll'

.class public auto ansi beforefieldinit B
       extends [mscorlib]System.Object
{
  .method public hidebysig instance class ['CS0012-lib-missing']A1 
          Test() cil managed
  {
    .locals init (class ['CS0012-lib-missing']A1 V_0)
    IL_0000:  nop
    IL_0001:  ldnull
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
