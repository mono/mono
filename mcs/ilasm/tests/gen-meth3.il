// Test for an abstract generic method

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'gen-meth3'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .ver  0:0:0:0
}
.module 'gen-meth3.dll'

  .class private auto ansi abstract beforefieldinit test
  	extends [mscorlib]System.Object
  {

    .method family hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    }

    .method public virtual  hidebysig  newslot  abstract 
           instance default void foo<A> ()  cil managed 
    {
    }

  }

