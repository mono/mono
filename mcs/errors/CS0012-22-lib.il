.assembly extern mscorlib
{
}

.assembly extern System.Core
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-22-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-22-lib.dll'


.class public abstract auto ansi sealed beforefieldinit B
       extends [mscorlib]System.Object
{
  .custom instance void [System.Core]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 

  .method public hidebysig static void
        Sum(class [mscorlib]System.Collections.Generic.IEnumerable`1<float64> a) cil managed
  {
    ret
  }

  .method public hidebysig static void
        Sum(class ['CS0012-lib-missing']Base.IQueryExpr`1<class [mscorlib]System.Collections.Generic.IEnumerable`1<float64>> a) cil managed
  {
    .custom instance void [System.Core]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = ( 01 00 00 00 ) 

    ret
  }
}
