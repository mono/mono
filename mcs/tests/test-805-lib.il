.assembly extern mscorlib
{
}

.assembly extern 'test-805'
{
  .ver 0:0:0:0
}

.assembly 'test-805-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'test-805-lib.dll'

.class public auto ansi beforefieldinit A
       extends ['test-805']C
{
  .method public hidebysig instance class ['test-805']B 
          Test() cil managed
  {
    .locals init (class ['test-805']B V_0)
    IL_0000:  newobj      instance void ['test-805']B::.ctor()
    IL_0005:  ret
  }
  
  .method public hidebysig instance void
          Test2(class ['test-805']G`1<float64> arg) cil managed
  {
    ret
  }

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    IL_0000:  ldarg.0
    IL_0001:  call       instance void ['test-805']C::.ctor()
    IL_0006:  ret
  }

}
