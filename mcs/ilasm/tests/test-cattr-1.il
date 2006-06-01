//Test for custom attributes on return type of a method

.assembly extern mscorlib
{
}
.assembly bb
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}
.module bb.exe

.class private auto ansi beforefieldinit GenParAttribute
       extends [mscorlib]System.Attribute
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Attribute::.ctor()
    IL_0006:  ret
  }

}

.class private auto ansi beforefieldinit foo
       extends [mscorlib]System.Object
{

  .method public hidebysig static void 
          abc () cil managed
  {
    .param [0]
    .custom instance void GenParAttribute::.ctor() = ( 01 00 00 00 ) 

    .entrypoint
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ret
  }
}
