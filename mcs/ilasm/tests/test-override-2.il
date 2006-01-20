// Test for "full" syntax of specifying overrides.
// This has method in a generic type

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'test-override-2'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) =  (01 00 08 00 00 00 00 00 ) // ........

  .ver  0:0:0:0
}
.module 'test-override-2.dll'


  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  	implements class IFoo`1<string>  {

    .method public final  virtual  hidebysig  newslot 
           instance default void foo (class IFoo`1<string> _f)  cil managed 
    {
	.override method instance void class IFoo`1<string>::foo(class IFoo`1<string>)
	.maxstack 8
	IL_0000:  nop 
	IL_0001:  ret 
    }

    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    }

  }

  .class interface private auto ansi abstract IFoo`1<T>
  {
    .method public virtual  hidebysig  newslot  abstract 
           instance default void foo (class IFoo`1<!T> _f)  cil managed 
    {
    }

  }
