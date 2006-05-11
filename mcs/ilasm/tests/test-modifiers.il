// Test for modifiers : modreq & modopt

.assembly extern mscorlib
{
  .ver 2:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'mod'
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute ::.ctor(int32 ) =  (01 00 08 00 00 00 00 00 )

  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module mod.dll

  // Test for modreq
  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
    .field  private  int32  modreq ([mscorlib]System.Runtime.CompilerServices.IsVolatile) f

    .method public hidebysig 
           instance default int32 modreq ([mscorlib]System.Runtime.CompilerServices.IsVolatile)  
	   foo (string modreq ([mscorlib]System.Runtime.CompilerServices.IsVolatile)  s, int32  b)  cil managed 
    {
	.maxstack 1
	.locals init (
		class test modreq ([mscorlib]System.Runtime.CompilerServices.IsVolatile) 	V_0,
		int32 	V_1)
	IL_0000:  nop 
	IL_0001:  ldc.i4.0 
	IL_0002:  stloc.1 
	IL_0003:  br.s IL_0005

	IL_0005:  ldloc.1 
	IL_0006:  ret 
    }

  }

  // Test for modopt
  .class private auto ansi beforefieldinit test2
  	extends [mscorlib]System.Object
  {
    .field  private  int32  modopt ([mscorlib]System.Runtime.CompilerServices.IsVolatile) f

    .method public hidebysig 
           instance default int32 modopt ([mscorlib]System.Runtime.CompilerServices.IsVolatile)  
	   foo (string modopt ([mscorlib]System.Runtime.CompilerServices.IsVolatile)  s, int32  b)  cil managed 
    {
	.maxstack 1
	.locals init (
		class test modopt ([mscorlib]System.Runtime.CompilerServices.IsVolatile) 	V_0,
		int32 	V_1)
	IL_0000:  nop 
	IL_0001:  ldc.i4.0 
	IL_0002:  stloc.1 
	IL_0003:  br.s IL_0005

	IL_0005:  ldloc.1 
	IL_0006:  ret 
    }

  }
