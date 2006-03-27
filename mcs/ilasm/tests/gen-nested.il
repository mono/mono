// Test for a generic class nested in a non-generic type
// with interface using named generic param ref (!B)

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'gen-nested'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .ver  0:0:0:0
}
.module 'gen-nested.dll'


  .class interface private auto ansi abstract IFoo`1<T>
  {

  }

  .class private auto ansi beforefieldinit outer
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

  /* Generic class nested in a non-generic class */  
  .class nested private auto ansi beforefieldinit inner`2<A,B>
  	extends [mscorlib]System.Object
  	implements class IFoo`1<!B>  {

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

