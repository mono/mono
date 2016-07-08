.assembly extern mscorlib
{
}

.assembly 'test-936-lib'
{
}

.class public auto ansi sealed beforefieldinit TypeWithIndexer
       extends [mscorlib]System.Object
{
  .custom instance void [mscorlib]System.Reflection.DefaultMemberAttribute::.ctor(string) = ( 01 00 04 49 74 65 6D 00 00 )                      // ...Item..

  .method public hidebysig specialname rtspecialname
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

  .field private uint8[] a
  .field private int32 b
  .method public hidebysig specialname instance uint8 
          get_B(int32 index) cil managed
  {
    IL_0000:  ldc.i4.0
    IL_0001:  conv.u1
    IL_0002:  ret
  }

  .method public hidebysig specialname 
          instance void  set_O(int32 index,
                           uint8 A_1) cil managed
  {
    IL_0000:  ret
  }

  .property instance uint8 Item(int32)
  {
    .get instance uint8 TypeWithIndexer::get_B(int32)
    .set instance void TypeWithIndexer::set_O(int32,
                                          uint8)
  }
}
