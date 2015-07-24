.assembly extern mscorlib
{
}

.assembly 'custom-attrs-01'
{
}

.class interface public abstract auto ansi I1
{
  .custom instance void [mscorlib]System.Reflection.DefaultMemberAttribute::.ctor(string)
           = {bytearray(55 05)}
  .method public hidebysig newslot specialname abstract virtual 
          instance int32  get_A(int32 x) cil managed
  {
  }
}

.class interface public abstract auto ansi I2
{
  .custom instance void [mscorlib]System.Reflection.DefaultMemberAttribute::.ctor(string)
           = {string('B')}
  .method public hidebysig newslot specialname abstract virtual 
          instance int32  get_B(int32 x) cil managed
  {
  }
}
