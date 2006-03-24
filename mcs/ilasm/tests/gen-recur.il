.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'gen-recur'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .ver  0:0:0:0
}
.module 'gen-recur.dll'

  .class interface private auto ansi abstract IFace`1<T>
  {
  }

  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
	/* class test, implements an interface whose generic arg is
	   test itself */
  	implements class IFace`1<class test>  {

    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    }

  }
