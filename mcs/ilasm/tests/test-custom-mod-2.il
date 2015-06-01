.assembly extern mscorlib
{
}

.assembly 'test-custom-mod-2'
{
}

.class interface public abstract auto ansi I
{
  .custom instance void [mscorlib]System.Reflection.DefaultMemberAttribute::.ctor(string)
           = {string('Item')}
  .method public hidebysig newslot specialname abstract virtual instance char
          get_P() cil managed
  {
  }

  .method public hidebysig newslot specialname abstract virtual instance void 
          set_P(char 'value') cil managed
  {
  }

  .method public hidebysig newslot specialname abstract virtual instance int32
          get_Item(bool x) cil managed
  {
  }

  .method public hidebysig newslot specialname abstract virtual instance void 
          set_Item(bool x,
                   int32 'value') cil managed
  {
  }

  .property instance char  modopt(int8) P()
  {
    .get instance char I::get_P()
    .set instance void I::set_P(char)
  }

  .property instance int32 modopt(int8) Item(bool  modopt(int16))
  {
    .get instance int32 I::get_Item(bool)
    .set instance void I::set_Item(bool,
                                      int32)
  }
}