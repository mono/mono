// Test for generic valuetypes

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly extern one
{
  .ver 0:0:0:0
}
.assembly 'gen-struct'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .ver  0:0:0:0
}
.module 'gen-struct.exe'

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
		valuetype [one]gen_struct`1<string>	V_0)
	IL_0000:  ldloca.s 0
	IL_0002:  initobj valuetype [one]gen_struct`1<string>
	IL_0008:  call void valuetype [one]gen_struct`1<int32>::foo()
	IL_000d:  ret 
    }

  }
