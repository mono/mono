//Test for call to a method returning a byref
.assembly extern mscorlib
{
}
.assembly 'test-byref'
{
}

  .class public auto ansi beforefieldinit Test
  	extends [mscorlib]System.Object
  {
    .field  private   float64[,] _data

    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    }

    .method public hidebysig 
           instance default void multByTwo ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldfld  float64[,] Test::_data
	IL_0006:  ldc.i4.0 
	IL_0007:  ldc.i4.0 
	IL_0008:  call instance float64&  float64[,]::Address(int32, int32)
	IL_000d:  dup 
	IL_000e:  ldind.r8 
	IL_000f:  ldc.r8 4
	IL_0018:  mul 
	IL_0019:  stind.r8 
	IL_001a:  ret 
    }
  }
