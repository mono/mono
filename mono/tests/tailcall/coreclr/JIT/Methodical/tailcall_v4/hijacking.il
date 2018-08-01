// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

.assembly extern mscorlib { }
.assembly hijacking
{
}

.class private sequential ansi sealed beforefieldinit ForceHelper
       extends [mscorlib]System.ValueType
{
  .field public valuetype ForceHelper* me
  .field public int64 counter
} // end of class ForceHelper

.class private auto ansi beforefieldinit Repro
       extends [mscorlib]System.Object
{
  .field private bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) stop
  .field private bool failed
  .field private int64 counts
  .field private int64 gc
  .method private hidebysig instance void 
          TailHelper1(valuetype ForceHelper h) cil managed
  {
    // Code size       123 (0x7b)
    .maxstack  3
    IL_0000:  ldarga.s   h
    IL_0002:  dup
    IL_0003:  ldfld      int64 ForceHelper::counter
    IL_0008:  ldc.i4.1
    IL_0009:  conv.i8
    IL_000a:  add
    IL_000b:  stfld      int64 ForceHelper::counter
    IL_0010:  ldarg.0
    IL_0011:  volatile.
    IL_0013:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Repro::stop
    IL_0018:  brfalse.s  IL_002e

    IL_001a:  ldarg.0
    IL_001b:  ldflda     int64 Repro::counts
    IL_0020:  ldarga.s   h
    IL_0022:  ldfld      int64 ForceHelper::counter
    IL_0027:  call       int64 [mscorlib]System.Threading.Interlocked::Add(int64&,
                                                                           int64)
    IL_002c:  pop
    IL_002d:  ret

    IL_002e:  ldarga.s   h
    IL_0030:  conv.u
    IL_0031:  ldarga.s   h
    IL_0033:  ldfld      valuetype ForceHelper* ForceHelper::me
    IL_0038:  beq.s      IL_0073

    IL_003a:  ldarga.s   h
    IL_003c:  ldfld      int64 ForceHelper::counter
    IL_0041:  ldc.i4.1
    IL_0042:  conv.i8
    IL_0043:  ble.s      IL_0069

    IL_0045:  ldarg.0
    IL_0046:  ldc.i4.1
    IL_0047:  stfld      bool Repro::failed
    IL_004c:  ldarg.0
    IL_004d:  ldc.i4.1
    IL_004e:  volatile.
    IL_0050:  stfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Repro::stop
    IL_0055:  ldarg.0
    IL_0056:  ldflda     int64 Repro::counts
    IL_005b:  ldarga.s   h
    IL_005d:  ldfld      int64 ForceHelper::counter
    IL_0062:  call       int64 [mscorlib]System.Threading.Interlocked::Add(int64&,
                                                                           int64)
    IL_0067:  pop
    IL_0068:  ret

    IL_0069:  ldarga.s   h
    IL_006b:  ldarga.s   h
    IL_006d:  conv.u
    IL_006e:  stfld      valuetype ForceHelper* ForceHelper::me
    IL_0073:  ldarg.0
    IL_0074:  ldarg.1
    IL_0075:  tail. call       instance void Repro::TailHelper2(valuetype ForceHelper)
    IL_007a:  ret
  } // end of method Repro::TailHelper1

  .method private hidebysig instance void 
          TailHelper2(valuetype ForceHelper h) cil managed
  {
    // Code size       123 (0x7b)
    .maxstack  3
    IL_0000:  ldarga.s   h
    IL_0002:  dup
    IL_0003:  ldfld      int64 ForceHelper::counter
    IL_0008:  ldc.i4.1
    IL_0009:  conv.i8
    IL_000a:  add
    IL_000b:  stfld      int64 ForceHelper::counter
    IL_0010:  ldarg.0
    IL_0011:  volatile.
    IL_0013:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Repro::stop
    IL_0018:  brfalse.s  IL_002e

    IL_001a:  ldarg.0
    IL_001b:  ldflda     int64 Repro::counts
    IL_0020:  ldarga.s   h
    IL_0022:  ldfld      int64 ForceHelper::counter
    IL_0027:  call       int64 [mscorlib]System.Threading.Interlocked::Add(int64&,
                                                                           int64)
    IL_002c:  pop
    IL_002d:  ret

    IL_002e:  ldarga.s   h
    IL_0030:  conv.u
    IL_0031:  ldarga.s   h
    IL_0033:  ldfld      valuetype ForceHelper* ForceHelper::me
    IL_0038:  beq.s      IL_0073

    IL_003a:  ldarga.s   h
    IL_003c:  ldfld      int64 ForceHelper::counter
    IL_0041:  ldc.i4.1
    IL_0042:  conv.i8
    IL_0043:  ble.s      IL_0069

    IL_0045:  ldarg.0
    IL_0046:  ldc.i4.1
    IL_0047:  stfld      bool Repro::failed
    IL_004c:  ldarg.0
    IL_004d:  ldc.i4.1
    IL_004e:  volatile.
    IL_0050:  stfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Repro::stop
    IL_0055:  ldarg.0
    IL_0056:  ldflda     int64 Repro::counts
    IL_005b:  ldarga.s   h
    IL_005d:  ldfld      int64 ForceHelper::counter
    IL_0062:  call       int64 [mscorlib]System.Threading.Interlocked::Add(int64&,
                                                                           int64)
    IL_0067:  pop
    IL_0068:  ret

    IL_0069:  ldarga.s   h
    IL_006b:  ldarga.s   h
    IL_006d:  conv.u
    IL_006e:  stfld      valuetype ForceHelper* ForceHelper::me
    IL_0073:  ldarg.0
    IL_0074:  ldarg.1
    IL_0075:  tail. call       instance void Repro::TailHelper1(valuetype ForceHelper)
    IL_007a:  ret
  } // end of method Repro::TailHelper2

  .method private hidebysig instance void 
          TailCallThread() cil managed
  {
    // Code size       16 (0x10)
    .maxstack  2
    .locals init (valuetype ForceHelper V_0)
    IL_0000:  ldloca.s   V_0
    IL_0002:  initobj    ForceHelper
    IL_0008:  ldarg.0
    IL_0009:  ldloc.0
    IL_000a:  tail. call       instance void Repro::TailHelper1(valuetype ForceHelper)
    IL_000f:  ret
  } // end of method Repro::TailCallThread

  .method private hidebysig instance void 
          Collector() cil managed
  {
    // Code size       33 (0x21)
    .maxstack  8
    IL_0000:  br.s       IL_0016

    IL_0002:  call       void [mscorlib]System.GC::Collect()
    IL_0007:  ldarg.0
    IL_0008:  dup
    IL_0009:  ldfld      int64 Repro::gc
    IL_000e:  ldc.i4.1
    IL_000f:  conv.i8
    IL_0010:  add
    IL_0011:  stfld      int64 Repro::gc
    IL_0016:  ldarg.0
    IL_0017:  volatile.
    IL_0019:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Repro::stop
    IL_001e:  brfalse.s  IL_0002

    IL_0020:  ret
  } // end of method Repro::Collector

  .method private hidebysig static int32 
          Main() cil managed
  {
    .entrypoint
    // Code size       300 (0x12c)
    .maxstack  5
    .locals init (class Repro V_0,
             class [mscorlib]System.Threading.Thread[] V_1,
             class [mscorlib]System.Threading.Thread V_2,
             int32 V_3,
             class [mscorlib]System.Threading.Thread V_4,
             class [mscorlib]System.Threading.Thread[] V_5,
             class [mscorlib]System.Threading.Thread[] V_6,
             int32 V_7,
             class [mscorlib]System.Threading.Thread[] V_8,
             int32 V_9)
    IL_0000:  newobj     instance void Repro::.ctor()
    IL_0005:  stloc.0
    IL_0006:  ldc.i4.4
    IL_0007:  newarr     [mscorlib]System.Threading.Thread
    IL_000c:  stloc.s    V_5
    IL_000e:  ldloc.s    V_5
    IL_0010:  ldc.i4.0
    IL_0011:  ldloc.0
    IL_0012:  ldftn      instance void Repro::Collector()
    IL_0018:  newobj     instance void [mscorlib]System.Threading.ThreadStart::.ctor(object,
                                                                                     native int)
    IL_001d:  newobj     instance void [mscorlib]System.Threading.Thread::.ctor(class [mscorlib]System.Threading.ThreadStart)
    IL_0022:  stelem.ref
    IL_0023:  ldloc.s    V_5
    IL_0025:  ldc.i4.1
    IL_0026:  ldloc.0
    IL_0027:  ldftn      instance void Repro::TailCallThread()
    IL_002d:  newobj     instance void [mscorlib]System.Threading.ThreadStart::.ctor(object,
                                                                                     native int)
    IL_0032:  newobj     instance void [mscorlib]System.Threading.Thread::.ctor(class [mscorlib]System.Threading.ThreadStart)
    IL_0037:  stelem.ref
    IL_0038:  ldloc.s    V_5
    IL_003a:  ldc.i4.2
    IL_003b:  ldloc.0
    IL_003c:  ldftn      instance void Repro::TailCallThread()
    IL_0042:  newobj     instance void [mscorlib]System.Threading.ThreadStart::.ctor(object,
                                                                                     native int)
    IL_0047:  newobj     instance void [mscorlib]System.Threading.Thread::.ctor(class [mscorlib]System.Threading.ThreadStart)
    IL_004c:  stelem.ref
    IL_004d:  ldloc.s    V_5
    IL_004f:  ldc.i4.3
    IL_0050:  ldloc.0
    IL_0051:  ldftn      instance void Repro::TailCallThread()
    IL_0057:  newobj     instance void [mscorlib]System.Threading.ThreadStart::.ctor(object,
                                                                                     native int)
    IL_005c:  newobj     instance void [mscorlib]System.Threading.Thread::.ctor(class [mscorlib]System.Threading.ThreadStart)
    IL_0061:  stelem.ref
    IL_0062:  ldloc.s    V_5
    IL_0064:  stloc.1
    IL_0065:  ldloc.1
    IL_0066:  stloc.s    V_6
    IL_0068:  ldc.i4.0
    IL_0069:  stloc.s    V_7
    IL_006b:  br.s       IL_007f

    IL_006d:  ldloc.s    V_6
    IL_006f:  ldloc.s    V_7
    IL_0071:  ldelem.ref
    IL_0072:  stloc.2
    IL_0073:  ldloc.2
    IL_0074:  callvirt   instance void [mscorlib]System.Threading.Thread::Start()
    IL_0079:  ldloc.s    V_7
    IL_007b:  ldc.i4.1
    IL_007c:  add
    IL_007d:  stloc.s    V_7
    IL_007f:  ldloc.s    V_7
    IL_0081:  ldloc.s    V_6
    IL_0083:  ldlen
    IL_0084:  conv.i4
    IL_0085:  blt.s      IL_006d

    IL_0087:  ldc.i4.0
    IL_0088:  stloc.3
    IL_0089:  br.s       IL_00a3

    IL_008b:  ldc.i4     0x3e8
    IL_0090:  call       void [mscorlib]System.Threading.Thread::Sleep(int32)
    IL_0095:  ldloc.0
    IL_0096:  volatile.
    IL_0098:  ldfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Repro::stop
    IL_009d:  brtrue.s   IL_00a8

    IL_009f:  ldloc.3
    IL_00a0:  ldc.i4.1
    IL_00a1:  add
    IL_00a2:  stloc.3
    IL_00a3:  ldloc.3
    IL_00a4:  ldc.i4.s   60
    IL_00a6:  blt.s      IL_008b

    IL_00a8:  ldloc.0
    IL_00a9:  ldc.i4.1
    IL_00aa:  volatile.
    IL_00ac:  stfld      bool modreq([mscorlib]System.Runtime.CompilerServices.IsVolatile) Repro::stop
    IL_00b1:  ldloc.1
    IL_00b2:  stloc.s    V_8
    IL_00b4:  ldc.i4.0
    IL_00b5:  stloc.s    V_9
    IL_00b7:  br.s       IL_00cd

    IL_00b9:  ldloc.s    V_8
    IL_00bb:  ldloc.s    V_9
    IL_00bd:  ldelem.ref
    IL_00be:  stloc.s    V_4
    IL_00c0:  ldloc.s    V_4
    IL_00c2:  callvirt   instance void [mscorlib]System.Threading.Thread::Join()
    IL_00c7:  ldloc.s    V_9
    IL_00c9:  ldc.i4.1
    IL_00ca:  add
    IL_00cb:  stloc.s    V_9
    IL_00cd:  ldloc.s    V_9
    IL_00cf:  ldloc.s    V_8
    IL_00d1:  ldlen
    IL_00d2:  conv.i4
    IL_00d3:  blt.s      IL_00b9

    IL_00d5:  ldstr      "Executed {0} GCs"
    IL_00da:  ldloc.0
    IL_00db:  ldfld      int64 Repro::gc
    IL_00e0:  box        [mscorlib]System.Int64
    IL_00e5:  call       void [mscorlib]System.Console::WriteLine(string,
                                                                  object)
    IL_00ea:  ldloc.0
    IL_00eb:  ldfld      bool Repro::failed
    IL_00f0:  brfalse.s  IL_010a

    IL_00f2:  ldstr      "Executed {0} tail calls before failing"
    IL_00f7:  ldloc.0
    IL_00f8:  ldfld      int64 Repro::counts
    IL_00fd:  box        [mscorlib]System.Int64
    IL_0102:  call       void [mscorlib]System.Console::WriteLine(string,
                                                                  object)
    IL_0107:  ldc.i4.s   101
    IL_0109:  ret

    IL_010a:  ldstr      "Executed {0} tail calls"
    IL_010f:  ldloc.0
    IL_0110:  ldfld      int64 Repro::counts
    IL_0115:  box        [mscorlib]System.Int64
    IL_011a:  call       void [mscorlib]System.Console::WriteLine(string,
                                                                  object)
    IL_011f:  ldstr      "PASS"
    IL_0124:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_0129:  ldc.i4.s   100

// convert success:100 to success:0
	dup
	ldc.i4 100
	bne.un exit
	pop
	ldc.i4 0
exit:

    IL_012b:  ret
  } // end of method Repro::Main

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  } // end of method Repro::.ctor

} // end of class Repro
