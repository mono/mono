// Test for invalid array bounds, [5...1]
// lower bound <= upper bound

.assembly extern mscorlib
{
  .ver 1:0:5000:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'err-invalid-array-bounds'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module 'err-invalid-array-bounds.exe'


  .class private auto ansi beforefieldinit test
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
           default void Main ()  cil managed 
    {
	.entrypoint
	.maxstack 0
	.locals init (
		int32[5...1]	V_0)
	IL_0000:  ret 
    }

  }
