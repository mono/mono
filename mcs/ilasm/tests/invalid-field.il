// Test for ref to an non-existant field

.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly 'invalid-field'
{
}

  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    }

    .method public hidebysig 
           instance default void foo ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  ldc.i4.5 
	IL_0002:  stfld  int32 test::invalid
	IL_0007:  ret 
    }

  }

