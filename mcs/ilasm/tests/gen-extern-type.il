// Test for Ref to external generic types

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly extern one
{
  .ver 0:0:0:0
}
.assembly 'gen-extern-type'
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )
  .ver 0:0:0:0
}

.module use.exe

.class private auto ansi beforefieldinit test
       extends [mscorlib]System.Object
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

  .method public hidebysig static void  Main() cil managed
  {
    .entrypoint
    .maxstack  3
    .locals init (class [one]ns.gen_m`1<int32>[] V_0)
    IL_0000:  ldc.i4.5
    IL_0001:  newarr     class [one]ns.gen_m`1<int32>
    IL_0006:  stloc.0
    IL_0007:  ldloc.0
    IL_0008:  ldc.i4.0
    IL_0009:  newobj     instance void class [one]ns.gen_m`1<int32>::.ctor()
    IL_000e:  stelem.ref
    IL_000f:  ldstr      "asd"
    IL_0014:  call       void class [one]ns.gen_m`1<int32>::foo<string>(!!0)
    IL_0019:  ldc.i4.5
    IL_001a:  call       void class [one]ns.gen_m`1<int32>::foo<int32,string>(!!0)
    IL_001f:  ret
  }

}
