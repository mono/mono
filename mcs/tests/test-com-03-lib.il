.assembly extern mscorlib
{
}

.assembly 'test-com-03-lib'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}

.module 'test-com-03-lib.dll'

// Import flag enables more magic

.class interface public auto ansi import C
{
  .method public hidebysig newslot specialname virtual abstract
          instance object 
          get_Value([opt] object a) runtime managed preservesig internalcall
  {
  }

  .property object Value(object)
  {
    .get instance object C::get_Value(object)
  }

  .method public hidebysig newslot specialname virtual abstract
          instance void 
          set_Value2([opt] object a, [opt] object b) runtime managed preservesig internalcall
  {
  }

  .property object Value2(object)
  {
    .set instance void C::set_Value2(object, object)
  }

}
