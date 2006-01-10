// Test : Using VAR/MVAR as class ref
// Eg. !!B as a constraint

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'gen-meth2'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .ver  0:0:0:0
}
.module meth2.dll


  /* Incorrect, meaning-less code, but this is just to
     check support for using VAR/MVAR as class refs */
  .class private auto ansi beforefieldinit g`1<T>
  	extends !T
	implements !T
  {

    .method public hidebysig  specialname  rtspecialname 
           instance default void .ctor ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::.ctor()
	IL_0006:  ret 
    }

    /* Using !!B as a constraint */
    .method public static  hidebysig 
           default !!A foo< (!!B) A,B> (!!B _b, !T _t, !!A _a)  cil managed 
    {
	.maxstack 1
	.locals init (
		!!A	V_0)
	IL_0000:  ldloca.s 0
	IL_0002:  initobj !!0
	IL_0008:  ldloc.0 
	IL_0009:  ret 
    }

  }

