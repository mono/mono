/*
using System;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

struct ValueType
{
	public long a0;
	public long a1;
	public long a2;
	public long a3;
	public long a4;
	public long a5;
	public long a6;
	public long a7;
	public long a8;
	public long a9;

	[MethodImpl (NoInlining)]
	public ValueType init (long a)
	{
		a0 = a;
		a1 = a;
		a2 = a;
		a3 = a;
		a4 = a;
		a5 = a;
		a6 = a;
		a7 = a;
		a8 = a;
		a9 = a;
		return this;
	}
}

class A
{
	[MethodImpl (NoInlining)]
	static int f0 (ValueType v0, ValueType v0)
	{
		return (int)v0.a0;
	}

	[MethodImpl (NoInlining)]
	static int f1 (ValueType v0, ValueType v1)
	{
		return f0 (v0, v1); // jmp
	}

	[MethodImpl (NoInlining)]
	static void Main (string [ ] args)
	{
		var v0 = new ValueType (). init(0);
		Environment.Exit (f1 (v0, v0.init (1)));
	}
}
*/

.assembly extern mscorlib { }

.assembly 'jmp' { }

.class ValueType extends [mscorlib]System.ValueType
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

.method public instance default valuetype ValueType 'init' (int64 a) noinlining
{
	.maxstack 99
	ldarg 0
	ldarg 1
	stfld int64 ValueType::a0
	ldarg 0
	ldarg 1
	stfld int64 ValueType::a1
	ldarg 0
	ldarg 1
	stfld int64 ValueType::a2
	ldarg 0
	ldarg 1
	stfld int64 ValueType::a3
	ldarg 0
	ldarg 1
	stfld int64 ValueType::a4
	ldarg 0
	ldarg 1
	stfld int64 ValueType::a5
	ldarg 0
	ldarg 1
	stfld int64 ValueType::a6
	ldarg 0
	ldarg 1
	stfld int64 ValueType::a7
	ldarg 0
	ldarg 1
	stfld int64 ValueType::a8
	ldarg 0
	ldarg 1
	stfld int64 ValueType::a9
	ldarg 0
	ldobj ValueType
	ret
}
}

.class A extends [mscorlib]System.Object
{

.method static int32 f0 (
	int64 a0, // use up registers
	int64 a1,
	int64 a2,
	int64 a3,
	int64 a4,
	int64 a5,
	int64 a6,
	int64 a7,
	int64 a8,
	int64 a9,
	valuetype ValueType v0, valuetype ValueType v1) noinlining
{
	ldarg 10
	ldfld int64 ValueType::a0
	conv.i4
	ret
}

.method static int32 f1 (
	int64 a0, // use up registers
	int64 a1,
	int64 a2,
	int64 a3,
	int64 a4,
	int64 a5,
	int64 a6,
	int64 a7,
	int64 a8,
	int64 a9,
	valuetype ValueType v0, valuetype ValueType v1) noinlining
{
	jmp int32 class A::f0 ( int64 a0, // use up registers
				int64 a1,
				int64 a2,
				int64 a3,
				int64 a4,
				int64 a5,
				int64 a6,
				int64 a7,
				int64 a8,
				int64 a9,
				valuetype ValueType, valuetype ValueType)
}

.method static int32 Main (string[] args) noinlining
{
	.maxstack 99
	.entrypoint
	.locals init ( valuetype ValueType V_0, valuetype ValueType V_1)

	ldc.i8 2 // use up registers
	ldc.i8 2
	ldc.i8 2
	ldc.i8 2
	ldc.i8 2
	ldc.i8 2
	ldc.i8 2
	ldc.i8 2
	ldc.i8 2
	ldc.i8 2

	ldloca 0
	ldc.i8 0
	call instance valuetype ValueType valuetype ValueType::'init'(int64)

	ldloca 1
	ldc.i8 1
	call instance valuetype ValueType valuetype ValueType::'init'(int64)

	call int32 class A::f1 (int64 a0, // use up registers
				int64 a1,
				int64 a2,
				int64 a3,
				int64 a4,
				int64 a5,
				int64 a6,
				int64 a7,
				int64 a8,
				int64 a9,
				valuetype ValueType, valuetype ValueType)
	ret
}

}
