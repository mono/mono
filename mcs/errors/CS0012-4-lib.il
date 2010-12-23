.assembly extern mscorlib
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-4-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-4-lib.dll'


.class public auto ansi beforefieldinit BB
       extends [mscorlib]System.Object
       implements ['CS0012-lib-missing']I1
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

}

.class public auto ansi beforefieldinit B
       extends [mscorlib]System.Object
{
  .method public hidebysig specialname instance void 
          set_Prop(class BB 'value') cil managed
  {
    IL_0000:  nop
    IL_0001:  ret
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

  .property instance class BB Prop()
  {
    .set instance void B::set_Prop(class BB)
  }
}
