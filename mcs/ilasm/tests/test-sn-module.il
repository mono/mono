// TEST #1
// 1. Assemble module (with signing)
// ilasm /dll /key=sn.snk test-sn-module.il
//
// 2. Verify strongname signature
// sn -v test-sn-module.dll
//
// Unable to load assembly: test-sn-module.dll

.assembly extern mscorlib {}

.class private auto ansi beforefieldinit Program extends [mscorlib]System.Object {
	.method private hidebysig static void Main() cil managed {
		.entrypoint
		.maxstack  8
		ldstr      "Hello Mono!"
		call       void [mscorlib]System.Console::WriteLine(string)
		ret
	}
}
