// See https://blogs.msdn.microsoft.com/junfeng/2005/11/19/module-initializer-a-k-a-module-constructor/
// for more information about module initializers

.assembly TestDll { }
.assembly extern mscorlib { }

.method assembly specialname rtspecialname static 
        void  .cctor() cil managed
{
	// If this method executes, we've succeeded
        ldc.i32 0;
        call void [mscorlib]System.Environment.Exit(Int32)
	ret
}

.namespace NS
{
	.class public TestClass extends [mscorlib]System.Object
	{
		.method public static void  Main() cil managed
		{
		  .entrypoint
		  // This should never run due to the module constructor
		  // exiting
		  ldc.i32 1;
		  call void [mscorlib]System.Environment.Exit(Int32)
		  ret
		}
	}
}
