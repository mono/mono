// TEST #1
// 1. Assemble (with signing)
// ilasm /key=sn.snk test-sn.il
//
// 2. Verify public key in signed assembly
// sn -Tp test-sn.exe
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
// sn -v test-sn.exe


// TEST #2
// 1. Assemble (without signing - creating a delay signed assembly)
// ilasm test-sn.il
//
// 2. Verify public key in signed assembly
// sn -Tp test-sn.exe
// (see results above)
//
// 4. Verify strongname signature (not signed!)
// sn -v test-sn.exe
//
// 5. Strongname the assembly
// sn -R test-sn.exe sn.snk
//
// 6. Verify strongname signature (valid)
// sn -v test-sn.exe


.assembly extern mscorlib {}

.assembly hello {
	.publickey = (
		00 24 00 00 04 80 00 00 94 00 00 00 06 02 00 00 00 24 00 00 52 53 41 31 00 04 00 00 01 00 01 00
		2D 02 A2 E3 77 CE 62 3F 6C 88 8A 57 07 F1 CA 48 F0 E2 26 FE 89 FF 95 3D DB 5F B1 FA 5E CA EE E3
		A8 CD 00 BC 99 B1 2D D5 A7 D5 F5 32 29 8B 9D 9D 4A 59 90 9A FF B0 06 EB 79 B6 66 7A 99 BF 52 68
		E9 13 88 3E 2E F7 E9 11 0B 40 8A C2 F1 E1 0F F1 C3 57 E1 3A B6 26 15 5B B9 66 A5 13 C1 CD DC D2
		9E CE 19 E7 D2 AF 13 03 77 1C B9 87 88 46 AC 53 CB 9D 70 B5 99 CD A6 A6 3D D8 B2 78 F1 90 9B B9
	)
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
