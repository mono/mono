// No entry in the TypeSpec table should be emitted for generic types (g<> and gen<>)
// referenced in local var sig as it(GenericInst) is emitted the sig itself, and not a TypeSpec token

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly extern one
{
}

.assembly 'gen-local'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .ver  0:0:0:0
}
.module 'gen-local.exe'

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
  }

  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
    .method public static  hidebysig 
           default void Main ()  cil managed 
    {
	.entrypoint
	.maxstack 3
	.locals init (
		class g`1<string> V_0,
		class [one]gen`1<string> V_1)
	IL_0010:  ret 
    }

  }
