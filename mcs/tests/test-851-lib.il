.assembly extern mscorlib
{
}

.assembly 'test-851-lib'
{
  .hash algorithm 0x00008004
}

.module 'test-851-lib.dll'

.class interface public auto ansi abstract I
{
  .field  public static initonly  int32 'value'

  .method public static specialname rtspecialname 
           default void '.cctor' ()  cil managed 
  {
    ret
  }

  .method public static specialname 
           default void '__<clinit>' ()  cil managed noinlining 
  {
    ret 
  }

  .class nested public auto ansi abstract sealed __Fields
    extends [mscorlib]System.Object
  {
  }
}
