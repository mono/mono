// Test for accessing undeclared identifier (args here)

.assembly extern mscorlib
{
  .ver 1:0:5000:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )
}
.assembly 'arg'
{
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module arg.exe

  .class public auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
    .method public static  hidebysig 
           default void Main ()  cil managed 
    {
	.entrypoint
	.maxstack 8
	ldarg args
	pop
	IL_0012:  ret 
    }

  }
