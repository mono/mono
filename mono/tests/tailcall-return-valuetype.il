/*
using System;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

unsafe public struct ValueType
{
	public long a, b, c, d, e, f, g, h, i, j, k, l, m;

	public void Init ()
	{
		a = b = c = d = e = f = g = h = i = j = k = l = m = 1;
	}

	public static ValueType New ()
	{
		var a = new ValueType ();
		a.Init ();
		return a;
	}

	[MethodImpl (NoInlining)]
	static void check (long stack1, long stack2)
	{
// NOTE: This is wierd in order to be later hand edited (removed) in the IL.
		if (stack1 != 0)
			return;
		if (stack1 == stack2)
			return;
		Console.WriteLine ("tailcall failure {0} {1}", stack1, stack2);
		Environment.Exit (1);
	}

	[MethodImpl (NoInlining)]
	public static ValueType Method1 (long depth = 999999, long stack = 0)
	{
		long local;
		if (depth > 0)
			return Method2 (depth - 1, (long)&local);
		check ((long)&local, stack);
		return New ();
	}

	[MethodImpl (NoInlining)]
	static ValueType Method2 (long depth = 999999, long stack = 0)
	{
		long local;
		if (depth > 0)
			return Method1 (depth - 1, (long)&local);
		check ((long)&local, stack);
		return New ();
	}
}

class B
{
	[MethodImpl (NoInlining)]
	public static void Main (string[] args)
	{
		Console.WriteLine(ValueType.Method1 (2));
	}
}
*/

.assembly extern mscorlib { }

.assembly 'tailcall-return-valuetype' { }

.class public ValueType
extends [mscorlib]System.ValueType
{
.field public int64 a0
.field public int64 a1
.field public int64 a2
.field public int64 a3
.field public int64 a4
.field public int64 a5
.field public int64 a6
.field public int64 a7
.field public int64 a8
.field public int64 a9

.method static void check (int64 stack1, int64 stack2) noinlining
{
/*
	ldarg 0
	brfalse IL_0004
	ret

IL_0004:
*/
	ldarg 0
	ldarg 1
	bne.un IL_0009
	ret

IL_0009:
	ldstr "tailcall failure {0} {1}"
	ldarg 0
	box [mscorlib]System.Int64
	ldarg 1
	box [mscorlib]System.Int64
	call void class [mscorlib]System.Console::WriteLine (string, object, object)
	ldc.i4 1
tail.
	call void class [mscorlib]System.Environment::Exit (int32)
	ret
}

.method public static valuetype ValueType Method1 (int64 depth, int64 stack) noinlining
{
	.locals init ( int64 V_0, valuetype ValueType V_1)

	ldarg 0
	ldc.i8 0
	ble IL_0013

	ldarg 0
	ldc.i8 1
	sub
	ldloca 0
	conv.u8
tail.
	call valuetype ValueType valuetype ValueType::Method2 (int64, int64)
	ret

IL_0013:
	ldloca 0
	conv.u8
	ldarg.1
	call void valuetype ValueType::check (int64, int64)
	ldloc 1
	ret
}

.method static valuetype ValueType Method2 (int64 depth, int64 stack) noinlining
{
	.locals init ( int64 V_0, valuetype ValueType V_1)

	ldarg 0
	ldc.i8 0
	ble IL_0013

	ldarg 0
	ldc.i8 1
	sub
	ldloca 0
	conv.u8
tail.
	call valuetype ValueType valuetype ValueType::Method1 (int64, int64)
	ret

IL_0013:
	ldloca 0
	conv.u8
	ldarg 1
	call void valuetype ValueType::check(int64, int64)
	ldloc 1
	ret
}
}

.class B
{

.method public static void Main (string[] args) noinlining
{
	.entrypoint
	ldc.i8 1
	ldc.i8 0
	call valuetype ValueType valuetype ValueType::Method1 (int64, int64)
	pop
	ret
}

}
