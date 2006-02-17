//Tests global field ref to a non-existant field

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly 'err-global'
{
   .ver 0:0:0:0
}
.module 'err-global.dll'

.class private auto ansi beforefieldinit abc
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

  .method public hidebysig static void  bar() cil managed
  {
    .maxstack  8
    ldsfld int32 somefield	    
    pop
    ret
  }

}
