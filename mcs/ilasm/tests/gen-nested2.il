// Test for specifying namespace as part of the type name,
// instead of using .namespace directive. ildasm 2.0 uses
// this style

.assembly extern mscorlib
{
  .ver 1:0:5000:0
}
.assembly 'nested'
{
  .ver  0:0:0:0
}
.module nested.exe

  .class private auto ansi beforefieldinit A.B
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

  .class nested public auto ansi beforefieldinit C
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
  }

 }
  .class private auto ansi beforefieldinit test
  	extends A.B/C
  {

    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void class A.B/C::.ctor()
	IL_0006:  ret 
    }

  }

