.assembly extern mscorlib { }
.assembly HelloWorld { }
.module HelloWorld.exe

.class public auto ansi Hello extends [mscorlib]System.Object {
	
	.method public static void check( ) cil managed {
		.entrypoint
		.maxstack 1	
	
		ldstr "Hello World."
		call void [corlib]System.Console::WriteLine(string)

	
		ret
	}		
}

