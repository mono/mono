//Tests global generic methods

.assembly extern mscorlib
{
  .ver 2:0:0:0
}
.assembly ca
{
   .ver 0:0:0:0
}
.module ca.dll

//Global Method
  .method public hidebysig static void  foo<A,B>() cil managed
  {
    .maxstack  8
    IL_0000:  ret
  }

.class private auto ansi beforefieldinit abc`1<Z>
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
    IL_0000:  call       void foo<!Z,string>()
    IL_0005:  ret
  }

}
