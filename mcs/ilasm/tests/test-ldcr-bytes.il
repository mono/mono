.assembly extern mscorlib
{
}
.assembly 'test-ldcr-bytes'
{
}
.module ldcr8.exe

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

    .method private static  hidebysig 
           default float64 foo ()  cil managed 
    {
	.maxstack 8
	ldc.r4 (00 01 02 03)
	pop
	ldc.r8 (00 01 02 03 04 05 06 07)
	IL_0009:  ret 
    }

    .method public static  hidebysig 
           default void Main (string[] args)  cil managed 
    {
	.entrypoint
	.maxstack 8
	IL_0000:  ldstr "{0}"
	IL_0005:  call float64 class test::foo()
	IL_000a:  box [mscorlib]System.Double
	IL_000f:  call void class [mscorlib]System.Console::WriteLine(string, object)
	IL_0014:  ret 
    }

  }
