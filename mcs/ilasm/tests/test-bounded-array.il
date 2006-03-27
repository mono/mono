//Test case for #76976
//uint8[0...] should not lose its lower-bound info

.assembly extern mscorlib
{
}
.assembly Output
{
  .ver 1:0:0:0
}
.module Output

.class private auto autochar beforefieldinit Test
       extends [mscorlib]System.Object
{
  .method public hidebysig static void  Main() cil managed
  {
    .entrypoint
    .maxstack 2 
    .locals init (uint8[8] V_0,
             int32 V_1)
    ldc.i4.8
    newobj     instance void uint8[0...]::.ctor(int32)
    pop
    ret
  }

}
