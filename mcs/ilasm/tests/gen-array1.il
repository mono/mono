// Test that no duplicate TypeSpec entries are emitted 
//
// Multiple instaces of generic type class g<T>, with the same
// argument (int32). Only one typespec should be emitted and shared by all these.

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'gen-array-1'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .ver  0:0:0:0
}
.module arr.exe

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
		/* Multiple generic instances with same argument */
		class g`1<int32>[]	V_0,
		class g`1<int32>[][]	V_1,
		class g`1<int32>	V_2,
		class g`1<int32>[]	V_3,
		class g`1<class g`1<int32>>	V_4,
		class g`1<class g`1<int32>>[]	V_5,
		class g`1<class g`1<int32>>[][]	V_6,
		class g`1<class g`1<int32>[]>[][]	V_7,
		class g`1<class g`1<int32>[][]>[][]	V_8)
	IL_0000:  ldc.i4.s 0x36
	IL_0002:  newarr class g`1<int32>
	IL_0007:  stloc.0 
	IL_0008:  ldloc.0 
	IL_0009:  ldc.i4.0 
	IL_000a:  newobj instance void class g`1<int32>::.ctor()
	IL_000f:  stelem.ref 
	IL_0010:  ret 
    }

  }
