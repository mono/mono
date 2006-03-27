// Test: Use of !!A, gen param ref by name in method signature,
// or in constraints

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'gen-meth1'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() =  (
		01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
		63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .ver  0:0:0:0
}
.module meth1.dll

  .class interface private auto ansi abstract IFoo`1<A>
  {
  }

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
           default !!A foo<(class g`1<!!B>)A,B> (!!B _b, !T _t, !!A _a)  cil managed 
    {
	.maxstack 1
	.locals init (
		!!A	V_0)
	IL_0000:  ldloca.s 0
	IL_0002:  initobj !!0
	IL_0008:  ldloc.0 
	IL_0009:  ret 
    }

    .method public static  hidebysig
           default void meth1 ()  cil managed
    {
        .maxstack 4
        .locals init (
                !T      V_0,
                !T[]    V_1)
        IL_0000:  ldtoken !T
        IL_0005:  call class [mscorlib]System.Type class [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
        IL_000a:  call object class [mscorlib]System.Activator::CreateInstance(class [mscorlib]System.Type)
        IL_000f:  unbox.any !0
        IL_0014:  stloc.0
        IL_0015:  ldc.i4.5
        IL_0016:  newarr !0
        IL_001b:  stloc.1
        IL_001c:  ret
    }
  }

