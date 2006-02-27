// Test for custom attributes on a 'undefined' method type parameter

.assembly extern mscorlib
{
}
.assembly 'err-gen-cattr-2'
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module 'err-gen-cattr-2.exe'

.class private auto ansi beforefieldinit GenParAttribute
       extends [mscorlib]System.Attribute
{
 .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 00 40 00 00 00 00 )
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Attribute::.ctor()
    IL_0006:  ret
  }

}

.class private auto ansi beforefieldinit cons`1<A>
       extends [mscorlib]System.Object
{
  .method public hidebysig instance void 
          abc<M>() cil managed
  {
    .param type A
    .custom instance void GenParAttribute::.ctor() = ( 01 00 00 00 ) 

    .maxstack  8
    IL_0000:  nop
    IL_0001:  ret
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  }

}

