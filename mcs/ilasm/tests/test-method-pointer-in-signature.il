
// unverifiable CIL which breaks the ECMA-335 rules. 
// this CIL should fail verification by a conforming CLI verifier.

.assembly 'unverifiable_newobj_unverifiable_types_3_generated'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}

.assembly extern mscorlib
{
  .ver 1:0:5000:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}

.class ClassA extends [mscorlib]System.Object
{
	.method public hidebysig  specialname  rtspecialname instance default void .ctor ()  cil managed 
	{
		.maxstack 8
		ldarg.0 
		call instance void object::.ctor()
		ret 
	}

	.method public hidebysig  specialname  rtspecialname instance default void .ctor (method int32 *(int32) tp)  cil managed 
	{
		.maxstack 8
		ldarg.0 
		call instance void object::.ctor()
		ret 
	}

}


.method public static void Main() cil managed
{
	.entrypoint
	.maxstack 8
	ret
}
