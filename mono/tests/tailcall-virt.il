/*
Test tail.callvirt.

Author:
    Jay Krell (jaykrell@microsoft.com)

Copyright 2018 Microsoft
Licensed under the MIT license. See LICENSE file in the project root for full license information.

This IL will exit with 0 for success, 1 for failure.
It is based on nearby C# that is based on nearby F#.

This test covers tail.callvirt.
Incidentally it is a mutual recursion, not a self-recursion,
no generics, no pointers, small function signature, no value types, etc.
*/

.assembly extern mscorlib { }

.assembly b { }

.class A
{
.method static int32
check(int64 stack1,
int64 stack2) 
{
ldarg.0
ldarg.1
beq.s IL_0006
ldc.i4.1
ret

IL_0006: ldc.i4.0
ret
}

.method newslot virtual instance int32 f1(int32 counter, int64 initial_stack, int64 current_stack) noinlining
{
.locals init (int32 V_0)
ldarg.1
ldc.i4.0
ble.s IL_0013
ldarg.0
ldarg.1
ldc.i4.1
sub
ldarg.2
ldloca.s V_0
conv.u
conv.u8
tail. callvirt instance int32 A::f2(int32, int64, int64)
ret

IL_0013: ldloca.s V_0
conv.u
conv.u8
ldarg.3
tail. call int32 A::check(int64, int64)
ret
}

.method newslot virtual instance int32 f2(int32 counter, int64 initial_stack, int64 current_stack) noinlining
{
.locals init (int32 V_0)
ldarg.1
ldc.i4.0
ble.s IL_0013
ldarg.0
ldarg.1
ldc.i4.1
sub
ldarg.2
ldloca.s V_0
conv.u
conv.u8
tail. callvirt instance int32 A::f1(int32, int64, int64)
ret

IL_0013: ldloca.s V_0
conv.u
conv.u8
ldarg.3
tail. call int32 A::check(int64, int64)
ret
}

.method static void Main() noinlining
{
.entrypoint
.locals init (int32 V_0)
newobj instance void A::.ctor()
ldc.i4 0x100
ldloca.s V_0
conv.u
conv.u8
ldc.i4.0
conv.i8
callvirt instance int32 A::f1(int32, int64, int64)
call void [mscorlib]System.Environment::Exit(int32)
ret
}

.method instance void .ctor() 
{
ldarg.0
tail. call instance void [mscorlib]System.Object::.ctor()
ret
}

}
