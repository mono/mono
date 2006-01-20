// Test for large numbers

.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly 'max'
{
  .ver  0:0:0:0
}
.module max.exe


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
	.maxstack 1
	.locals init (
		unsigned int64	V_0)
	ldc.i8 9223372036854775810
	pop
	ldc.r8 9223372036854775810
	pop
	ldc.i8 -9223372036854775808
	IL_0009:  stloc.0 
	IL_000a:  ret 
    }

  }
