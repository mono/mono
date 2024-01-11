// Test using calli with native methods and ensure that it respects calling convention

.assembly extern mscorlib{}
.assembly calli_native{}

.method public static pinvokeimpl ("libtest" cdecl) vararg int32 mono_test_vararg_cdecl(int32) cil managed preservesig {}

.method public static int32 test_0_calli_native () cil managed
{
	.entrypoint
	.locals init (void *V_0)

	ldc.i4 -1
	call vararg int32 mono_test_vararg_cdecl(int32)
	ldc.i4 -1
	ceq
	brfalse fail

	ldc.i4 0
	ldc.i4 12
	ldc.i4 10
	call vararg int32 mono_test_vararg_cdecl(int32, ..., int32, int32)
	ldc.i4 12
	ceq
	brfalse fail

	ldc.i4 1
	ldc.i4 12
	ldc.i4 10
	call vararg int32 mono_test_vararg_cdecl(int32, ..., int32, int32)
	ldc.i4 10
	ceq
	brfalse fail

	ldstr "PASS"
	call void [mscorlib]System.Console::WriteLine(string)
	ldc.i4.0
	ret

fail:
	ldstr "FAIL"
	call void [mscorlib]System.Console::WriteLine(string)
	ldc.i4.1
	ret
}
