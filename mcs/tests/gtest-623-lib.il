.assembly extern mscorlib
{
}

.assembly 'gtest-623-lib'
{
}

.module 'gtest-623-lib.dll'

  .class public sequential ansi sealed beforefieldinit S
  	extends [mscorlib]System.ValueType
  	implements I  {

    .field  public  int32 Field

    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
		ldarg.0 
		ldc.i4.3 
		stfld int32 S::Field
		ret 
    }

    .method public final virtual hidebysig newslot
           instance default int32 GetValue ()  cil managed
    {
		ldarg.0
		ldfld int32 S::Field
		ret
    }
  }

  .class interface public auto ansi abstract I
  {

    .method public virtual hidebysig newslot abstract
           instance default int32 GetValue ()  cil managed
    {
    }
  }
