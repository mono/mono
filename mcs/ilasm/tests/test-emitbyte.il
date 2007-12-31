/*
This test try to use .emitbyte to generate the return value,
the program should return 1 on sucess
*/
.assembly extern mscorlib
{
  .ver 2:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )
}
.assembly 'test-emitbyte'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module ld_type.exe

.class public auto ansi beforefieldinit Driver
        extends [mscorlib]System.Object
{

        .method public static int32 Main ()
        {
                .entrypoint
                .maxstack 2
                .locals init (int32 bla)
		nop
		.emitbyte 0x17 //equivalent to the ldc.i4.1 mnemonic
		ret

        }
}


