// TEST #1
// 1. Assemble (with signing)
// ilasm /key=sn.snk test-sn-key.il
//
// 2. Verify public key in signed assembly
// sn -Tp test-sn-key.exe
//
// Public Key:
// 00240000048000009400000006020000002400005253413100040000010001002d02a2e377ce62
// 3f6c888a5707f1ca48f0e226fe89ff953ddb5fb1fa5ecaeee3a8cd00bc99b12dd5a7d5f532298b
// 9d9d4a59909affb006eb79b6667a99bf5268e913883e2ef7e9110b408ac2f1e10ff1c357e13ab6
// 26155bb966a513c1cddcd29ece19e7d2af1303771cb9878846ac53cb9d70b599cda6a63dd8b278
// f1909bb9
// 
// Public Key Token: 30b3f489250b41bd
//
// 3. Verify strongname signature (valid)
// sn -v test-sn-key.exe

.assembly extern mscorlib {}

.assembly hello {
}

.class private auto ansi beforefieldinit Program extends [mscorlib]System.Object {
	.method private hidebysig static void Main() cil managed {
		.entrypoint
		.maxstack  8
		ldstr      "Hello Mono!"
		call       void [mscorlib]System.Console::WriteLine(string)
		ret
	}
}
