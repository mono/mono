.assembly extern mscorlib
{
}

.assembly 'test-823-lib'
{
  .hash algorithm 0x00008004
}

.module 'test-823-lib.dll'

.class public auto ansi beforefieldinit A
       extends [mscorlib]System.Object
{
  .method public hidebysig specialname instance int32 get_Prop() cil managed
  {
    ldc.i4.1
    ret
  }

  .method public hidebysig specialname instance int32 get_Prop(int32 arg) cil managed
  {
    ldc.i4.2
    ret
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

  .property instance int32 Prop()
  {
    .get instance int32 A::get_Prop()
  }
  
  .property instance int32 Prop(int32)
  {
    .get instance int32 A::get_Prop(int32)
  }
}
