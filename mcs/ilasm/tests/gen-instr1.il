// Test for type parameter as an operand for
// a type instr
// Here, initobj and box have operand : !0 

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'gpr'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .ver  0:0:0:0
}
.module gpr.dll

  .class private auto ansi beforefieldinit g`1<T>
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
           default void foo ()  cil managed 
    {
	.maxstack 2
	.locals init (
		!0	V_0,
		object	V_1,
		!0 V_2)
	IL_0000:  ldloca.s 2
	IL_0002:  initobj !0
	IL_0008:  ldloc.2 
	IL_0009:  box !0
	IL_000e:  stloc.1 
	IL_000f:  ret 
    }

  }

