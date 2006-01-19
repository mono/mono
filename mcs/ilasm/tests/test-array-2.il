// Test for methodref for array methods
// Eg. class test [,]::.ctor (int32, int32)

.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly 'test-array'
{
  .ver  0:0:0:0
}
.module 'test-array.exe'


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

    .method public static  hidebysig 
           default void Main ()  cil managed 
    {
	.entrypoint
	.maxstack 3
	.locals init (
		class test[,]	V_0)
	IL_0000:  ldc.i4.1 
	IL_0001:  ldc.i4.1 
	IL_0002:  newobj instance void class test[,]::.ctor(int32, int32)
	IL_0007:  stloc.0 
	IL_0008:  ret 
    }

  }
