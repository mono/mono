/*
This test is derived from nearby fsharp.fs.
	Comment out the two failing tests.
	fsharpc --standalone --optimize- fsharp.fs
	monolinker fsharp.exe to trim a little
	change resource-using code to use constant strings
*/

.assembly extern mscorlib { }
.assembly extern System.Numerics { }
.assembly extern System { }
.assembly extern System.Core { }
.assembly fsharp { }

.class private abstract auto ansi sealed '<StartupCode$fsharp>'.$Fsharp
       extends [mscorlib]System.Object
{
  .method public static void  main@() cil managed
  {
    .entrypoint
    // Code size       2397 (0x95d)
    .maxstack  5
    IL_0000:  ldstr      "simpleTail1"
    IL_0005:  call       int32 Fsharp::get_HugeInt()
    IL_000a:  ldc.i4.0
    IL_000b:  call       int32 Fsharp::simpleTail1(int32,
                                                   int32)
    IL_0010:  call       int32 Fsharp::get_HugeInt()
    IL_0015:  call       void Fsharp::RunTest<int32>(string,
                                                     !!0,
                                                     !!0)
    IL_001a:  ldstr      "simpleTail2"
    IL_001f:  call       int32 Fsharp::get_HugeInt()
    IL_0024:  ldc.i4.0
    IL_0025:  call       int32 Fsharp::simpleTail2(int32,
                                                   int32)
    IL_002a:  call       int32 Fsharp::get_HugeInt()
    IL_002f:  call       void Fsharp::RunTest<int32>(string,
                                                     !!0,
                                                     !!0)
    IL_0034:  ldstr      "mutualTail1IsOdd"
    IL_0039:  call       int32 Fsharp::get_HugeInt()
    IL_003e:  call       bool Fsharp::mutualTail1IsOdd(int32)
    IL_0043:  ldc.i4.0
    IL_0044:  call       void Fsharp::RunTest<bool>(string,
                                                    !!0,
                                                    !!0)
    IL_0049:  ldstr      "mutualTail1IsEven"
    IL_004e:  call       int32 Fsharp::get_HugeInt()
    IL_0053:  call       bool Fsharp::mutualTail1IsEven(int32)
    IL_0058:  ldc.i4.1
    IL_0059:  call       void Fsharp::RunTest<bool>(string,
                                                    !!0,
                                                    !!0)
    IL_005e:  ldstr      "TailCallLoop<int>"
    IL_0063:  newobj     instance void class Fsharp/TailCallLoop`1<int32>::.ctor()
    IL_0068:  callvirt   instance int32 class Fsharp/TailCallLoop`1<int32>::get_Result()
    IL_006d:  ldc.i4.2
    IL_006e:  call       void Fsharp::RunTest<int32>(string,
                                                     !!0,
                                                     !!0)
    IL_0073:  ldstr      "TailCallLoop<DateTime>"
    IL_0078:  newobj     instance void class Fsharp/TailCallLoop`1<valuetype [mscorlib]System.DateTime>::.ctor()
    IL_007d:  callvirt   instance int32 class Fsharp/TailCallLoop`1<valuetype [mscorlib]System.DateTime>::get_Result()
    IL_0082:  ldc.i4.2
    IL_0083:  call       void Fsharp::RunTest<int32>(string,
                                                     !!0,
                                                     !!0)
    IL_0088:  ldstr      "TailCallLoopGenericClassAndMethod<byte>.Method1<by"
    + "te>"
    IL_008d:  ldc.i4.3
    IL_008e:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<uint8>::.ctor(!0)
    IL_0093:  ldc.i4     0x989680
    IL_0098:  ldc.i4.4
    IL_0099:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<uint8>::Method1<uint8>(int32,
                                                                                                                                                 !!0)
    IL_009e:  ldc.i4.3
    IL_009f:  ldc.i4.4
    IL_00a0:  newobj     instance void class [mscorlib]System.Tuple`2<uint8,uint8>::.ctor(!0,
                                                                                          !1)
    IL_00a5:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<uint8,uint8>>(string,
                                                                                           !!0,
                                                                                           !!0)
    IL_00aa:  ldstr      "TailCallLoopGenericClassAndMethod<byte>.Method1<int>"
    IL_00af:  ldc.i4.3
    IL_00b0:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<uint8>::.ctor(!0)
    IL_00b5:  ldc.i4     0x989680
    IL_00ba:  ldc.i4.4
    IL_00bb:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<uint8>::Method1<int32>(int32,
                                                                                                                                                 !!0)
    IL_00c0:  ldc.i4.3
    IL_00c1:  ldc.i4.4
    IL_00c2:  newobj     instance void class [mscorlib]System.Tuple`2<uint8,int32>::.ctor(!0,
                                                                                          !1)
    IL_00c7:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<uint8,int32>>(string,
                                                                                           !!0,
                                                                                           !!0)
    IL_00cc:  ldstr      "TailCallLoopGenericClassAndMethod<byte>.Method1<Da"
    + "teTime>"
    IL_00d1:  ldc.i4.3
    IL_00d2:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<uint8>::.ctor(!0)
    IL_00d7:  ldc.i4     0x989680
    IL_00dc:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_00e1:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<uint8>::Method1<valuetype [mscorlib]System.DateTime>(int32,
                                                                                                                                                                               !!0)
    IL_00e6:  ldc.i4.3
    IL_00e7:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_00ec:  newobj     instance void class [mscorlib]System.Tuple`2<uint8,valuetype [mscorlib]System.DateTime>::.ctor(!0,
                                                                                                                        !1)
    IL_00f1:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<uint8,valuetype [mscorlib]System.DateTime>>(string,
                                                                                                                         !!0,
                                                                                                                         !!0)
    IL_00f6:  ldstr      "TailCallLoopGenericClassAndMethod<byte>.Method1<st"
    + "ring>"
    IL_00fb:  ldc.i4.3
    IL_00fc:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<uint8>::.ctor(!0)
    IL_0101:  ldc.i4     0x989680
    IL_0106:  ldstr      "abc"
    IL_010b:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<uint8>::Method1<string>(int32,
                                                                                                                                                  !!0)
    IL_0110:  ldc.i4.3
    IL_0111:  ldstr      "abc"
    IL_0116:  newobj     instance void class [mscorlib]System.Tuple`2<uint8,string>::.ctor(!0,
                                                                                           !1)
    IL_011b:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<uint8,string>>(string,
                                                                                            !!0,
                                                                                            !!0)
    IL_0120:  ldstr      "TailCallLoopGenericClassAndMethod<int>.Method1<byte>"
    IL_0125:  ldc.i4.3
    IL_0126:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<int32>::.ctor(!0)
    IL_012b:  ldc.i4     0x989680
    IL_0130:  ldc.i4.4
    IL_0131:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<int32>::Method1<uint8>(int32,
                                                                                                                                                 !!0)
    IL_0136:  ldc.i4.3
    IL_0137:  ldc.i4.4
    IL_0138:  newobj     instance void class [mscorlib]System.Tuple`2<int32,uint8>::.ctor(!0,
                                                                                          !1)
    IL_013d:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<int32,uint8>>(string,
                                                                                           !!0,
                                                                                           !!0)
    IL_0142:  ldstr      "TailCallLoopGenericClassAndMethod<int>.Method1<int>"
    IL_0147:  ldc.i4.3
    IL_0148:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<int32>::.ctor(!0)
    IL_014d:  ldc.i4     0x989680
    IL_0152:  ldc.i4.4
    IL_0153:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<int32>::Method1<int32>(int32,
                                                                                                                                                 !!0)
    IL_0158:  ldc.i4.3
    IL_0159:  ldc.i4.4
    IL_015a:  newobj     instance void class [mscorlib]System.Tuple`2<int32,int32>::.ctor(!0,
                                                                                          !1)
    IL_015f:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<int32,int32>>(string,
                                                                                           !!0,
                                                                                           !!0)
    IL_0164:  ldstr      "TailCallLoopGenericClassAndMethod<int>.Method1<Dat"
    + "eTime>"
    IL_0169:  ldc.i4.3
    IL_016a:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<int32>::.ctor(!0)
    IL_016f:  ldc.i4     0x989680
    IL_0174:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_0179:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<int32>::Method1<valuetype [mscorlib]System.DateTime>(int32,
                                                                                                                                                                               !!0)
    IL_017e:  ldc.i4.3
    IL_017f:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_0184:  newobj     instance void class [mscorlib]System.Tuple`2<int32,valuetype [mscorlib]System.DateTime>::.ctor(!0,
                                                                                                                        !1)
    IL_0189:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<int32,valuetype [mscorlib]System.DateTime>>(string,
                                                                                                                         !!0,
                                                                                                                         !!0)
    IL_018e:  ldstr      "TailCallLoopGenericClassAndMethod<int>.Method1<str"
    + "ing>"
    IL_0193:  ldc.i4.3
    IL_0194:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<int32>::.ctor(!0)
    IL_0199:  ldc.i4     0x989680
    IL_019e:  ldstr      "abc"
    IL_01a3:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<int32>::Method1<string>(int32,
                                                                                                                                                  !!0)
    IL_01a8:  ldc.i4.3
    IL_01a9:  ldstr      "abc"
    IL_01ae:  newobj     instance void class [mscorlib]System.Tuple`2<int32,string>::.ctor(!0,
                                                                                           !1)
    IL_01b3:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<int32,string>>(string,
                                                                                            !!0,
                                                                                            !!0)
    IL_01b8:  ldstr      "TailCallLoopGenericClassAndMethod<DateTime>.Method"
    + "1<byte>"
    IL_01bd:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_01c2:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<valuetype [mscorlib]System.DateTime>::.ctor(!0)
    IL_01c7:  ldc.i4     0x989680
    IL_01cc:  ldc.i4.4
    IL_01cd:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<valuetype [mscorlib]System.DateTime>::Method1<uint8>(int32,
                                                                                                                                                                               !!0)
    IL_01d2:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_01d7:  ldc.i4.4
    IL_01d8:  newobj     instance void class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,uint8>::.ctor(!0,
                                                                                                                        !1)
    IL_01dd:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,uint8>>(string,
                                                                                                                         !!0,
                                                                                                                         !!0)
    IL_01e2:  ldstr      "TailCallLoopGenericClassAndMethod<DateTime>.Method"
    + "1<int>"
    IL_01e7:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_01ec:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<valuetype [mscorlib]System.DateTime>::.ctor(!0)
    IL_01f1:  ldc.i4     0x989680
    IL_01f6:  ldc.i4.4
    IL_01f7:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<valuetype [mscorlib]System.DateTime>::Method1<int32>(int32,
                                                                                                                                                                               !!0)
    IL_01fc:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_0201:  ldc.i4.4
    IL_0202:  newobj     instance void class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,int32>::.ctor(!0,
                                                                                                                        !1)
    IL_0207:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,int32>>(string,
                                                                                                                         !!0,
                                                                                                                         !!0)
    IL_020c:  ldstr      "TailCallLoopGenericClassAndMethod<DateTime>.Method"
    + "1<DateTime>"
    IL_0211:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_0216:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<valuetype [mscorlib]System.DateTime>::.ctor(!0)
    IL_021b:  ldc.i4     0x989680
    IL_0220:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_0225:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<valuetype [mscorlib]System.DateTime>::Method1<valuetype [mscorlib]System.DateTime>(int32,
                                                                                                                                                                                                             !!0)
    IL_022a:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_022f:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_0234:  newobj     instance void class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,valuetype [mscorlib]System.DateTime>::.ctor(!0,
                                                                                                                                                      !1)
    IL_0239:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,valuetype [mscorlib]System.DateTime>>(string,
                                                                                                                                                       !!0,
                                                                                                                                                       !!0)
    IL_023e:  ldstr      "TailCallLoopGenericClassAndMethod<DateTime>.Method"
    + "1<string>"
    IL_0243:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_0248:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<valuetype [mscorlib]System.DateTime>::.ctor(!0)
    IL_024d:  ldc.i4     0x989680
    IL_0252:  ldstr      "abc"
    IL_0257:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<valuetype [mscorlib]System.DateTime>::Method1<string>(int32,
                                                                                                                                                                                !!0)
    IL_025c:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_0261:  ldstr      "abc"
    IL_0266:  newobj     instance void class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,string>::.ctor(!0,
                                                                                                                         !1)
    IL_026b:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,string>>(string,
                                                                                                                          !!0,
                                                                                                                          !!0)
    IL_0270:  ldstr      "TailCallLoopGenericClassAndMethod<string>.Method1<"
    + "byte>"
    IL_0275:  ldstr      "qq"
    IL_027a:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<string>::.ctor(!0)
    IL_027f:  ldc.i4     0x989680
    IL_0284:  ldc.i4.4
    IL_0285:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<string>::Method1<uint8>(int32,
                                                                                                                                                  !!0)
    IL_028a:  ldstr      "qq"
    IL_028f:  ldc.i4.4
    IL_0290:  newobj     instance void class [mscorlib]System.Tuple`2<string,uint8>::.ctor(!0,
                                                                                           !1)
    IL_0295:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<string,uint8>>(string,
                                                                                            !!0,
                                                                                            !!0)
    IL_029a:  ldstr      "TailCallLoopGenericClassAndMethod<string>.Method1<"
    + "int>"
    IL_029f:  ldstr      "qq"
    IL_02a4:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<string>::.ctor(!0)
    IL_02a9:  ldc.i4     0x989680
    IL_02ae:  ldc.i4.4
    IL_02af:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<string>::Method1<int32>(int32,
                                                                                                                                                  !!0)
    IL_02b4:  ldstr      "qq"
    IL_02b9:  ldc.i4.4
    IL_02ba:  newobj     instance void class [mscorlib]System.Tuple`2<string,int32>::.ctor(!0,
                                                                                           !1)
    IL_02bf:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<string,int32>>(string,
                                                                                            !!0,
                                                                                            !!0)
    IL_02c4:  ldstr      "TailCallLoopGenericClassAndMethod<string>.Method1<"
    + "DateTime>"
    IL_02c9:  ldstr      "qq"
    IL_02ce:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<string>::.ctor(!0)
    IL_02d3:  ldc.i4     0x989680
    IL_02d8:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_02dd:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<string>::Method1<valuetype [mscorlib]System.DateTime>(int32,
                                                                                                                                                                                !!0)
    IL_02e2:  ldstr      "qq"
    IL_02e7:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_02ec:  newobj     instance void class [mscorlib]System.Tuple`2<string,valuetype [mscorlib]System.DateTime>::.ctor(!0,
                                                                                                                         !1)
    IL_02f1:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<string,valuetype [mscorlib]System.DateTime>>(string,
                                                                                                                          !!0,
                                                                                                                          !!0)
    IL_02f6:  ldstr      "TailCallLoopGenericClassAndMethod<string>.Method1<"
    + "string>"
    IL_02fb:  ldstr      "qq"
    IL_0300:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethod`1<string>::.ctor(!0)
    IL_0305:  ldc.i4     0x989680
    IL_030a:  ldstr      "abc"
    IL_030f:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<string>::Method1<string>(int32,
                                                                                                                                                   !!0)
    IL_0314:  ldstr      "qq"
    IL_0319:  ldstr      "abc"
    IL_031e:  newobj     instance void class [mscorlib]System.Tuple`2<string,string>::.ctor(!0,
                                                                                            !1)
    IL_0323:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<string,string>>(string,
                                                                                             !!0,
                                                                                             !!0)
    IL_0328:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<byt"
    + "e>.Method1<byte>"
    IL_032d:  ldc.i4.3
    IL_032e:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<uint8>::.ctor(!0)
    IL_0333:  ldc.i4     0x989680
    IL_0338:  ldc.i4.4
    IL_0339:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<uint8>::Method1<uint8>(int32,
                                                                                                                                                         !!0)
    IL_033e:  ldc.i4.3
    IL_033f:  ldc.i4.4
    IL_0340:  newobj     instance void class [mscorlib]System.Tuple`2<uint8,uint8>::.ctor(!0,
                                                                                          !1)
    IL_0345:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<uint8,uint8>>(string,
                                                                                           !!0,
                                                                                           !!0)
    IL_034a:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<byt"
    + "e>.Method1<int>"
    IL_034f:  ldc.i4.3
    IL_0350:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<uint8>::.ctor(!0)
    IL_0355:  ldc.i4     0x989680
    IL_035a:  ldc.i4.4
    IL_035b:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<uint8>::Method1<int32>(int32,
                                                                                                                                                         !!0)
    IL_0360:  ldc.i4.3
    IL_0361:  ldc.i4.4
    IL_0362:  newobj     instance void class [mscorlib]System.Tuple`2<uint8,int32>::.ctor(!0,
                                                                                          !1)
    IL_0367:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<uint8,int32>>(string,
                                                                                           !!0,
                                                                                           !!0)
    IL_036c:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<byt"
    + "e>.Method1<DateTime>"
    IL_0371:  ldc.i4.3
    IL_0372:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<uint8>::.ctor(!0)
    IL_0377:  ldc.i4     0x989680
    IL_037c:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_0381:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<uint8>::Method1<valuetype [mscorlib]System.DateTime>(int32,
                                                                                                                                                                                       !!0)
    IL_0386:  ldc.i4.3
    IL_0387:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_038c:  newobj     instance void class [mscorlib]System.Tuple`2<uint8,valuetype [mscorlib]System.DateTime>::.ctor(!0,
                                                                                                                        !1)
    IL_0391:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<uint8,valuetype [mscorlib]System.DateTime>>(string,
                                                                                                                         !!0,
                                                                                                                         !!0)
    IL_0396:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<byt"
    + "e>.Method1<string>"
    IL_039b:  ldc.i4.3
    IL_039c:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<uint8>::.ctor(!0)
    IL_03a1:  ldc.i4     0x989680
    IL_03a6:  ldstr      "abc"
    IL_03ab:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<uint8>::Method1<string>(int32,
                                                                                                                                                          !!0)
    IL_03b0:  ldc.i4.3
    IL_03b1:  ldstr      "abc"
    IL_03b6:  newobj     instance void class [mscorlib]System.Tuple`2<uint8,string>::.ctor(!0,
                                                                                           !1)
    IL_03bb:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<uint8,string>>(string,
                                                                                            !!0,
                                                                                            !!0)
    IL_03c0:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<int"
    + ">.Method1<byte>"
    IL_03c5:  ldc.i4.3
    IL_03c6:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<int32>::.ctor(!0)
    IL_03cb:  ldc.i4     0x989680
    IL_03d0:  ldc.i4.4
    IL_03d1:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<int32>::Method1<uint8>(int32,
                                                                                                                                                         !!0)
    IL_03d6:  ldc.i4.3
    IL_03d7:  ldc.i4.4
    IL_03d8:  newobj     instance void class [mscorlib]System.Tuple`2<int32,uint8>::.ctor(!0,
                                                                                          !1)
    IL_03dd:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<int32,uint8>>(string,
                                                                                           !!0,
                                                                                           !!0)
    IL_03e2:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<int"
    + ">.Method1<int>"
    IL_03e7:  ldc.i4.3
    IL_03e8:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<int32>::.ctor(!0)
    IL_03ed:  ldc.i4     0x989680
    IL_03f2:  ldc.i4.4
    IL_03f3:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<int32>::Method1<int32>(int32,
                                                                                                                                                         !!0)
    IL_03f8:  ldc.i4.3
    IL_03f9:  ldc.i4.4
    IL_03fa:  newobj     instance void class [mscorlib]System.Tuple`2<int32,int32>::.ctor(!0,
                                                                                          !1)
    IL_03ff:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<int32,int32>>(string,
                                                                                           !!0,
                                                                                           !!0)
    IL_0404:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<int"
    + ">.Method1<DateTime>"
    IL_0409:  ldc.i4.3
    IL_040a:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<int32>::.ctor(!0)
    IL_040f:  ldc.i4     0x989680
    IL_0414:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_0419:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<int32>::Method1<valuetype [mscorlib]System.DateTime>(int32,
                                                                                                                                                                                       !!0)
    IL_041e:  ldc.i4.3
    IL_041f:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_0424:  newobj     instance void class [mscorlib]System.Tuple`2<int32,valuetype [mscorlib]System.DateTime>::.ctor(!0,
                                                                                                                        !1)
    IL_0429:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<int32,valuetype [mscorlib]System.DateTime>>(string,
                                                                                                                         !!0,
                                                                                                                         !!0)
    IL_042e:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<int"
    + ">.Method1<string>"
    IL_0433:  ldc.i4.3
    IL_0434:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<int32>::.ctor(!0)
    IL_0439:  ldc.i4     0x989680
    IL_043e:  ldstr      "abc"
    IL_0443:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<int32>::Method1<string>(int32,
                                                                                                                                                          !!0)
    IL_0448:  ldc.i4.3
    IL_0449:  ldstr      "abc"
    IL_044e:  newobj     instance void class [mscorlib]System.Tuple`2<int32,string>::.ctor(!0,
                                                                                           !1)
    IL_0453:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<int32,string>>(string,
                                                                                            !!0,
                                                                                            !!0)
    IL_0458:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<Dat"
    + "eTime>.Method1<byte>"
    IL_045d:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_0462:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<valuetype [mscorlib]System.DateTime>::.ctor(!0)
    IL_0467:  ldc.i4     0x989680
    IL_046c:  ldc.i4.4
    IL_046d:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<valuetype [mscorlib]System.DateTime>::Method1<uint8>(int32,
                                                                                                                                                                                       !!0)
    IL_0472:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_0477:  ldc.i4.4
    IL_0478:  newobj     instance void class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,uint8>::.ctor(!0,
                                                                                                                        !1)
    IL_047d:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,uint8>>(string,
                                                                                                                         !!0,
                                                                                                                         !!0)
    IL_0482:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<Dat"
    + "eTime>.Method1<int>"
    IL_0487:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_048c:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<valuetype [mscorlib]System.DateTime>::.ctor(!0)
    IL_0491:  ldc.i4     0x989680
    IL_0496:  ldc.i4.4
    IL_0497:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<valuetype [mscorlib]System.DateTime>::Method1<int32>(int32,
                                                                                                                                                                                       !!0)
    IL_049c:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_04a1:  ldc.i4.4
    IL_04a2:  newobj     instance void class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,int32>::.ctor(!0,
                                                                                                                        !1)
    IL_04a7:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,int32>>(string,
                                                                                                                         !!0,
                                                                                                                         !!0)
    IL_04ac:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<Dat"
    + "eTime>.Method1<DateTime>"
    IL_04b1:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_04b6:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<valuetype [mscorlib]System.DateTime>::.ctor(!0)
    IL_04bb:  ldc.i4     0x989680
    IL_04c0:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_04c5:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<valuetype [mscorlib]System.DateTime>::Method1<valuetype [mscorlib]System.DateTime>(int32,
                                                                                                                                                                                                                     !!0)
    IL_04ca:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_04cf:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_04d4:  newobj     instance void class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,valuetype [mscorlib]System.DateTime>::.ctor(!0,
                                                                                                                                                      !1)
    IL_04d9:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,valuetype [mscorlib]System.DateTime>>(string,
                                                                                                                                                       !!0,
                                                                                                                                                       !!0)
    IL_04de:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<Dat"
    + "eTime>.Method1<string>"
    IL_04e3:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_04e8:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<valuetype [mscorlib]System.DateTime>::.ctor(!0)
    IL_04ed:  ldc.i4     0x989680
    IL_04f2:  ldstr      "abc"
    IL_04f7:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<valuetype [mscorlib]System.DateTime>::Method1<string>(int32,
                                                                                                                                                                                        !!0)
    IL_04fc:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_0501:  ldstr      "abc"
    IL_0506:  newobj     instance void class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,string>::.ctor(!0,
                                                                                                                         !1)
    IL_050b:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,string>>(string,
                                                                                                                          !!0,
                                                                                                                          !!0)
    IL_0510:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<str"
    + "ing>.Method1<byte>"
    IL_0515:  ldstr      "qq"
    IL_051a:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<string>::.ctor(!0)
    IL_051f:  ldc.i4     0x989680
    IL_0524:  ldc.i4.4
    IL_0525:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<string>::Method1<uint8>(int32,
                                                                                                                                                          !!0)
    IL_052a:  ldstr      "qq"
    IL_052f:  ldc.i4.4
    IL_0530:  newobj     instance void class [mscorlib]System.Tuple`2<string,uint8>::.ctor(!0,
                                                                                           !1)
    IL_0535:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<string,uint8>>(string,
                                                                                            !!0,
                                                                                            !!0)
    IL_053a:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<str"
    + "ing>.Method1<int>"
    IL_053f:  ldstr      "qq"
    IL_0544:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<string>::.ctor(!0)
    IL_0549:  ldc.i4     0x989680
    IL_054e:  ldc.i4.4
    IL_054f:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<string>::Method1<int32>(int32,
                                                                                                                                                          !!0)
    IL_0554:  ldstr      "qq"
    IL_0559:  ldc.i4.4
    IL_055a:  newobj     instance void class [mscorlib]System.Tuple`2<string,int32>::.ctor(!0,
                                                                                           !1)
    IL_055f:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<string,int32>>(string,
                                                                                            !!0,
                                                                                            !!0)
    IL_0564:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<str"
    + "ing>.Method1<DateTime>"
    IL_0569:  ldstr      "qq"
    IL_056e:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<string>::.ctor(!0)
    IL_0573:  ldc.i4     0x989680
    IL_0578:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_057d:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<string>::Method1<valuetype [mscorlib]System.DateTime>(int32,
                                                                                                                                                                                        !!0)
    IL_0582:  ldstr      "qq"
    IL_0587:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_058c:  newobj     instance void class [mscorlib]System.Tuple`2<string,valuetype [mscorlib]System.DateTime>::.ctor(!0,
                                                                                                                         !1)
    IL_0591:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<string,valuetype [mscorlib]System.DateTime>>(string,
                                                                                                                          !!0,
                                                                                                                          !!0)
    IL_0596:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<Dat"
    + "stringeTime>.Method1<string>"
    IL_059b:  ldstr      "qq"
    IL_05a0:  newobj     instance void class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<string>::.ctor(!0)
    IL_05a5:  ldc.i4     0x989680
    IL_05aa:  ldstr      "abc"
    IL_05af:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<string>::Method1<string>(int32,
                                                                                                                                                           !!0)
    IL_05b4:  ldstr      "qq"
    IL_05b9:  ldstr      "abc"
    IL_05be:  newobj     instance void class [mscorlib]System.Tuple`2<string,string>::.ctor(!0,
                                                                                            !1)
    IL_05c3:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<string,string>>(string,
                                                                                             !!0,
                                                                                             !!0)
    IL_05c8:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<byt"
    + "e>.Method1<byte>"
    IL_05cd:  ldc.i4.3
    IL_05ce:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<uint8>(!!0)
    IL_05d3:  ldc.i4     0x989680
    IL_05d8:  ldc.i4.4
    IL_05d9:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<uint8>::Method1<uint8>(int32,
                                                                                                                                                     !!0)
    IL_05de:  ldc.i4.3
    IL_05df:  ldc.i4.4
    IL_05e0:  newobj     instance void class [mscorlib]System.Tuple`2<uint8,uint8>::.ctor(!0,
                                                                                          !1)
    IL_05e5:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<uint8,uint8>>(string,
                                                                                           !!0,
                                                                                           !!0)
    IL_05ea:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<byt"
    + "e>.Method1<int>"
    IL_05ef:  ldc.i4.3
    IL_05f0:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<uint8>(!!0)
    IL_05f5:  ldc.i4     0x989680
    IL_05fa:  ldc.i4.4
    IL_05fb:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<uint8>::Method1<int32>(int32,
                                                                                                                                                     !!0)
    IL_0600:  ldc.i4.3
    IL_0601:  ldc.i4.4
    IL_0602:  newobj     instance void class [mscorlib]System.Tuple`2<uint8,int32>::.ctor(!0,
                                                                                          !1)
    IL_0607:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<uint8,int32>>(string,
                                                                                           !!0,
                                                                                           !!0)
    IL_060c:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<byt"
    + "e>.Method1<DateTime>"
    IL_0611:  ldc.i4.3
    IL_0612:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<uint8>(!!0)
    IL_0617:  ldc.i4     0x989680
    IL_061c:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_0621:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<uint8>::Method1<valuetype [mscorlib]System.DateTime>(int32,
                                                                                                                                                                                   !!0)
    IL_0626:  ldc.i4.3
    IL_0627:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_062c:  newobj     instance void class [mscorlib]System.Tuple`2<uint8,valuetype [mscorlib]System.DateTime>::.ctor(!0,
                                                                                                                        !1)
    IL_0631:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<uint8,valuetype [mscorlib]System.DateTime>>(string,
                                                                                                                         !!0,
                                                                                                                         !!0)
    IL_0636:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<byt"
    + "e>.Method1<string>"
    IL_063b:  ldc.i4.3
    IL_063c:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<uint8>(!!0)
    IL_0641:  ldc.i4     0x989680
    IL_0646:  ldstr      "abc"
    IL_064b:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<uint8>::Method1<string>(int32,
                                                                                                                                                      !!0)
    IL_0650:  ldc.i4.3
    IL_0651:  ldstr      "abc"
    IL_0656:  newobj     instance void class [mscorlib]System.Tuple`2<uint8,string>::.ctor(!0,
                                                                                           !1)
    IL_065b:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<uint8,string>>(string,
                                                                                            !!0,
                                                                                            !!0)
    IL_0660:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<int"
    + ">.Method1<byte>"
    IL_0665:  ldc.i4.3
    IL_0666:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<int32>(!!0)
    IL_066b:  ldc.i4     0x989680
    IL_0670:  ldc.i4.4
    IL_0671:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<int32>::Method1<uint8>(int32,
                                                                                                                                                     !!0)
    IL_0676:  ldc.i4.3
    IL_0677:  ldc.i4.4
    IL_0678:  newobj     instance void class [mscorlib]System.Tuple`2<int32,uint8>::.ctor(!0,
                                                                                          !1)
    IL_067d:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<int32,uint8>>(string,
                                                                                           !!0,
                                                                                           !!0)
    IL_0682:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<int"
    + ">.Method1<int>"
    IL_0687:  ldc.i4.3
    IL_0688:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<int32>(!!0)
    IL_068d:  ldc.i4     0x989680
    IL_0692:  ldc.i4.4
    IL_0693:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<int32>::Method1<int32>(int32,
                                                                                                                                                     !!0)
    IL_0698:  ldc.i4.3
    IL_0699:  ldc.i4.4
    IL_069a:  newobj     instance void class [mscorlib]System.Tuple`2<int32,int32>::.ctor(!0,
                                                                                          !1)
    IL_069f:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<int32,int32>>(string,
                                                                                           !!0,
                                                                                           !!0)
    IL_06a4:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<int"
    + ">.Method1<DateTime>"
    IL_06a9:  ldc.i4.3
    IL_06aa:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<int32>(!!0)
    IL_06af:  ldc.i4     0x989680
    IL_06b4:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_06b9:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<int32>::Method1<valuetype [mscorlib]System.DateTime>(int32,
                                                                                                                                                                                   !!0)
    IL_06be:  ldc.i4.3
    IL_06bf:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_06c4:  newobj     instance void class [mscorlib]System.Tuple`2<int32,valuetype [mscorlib]System.DateTime>::.ctor(!0,
                                                                                                                        !1)
    IL_06c9:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<int32,valuetype [mscorlib]System.DateTime>>(string,
                                                                                                                         !!0,
                                                                                                                         !!0)
    IL_06ce:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<int"
    + ">.Method1<string>"
    IL_06d3:  ldc.i4.3
    IL_06d4:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<int32>(!!0)
    IL_06d9:  ldc.i4     0x989680
    IL_06de:  ldstr      "abc"
    IL_06e3:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<int32>::Method1<string>(int32,
                                                                                                                                                      !!0)
    IL_06e8:  ldc.i4.3
    IL_06e9:  ldstr      "abc"
    IL_06ee:  newobj     instance void class [mscorlib]System.Tuple`2<int32,string>::.ctor(!0,
                                                                                           !1)
    IL_06f3:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<int32,string>>(string,
                                                                                            !!0,
                                                                                            !!0)
    IL_06f8:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<Dat"
    + "eTime>.Method1<byte>"
    IL_06fd:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_0702:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<valuetype [mscorlib]System.DateTime>(!!0)
    IL_0707:  ldc.i4     0x989680
    IL_070c:  ldc.i4.4
    IL_070d:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<valuetype [mscorlib]System.DateTime>::Method1<uint8>(int32,
                                                                                                                                                                                   !!0)
    IL_0712:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_0717:  ldc.i4.4
    IL_0718:  newobj     instance void class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,uint8>::.ctor(!0,
                                                                                                                        !1)
    IL_071d:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,uint8>>(string,
                                                                                                                         !!0,
                                                                                                                         !!0)
    IL_0722:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<Dat"
    + "eTime>.Method1<int>"
    IL_0727:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_072c:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<valuetype [mscorlib]System.DateTime>(!!0)
    IL_0731:  ldc.i4     0x989680
    IL_0736:  ldc.i4.4
    IL_0737:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<valuetype [mscorlib]System.DateTime>::Method1<int32>(int32,
                                                                                                                                                                                   !!0)
    IL_073c:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_0741:  ldc.i4.4
    IL_0742:  newobj     instance void class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,int32>::.ctor(!0,
                                                                                                                        !1)
    IL_0747:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,int32>>(string,
                                                                                                                         !!0,
                                                                                                                         !!0)
    IL_074c:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<Dat"
    + "eTime>.Method1<DateTime>"
    IL_0751:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_0756:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<valuetype [mscorlib]System.DateTime>(!!0)
    IL_075b:  ldc.i4     0x989680
    IL_0760:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_0765:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<valuetype [mscorlib]System.DateTime>::Method1<valuetype [mscorlib]System.DateTime>(int32,
                                                                                                                                                                                                                 !!0)
    IL_076a:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_076f:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_0774:  newobj     instance void class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,valuetype [mscorlib]System.DateTime>::.ctor(!0,
                                                                                                                                                      !1)
    IL_0779:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,valuetype [mscorlib]System.DateTime>>(string,
                                                                                                                                                       !!0,
                                                                                                                                                       !!0)
    IL_077e:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<Dat"
    + "eTime>.Method1<string>"
    IL_0783:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_0788:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<valuetype [mscorlib]System.DateTime>(!!0)
    IL_078d:  ldc.i4     0x989680
    IL_0792:  ldstr      "abc"
    IL_0797:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<valuetype [mscorlib]System.DateTime>::Method1<string>(int32,
                                                                                                                                                                                    !!0)
    IL_079c:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MaxValue
    IL_07a1:  ldstr      "abc"
    IL_07a6:  newobj     instance void class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,string>::.ctor(!0,
                                                                                                                         !1)
    IL_07ab:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<valuetype [mscorlib]System.DateTime,string>>(string,
                                                                                                                          !!0,
                                                                                                                          !!0)
    IL_07b0:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<str"
    + "ing>.Method1<byte>"
    IL_07b5:  ldstr      "qq"
    IL_07ba:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<string>(!!0)
    IL_07bf:  ldc.i4     0x989680
    IL_07c4:  ldc.i4.4
    IL_07c5:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<string>::Method1<uint8>(int32,
                                                                                                                                                      !!0)
    IL_07ca:  ldstr      "qq"
    IL_07cf:  ldc.i4.4
    IL_07d0:  newobj     instance void class [mscorlib]System.Tuple`2<string,uint8>::.ctor(!0,
                                                                                           !1)
    IL_07d5:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<string,uint8>>(string,
                                                                                            !!0,
                                                                                            !!0)
    IL_07da:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<str"
    + "ing>.Method1<int>"
    IL_07df:  ldstr      "qq"
    IL_07e4:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<string>(!!0)
    IL_07e9:  ldc.i4     0x989680
    IL_07ee:  ldc.i4.4
    IL_07ef:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<string>::Method1<int32>(int32,
                                                                                                                                                      !!0)
    IL_07f4:  ldstr      "qq"
    IL_07f9:  ldc.i4.4
    IL_07fa:  newobj     instance void class [mscorlib]System.Tuple`2<string,int32>::.ctor(!0,
                                                                                           !1)
    IL_07ff:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<string,int32>>(string,
                                                                                            !!0,
                                                                                            !!0)
    IL_0804:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<str"
    + "ing>.Method1<DateTime>"
    IL_0809:  ldstr      "qq"
    IL_080e:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<string>(!!0)
    IL_0813:  ldc.i4     0x989680
    IL_0818:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_081d:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<string>::Method1<valuetype [mscorlib]System.DateTime>(int32,
                                                                                                                                                                                    !!0)
    IL_0822:  ldstr      "qq"
    IL_0827:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_082c:  newobj     instance void class [mscorlib]System.Tuple`2<string,valuetype [mscorlib]System.DateTime>::.ctor(!0,
                                                                                                                         !1)
    IL_0831:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<string,valuetype [mscorlib]System.DateTime>>(string,
                                                                                                                          !!0,
                                                                                                                          !!0)
    IL_0836:  ldstr      "TailCallLoopGenericClassAndMethodAbstractClass<Dat"
    + "stringeTime>.Method1<string>"
    IL_083b:  ldstr      "qq"
    IL_0840:  call       class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!0> Fsharp::create<string>(!!0)
    IL_0845:  ldc.i4     0x989680
    IL_084a:  ldstr      "abc"
    IL_084f:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<string>::Method1<string>(int32,
                                                                                                                                                       !!0)
    IL_0854:  ldstr      "qq"
    IL_0859:  ldstr      "abc"
    IL_085e:  newobj     instance void class [mscorlib]System.Tuple`2<string,string>::.ctor(!0,
                                                                                            !1)
    IL_0863:  call       void Fsharp::RunTest<class [mscorlib]System.Tuple`2<string,string>>(string,
                                                                                             !!0,
                                                                                             !!0)
    IL_0868:  ldstr      "TailCallLoopGenericClass<int>"
    IL_086d:  ldc.i4.3
    IL_086e:  newobj     instance void class Fsharp/TailCallLoopGenericClass`1<int32>::.ctor(!0)
    IL_0873:  callvirt   instance !0 class Fsharp/TailCallLoopGenericClass`1<int32>::get_Result()
    IL_0878:  ldc.i4.3
    IL_0879:  call       void Fsharp::RunTest<int32>(string,
                                                     !!0,
                                                     !!0)
    IL_087e:  ldstr      "TailCallLoopGenericClass<DateTime>"
    IL_0883:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_0888:  newobj     instance void class Fsharp/TailCallLoopGenericClass`1<valuetype [mscorlib]System.DateTime>::.ctor(!0)
    IL_088d:  callvirt   instance !0 class Fsharp/TailCallLoopGenericClass`1<valuetype [mscorlib]System.DateTime>::get_Result()
    IL_0892:  ldsfld     valuetype [mscorlib]System.DateTime [mscorlib]System.DateTime::MinValue
    IL_0897:  call       void Fsharp::RunTest<valuetype [mscorlib]System.DateTime>(string,
                                                                                   !!0,
                                                                                   !!0)
    IL_089c:  ldstr      "TailCallLoopGenericClass<string>"
    IL_08a1:  ldstr      "abc"
    IL_08a6:  newobj     instance void class Fsharp/TailCallLoopGenericClass`1<string>::.ctor(!0)
    IL_08ab:  callvirt   instance !0 class Fsharp/TailCallLoopGenericClass`1<string>::get_Result()
    IL_08b0:  ldstr      "abc"
    IL_08b5:  call       void Fsharp::RunTest<string>(string,
                                                      !!0,
                                                      !!0)
    IL_08ba:  ldstr      "StaticTailCallLoop<int>"
    IL_08bf:  call       int32 class Fsharp/StaticTailCallLoop`1<int32>::get_Result()
    IL_08c4:  ldc.i4.8
    IL_08c5:  call       void Fsharp::RunTest<int32>(string,
                                                     !!0,
                                                     !!0)
    IL_08ca:  ldstr      "StaticTailCallLoop<DateTime>"
    IL_08cf:  call       int32 class Fsharp/StaticTailCallLoop`1<valuetype [mscorlib]System.DateTime>::get_Result()
    IL_08d4:  ldc.i4.s   16
    IL_08d6:  call       void Fsharp::RunTest<int32>(string,
                                                     !!0,
                                                     !!0)
    IL_08db:  ldstr      "Seq.filter-length1"
    IL_08e0:  newobj     instance void Fsharp/clo@223::.ctor()
    IL_08e5:  ldc.i4.1
    IL_08e6:  ldc.i4.1
    IL_08e7:  ldc.i4     0xf4240
    IL_08ec:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics::RangeInt32(int32,
                                                                                                                                                        int32,
                                                                                                                                                        int32)
    IL_08f1:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0> Microsoft.FSharp.Collections.SeqModule::Filter<int32>(class Microsoft.FSharp.Core.FSharpFunc`2<!!0,bool>,
                                                                                                                                             class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_08f6:  call       int32 Microsoft.FSharp.Collections.SeqModule::Length<int32>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_08fb:  ldc.i4     0xf423f
    IL_0900:  call       void Fsharp::RunTest<int32>(string,
                                                     !!0,
                                                     !!0)
    IL_0905:  ldstr      "Seq.filter-length2"
    IL_090a:  newobj     instance void Fsharp/'clo@224-1'::.ctor()
    IL_090f:  ldc.i4.1
    IL_0910:  ldc.i4.1
    IL_0911:  ldc.i4     0xf4240
    IL_0916:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics::RangeInt32(int32,
                                                                                                                                                        int32,
                                                                                                                                                        int32)
    IL_091b:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0> Microsoft.FSharp.Collections.SeqModule::Filter<int32>(class Microsoft.FSharp.Core.FSharpFunc`2<!!0,bool>,
                                                                                                                                             class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_0920:  call       int32 Microsoft.FSharp.Collections.SeqModule::Length<int32>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_0925:  ldc.i4.1
    IL_0926:  call       void Fsharp::RunTest<int32>(string,
                                                     !!0,
                                                     !!0)
    IL_092b:  ldstr      "Seq.filter-length3"
    IL_0930:  newobj     instance void Fsharp/'clo@225-2'::.ctor()
    IL_0935:  ldc.i4.1
    IL_0936:  ldc.i4.1
    IL_0937:  ldc.i4     0xf4240
    IL_093c:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics::RangeInt32(int32,
                                                                                                                                                        int32,
                                                                                                                                                        int32)
    IL_0941:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0> Microsoft.FSharp.Collections.SeqModule::Filter<int32>(class Microsoft.FSharp.Core.FSharpFunc`2<!!0,bool>,
                                                                                                                                             class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_0946:  call       int32 Microsoft.FSharp.Collections.SeqModule::Length<int32>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_094b:  ldc.i4     0x7a120
    IL_0950:  call       void Fsharp::RunTest<int32>(string,
                                                     !!0,
                                                     !!0)
    IL_0955:  ldc.i4.0
    IL_0956:  call       !!0 Microsoft.FSharp.Core.Operators::Exit<class Microsoft.FSharp.Core.Unit>(int32)
    IL_095b:  pop
    IL_095c:  ret
  } // end of method $Fsharp::main@

} // end of class '<StartupCode$fsharp>'.$Fsharp

.class private abstract auto ansi sealed '<StartupCode$FSharp-Core>'.'$Prim-types'
       extends [mscorlib]System.Object
{
  .field static assembly initonly string AddressOpNotFirstClassString@656
  .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                              int32) = ( 01 00 49 54 68 69 73 20 76 61 6C 75 65 20 69 73   // ..IThis value is
                                                                                         20 66 6F 72 20 75 73 65 20 62 79 20 63 6F 6D 70   //  for use by comp
                                                                                         69 6C 65 64 20 46 23 20 63 6F 64 65 20 61 6E 64   // iled F# code and
                                                                                         20 73 68 6F 75 6C 64 20 6E 6F 74 20 62 65 20 75   //  should not be u
                                                                                         73 65 64 20 64 69 72 65 63 74 6C 79 B4 04 00 00   // sed directly....
                                                                                         01 00 54 02 08 49 73 48 69 64 64 65 6E 01 )       // ..T..IsHidden.
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly string NoNegateMinValueString@657
  .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                              int32) = ( 01 00 49 54 68 69 73 20 76 61 6C 75 65 20 69 73   // ..IThis value is
                                                                                         20 66 6F 72 20 75 73 65 20 62 79 20 63 6F 6D 70   //  for use by comp
                                                                                         69 6C 65 64 20 46 23 20 63 6F 64 65 20 61 6E 64   // iled F# code and
                                                                                         20 73 68 6F 75 6C 64 20 6E 6F 74 20 62 65 20 75   //  should not be u
                                                                                         73 65 64 20 64 69 72 65 63 74 6C 79 B4 04 00 00   // sed directly....
                                                                                         01 00 54 02 08 49 73 48 69 64 64 65 6E 01 )       // ..T..IsHidden.
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly string InputSequenceEmptyString@659
  .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                              int32) = ( 01 00 49 54 68 69 73 20 76 61 6C 75 65 20 69 73   // ..IThis value is
                                                                                         20 66 6F 72 20 75 73 65 20 62 79 20 63 6F 6D 70   //  for use by comp
                                                                                         69 6C 65 64 20 46 23 20 63 6F 64 65 20 61 6E 64   // iled F# code and
                                                                                         20 73 68 6F 75 6C 64 20 6E 6F 74 20 62 65 20 75   //  should not be u
                                                                                         73 65 64 20 64 69 72 65 63 74 6C 79 B4 04 00 00   // sed directly....
                                                                                         01 00 54 02 08 49 73 48 69 64 64 65 6E 01 )       // ..T..IsHidden.
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly string InputArrayEmptyString@661
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                              int32) = ( 01 00 49 54 68 69 73 20 76 61 6C 75 65 20 69 73   // ..IThis value is
                                                                                         20 66 6F 72 20 75 73 65 20 62 79 20 63 6F 6D 70   //  for use by comp
                                                                                         69 6C 65 64 20 46 23 20 63 6F 64 65 20 61 6E 64   // iled F# code and
                                                                                         20 73 68 6F 75 6C 64 20 6E 6F 74 20 62 65 20 75   //  should not be u
                                                                                         73 65 64 20 64 69 72 65 63 74 6C 79 B4 04 00 00   // sed directly....
                                                                                         01 00 54 02 08 49 73 48 69 64 64 65 6E 01 )       // ..T..IsHidden.
  .field static assembly initonly string InputMustBeNonNegativeString@663
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                              int32) = ( 01 00 49 54 68 69 73 20 76 61 6C 75 65 20 69 73   // ..IThis value is
                                                                                         20 66 6F 72 20 75 73 65 20 62 79 20 63 6F 6D 70   //  for use by comp
                                                                                         69 6C 65 64 20 46 23 20 63 6F 64 65 20 61 6E 64   // iled F# code and
                                                                                         20 73 68 6F 75 6C 64 20 6E 6F 74 20 62 65 20 75   //  should not be u
                                                                                         73 65 64 20 64 69 72 65 63 74 6C 79 B4 04 00 00   // sed directly....
                                                                                         01 00 54 02 08 49 73 48 69 64 64 65 6E 01 )       // ..T..IsHidden.
  .field static assembly initonly class [mscorlib]System.Exception NaNException@951
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer fsComparerPER@1179
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer fsComparerER@1182
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.IEqualityComparer fsEqualityComparerNoHashingPER@1654
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.IEqualityComparer fsEqualityComparerNoHashingER@1661
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherER fsEqualityComparerUnlimitedHashingER@1807
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherPER fsEqualityComparerUnlimitedHashingPER@1810
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.IComparer GenericComparer@2195
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.IEqualityComparer GenericEqualityComparer@2196
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.IEqualityComparer GenericEqualityERComparer@2197
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<bool> BoolIEquality@2220
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<char> CharIEquality@2221
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<string> StringIEquality@2222
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int8> SByteIEquality@2223
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int16> Int16IEquality@2224
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int32> Int32IEquality@2225
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int64> Int64IEquality@2226
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<native int> IntPtrIEquality@2227
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint8> ByteIEquality@2228
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint16> UInt16IEquality@2229
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint32> UInt32IEquality@2230
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint64> UInt64IEquality@2231
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<native uint> UIntPtrIEquality@2232
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<float64> FloatIEquality@2233
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<float32> Float32IEquality@2234
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IEqualityComparer`1<valuetype [mscorlib]System.Decimal> DecimalIEquality@2235
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<char> CharComparer@2299
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<string> StringComparer@2300
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<int8> SByteComparer@2301
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<int16> Int16Comparer@2302
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<int32> Int32Comparer@2303
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<int64> Int64Comparer@2304
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<native int> IntPtrComparer@2305
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<uint8> ByteComparer@2306
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<uint16> UInt16Comparer@2307
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<uint32> UInt32Comparer@2308
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<uint64> UInt64Comparer@2309
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<native uint> UIntPtrComparer@2310
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<float64> FloatComparer@2311
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<float32> Float32Comparer@2312
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field static assembly initonly class [mscorlib]System.Collections.Generic.IComparer`1<valuetype [mscorlib]System.Decimal> DecimalComparer@2313
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .method private specialname rtspecialname static
          void  .cctor() cil managed
  {
    // Code size       476 (0x1dc)
    .maxstack  1
    IL_0000:  nop
    IL_0001:  ldstr      "addressOpNotFirstClass"
    IL_0006:  stsfld     string '<StartupCode$FSharp-Core>'.'$Prim-types'::AddressOpNotFirstClassString@656
    IL_000b:  ldstr      "noNegateMinValue"
    IL_0010:  stsfld     string '<StartupCode$FSharp-Core>'.'$Prim-types'::NoNegateMinValueString@657
    IL_0015:  ldstr      "inputSequenceEmpty"
    IL_001a:  stsfld     string '<StartupCode$FSharp-Core>'.'$Prim-types'::InputSequenceEmptyString@659
    IL_001f:  ldstr      "arrayWasEmpty"
    IL_0024:  stsfld     string '<StartupCode$FSharp-Core>'.'$Prim-types'::InputArrayEmptyString@661
    IL_0029:  ldstr      "inputMustBeNonNegative"
    IL_002e:  stsfld     string '<StartupCode$FSharp-Core>'.'$Prim-types'::InputMustBeNonNegativeString@663
    IL_0033:  nop
    IL_0034:  nop
    IL_0035:  nop
    IL_0036:  nop
    IL_0037:  nop
    IL_0038:  newobj     instance void [mscorlib]System.Exception::.ctor()
    IL_003d:  stsfld     class [mscorlib]System.Exception '<StartupCode$FSharp-Core>'.'$Prim-types'::NaNException@951
    IL_0042:  ldc.i4.1
    IL_0043:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer::.ctor(bool)
    IL_0048:  stsfld     class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer '<StartupCode$FSharp-Core>'.'$Prim-types'::fsComparerPER@1179
    IL_004d:  ldc.i4.0
    IL_004e:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer::.ctor(bool)
    IL_0053:  stsfld     class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer '<StartupCode$FSharp-Core>'.'$Prim-types'::fsComparerER@1182
    IL_0058:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/fsEqualityComparerNoHashingPER@1655::.ctor()
    IL_005d:  stsfld     class [mscorlib]System.Collections.IEqualityComparer '<StartupCode$FSharp-Core>'.'$Prim-types'::fsEqualityComparerNoHashingPER@1654
    IL_0062:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/fsEqualityComparerNoHashingER@1662::.ctor()
    IL_0067:  stsfld     class [mscorlib]System.Collections.IEqualityComparer '<StartupCode$FSharp-Core>'.'$Prim-types'::fsEqualityComparerNoHashingER@1661
    IL_006c:  nop
    IL_006d:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherER::.ctor()
    IL_0072:  stsfld     class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherER '<StartupCode$FSharp-Core>'.'$Prim-types'::fsEqualityComparerUnlimitedHashingER@1807
    IL_0077:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherPER::.ctor()
    IL_007c:  stsfld     class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherPER '<StartupCode$FSharp-Core>'.'$Prim-types'::fsEqualityComparerUnlimitedHashingPER@1810
    IL_0081:  call       class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_fsComparerER()
    IL_0086:  stsfld     class [mscorlib]System.Collections.IComparer '<StartupCode$FSharp-Core>'.'$Prim-types'::GenericComparer@2195
    IL_008b:  call       class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherPER Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_fsEqualityComparerUnlimitedHashingPER()
    IL_0090:  stsfld     class [mscorlib]System.Collections.IEqualityComparer '<StartupCode$FSharp-Core>'.'$Prim-types'::GenericEqualityComparer@2196
    IL_0095:  call       class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherER Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_fsEqualityComparerUnlimitedHashingER()
    IL_009a:  stsfld     class [mscorlib]System.Collections.IEqualityComparer '<StartupCode$FSharp-Core>'.'$Prim-types'::GenericEqualityERComparer@2197
    IL_009f:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/BoolIEquality@2220::.ctor()
    IL_00a4:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<bool> '<StartupCode$FSharp-Core>'.'$Prim-types'::BoolIEquality@2220
    IL_00a9:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/CharIEquality@2221::.ctor()
    IL_00ae:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<char> '<StartupCode$FSharp-Core>'.'$Prim-types'::CharIEquality@2221
    IL_00b3:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/StringIEquality@2222::.ctor()
    IL_00b8:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<string> '<StartupCode$FSharp-Core>'.'$Prim-types'::StringIEquality@2222
    IL_00bd:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/SByteIEquality@2223::.ctor()
    IL_00c2:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int8> '<StartupCode$FSharp-Core>'.'$Prim-types'::SByteIEquality@2223
    IL_00c7:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/Int16IEquality@2224::.ctor()
    IL_00cc:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int16> '<StartupCode$FSharp-Core>'.'$Prim-types'::Int16IEquality@2224
    IL_00d1:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/Int32IEquality@2225::.ctor()
    IL_00d6:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int32> '<StartupCode$FSharp-Core>'.'$Prim-types'::Int32IEquality@2225
    IL_00db:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/Int64IEquality@2226::.ctor()
    IL_00e0:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int64> '<StartupCode$FSharp-Core>'.'$Prim-types'::Int64IEquality@2226
    IL_00e5:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/IntPtrIEquality@2227::.ctor()
    IL_00ea:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<native int> '<StartupCode$FSharp-Core>'.'$Prim-types'::IntPtrIEquality@2227
    IL_00ef:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/ByteIEquality@2228::.ctor()
    IL_00f4:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint8> '<StartupCode$FSharp-Core>'.'$Prim-types'::ByteIEquality@2228
    IL_00f9:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/UInt16IEquality@2229::.ctor()
    IL_00fe:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint16> '<StartupCode$FSharp-Core>'.'$Prim-types'::UInt16IEquality@2229
    IL_0103:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/UInt32IEquality@2230::.ctor()
    IL_0108:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint32> '<StartupCode$FSharp-Core>'.'$Prim-types'::UInt32IEquality@2230
    IL_010d:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/UInt64IEquality@2231::.ctor()
    IL_0112:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint64> '<StartupCode$FSharp-Core>'.'$Prim-types'::UInt64IEquality@2231
    IL_0117:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/UIntPtrIEquality@2232::.ctor()
    IL_011c:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<native uint> '<StartupCode$FSharp-Core>'.'$Prim-types'::UIntPtrIEquality@2232
    IL_0121:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/FloatIEquality@2233::.ctor()
    IL_0126:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<float64> '<StartupCode$FSharp-Core>'.'$Prim-types'::FloatIEquality@2233
    IL_012b:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/Float32IEquality@2234::.ctor()
    IL_0130:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<float32> '<StartupCode$FSharp-Core>'.'$Prim-types'::Float32IEquality@2234
    IL_0135:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/DecimalIEquality@2235::.ctor()
    IL_013a:  stsfld     class [mscorlib]System.Collections.Generic.IEqualityComparer`1<valuetype [mscorlib]System.Decimal> '<StartupCode$FSharp-Core>'.'$Prim-types'::DecimalIEquality@2235
    IL_013f:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/CharComparer@2299::.ctor()
    IL_0144:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<char> '<StartupCode$FSharp-Core>'.'$Prim-types'::CharComparer@2299
    IL_0149:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/StringComparer@2300::.ctor()
    IL_014e:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<string> '<StartupCode$FSharp-Core>'.'$Prim-types'::StringComparer@2300
    IL_0153:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/SByteComparer@2301::.ctor()
    IL_0158:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<int8> '<StartupCode$FSharp-Core>'.'$Prim-types'::SByteComparer@2301
    IL_015d:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/Int16Comparer@2302::.ctor()
    IL_0162:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<int16> '<StartupCode$FSharp-Core>'.'$Prim-types'::Int16Comparer@2302
    IL_0167:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/Int32Comparer@2303::.ctor()
    IL_016c:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<int32> '<StartupCode$FSharp-Core>'.'$Prim-types'::Int32Comparer@2303
    IL_0171:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/Int64Comparer@2304::.ctor()
    IL_0176:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<int64> '<StartupCode$FSharp-Core>'.'$Prim-types'::Int64Comparer@2304
    IL_017b:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/IntPtrComparer@2305::.ctor()
    IL_0180:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<native int> '<StartupCode$FSharp-Core>'.'$Prim-types'::IntPtrComparer@2305
    IL_0185:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/ByteComparer@2306::.ctor()
    IL_018a:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<uint8> '<StartupCode$FSharp-Core>'.'$Prim-types'::ByteComparer@2306
    IL_018f:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/UInt16Comparer@2307::.ctor()
    IL_0194:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<uint16> '<StartupCode$FSharp-Core>'.'$Prim-types'::UInt16Comparer@2307
    IL_0199:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/UInt32Comparer@2308::.ctor()
    IL_019e:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<uint32> '<StartupCode$FSharp-Core>'.'$Prim-types'::UInt32Comparer@2308
    IL_01a3:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/UInt64Comparer@2309::.ctor()
    IL_01a8:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<uint64> '<StartupCode$FSharp-Core>'.'$Prim-types'::UInt64Comparer@2309
    IL_01ad:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/UIntPtrComparer@2310::.ctor()
    IL_01b2:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<native uint> '<StartupCode$FSharp-Core>'.'$Prim-types'::UIntPtrComparer@2310
    IL_01b7:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/FloatComparer@2311::.ctor()
    IL_01bc:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<float64> '<StartupCode$FSharp-Core>'.'$Prim-types'::FloatComparer@2311
    IL_01c1:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/Float32Comparer@2312::.ctor()
    IL_01c6:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<float32> '<StartupCode$FSharp-Core>'.'$Prim-types'::Float32Comparer@2312
    IL_01cb:  newobj     instance void Microsoft.FSharp.Core.LanguagePrimitives/DecimalComparer@2313::.ctor()
    IL_01d0:  stsfld     class [mscorlib]System.Collections.Generic.IComparer`1<valuetype [mscorlib]System.Decimal> '<StartupCode$FSharp-Core>'.'$Prim-types'::DecimalComparer@2313
    IL_01d5:  nop
    IL_01d6:  nop
    IL_01d7:  nop
    IL_01d8:  nop
    IL_01d9:  nop
    IL_01da:  nop
    IL_01db:  ret
  } // end of method '$Prim-types'::.cctor

} // end of class '<StartupCode$FSharp-Core>'.'$Prim-types'

.class public abstract auto ansi sealed Fsharp
       extends [mscorlib]System.Object
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 07 00 00 00 00 00 )
  .class abstract auto ansi serializable nested public AbstractTailCallLoopGenericClassAndMethod`1<T1>
         extends [mscorlib]System.Object
  {
    .custom instance void Microsoft.FSharp.Core.AbstractClassAttribute::.ctor() = ( 01 00 00 00 )
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
    .field assembly !T1 resultA
    .method public hidebysig abstract virtual
            instance class [mscorlib]System.Tuple`2<!T1,!!T2>
            Method1<T2>(int32 A_1,
                        !!T2 A_2) cil managed
    {
    } // end of method AbstractTailCallLoopGenericClassAndMethod`1::Method1

    .method public specialname rtspecialname
            instance void  .ctor(!T1 resultA) cil managed
    {
      // Code size       16 (0x10)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ldarg.0
      IL_0007:  pop
      IL_0008:  ldarg.0
      IL_0009:  ldarg.1
      IL_000a:  stfld      !0 class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<!T1>::resultA
      IL_000f:  ret
    } // end of method AbstractTailCallLoopGenericClassAndMethod`1::.ctor

    .method public hidebysig virtual instance class [mscorlib]System.Tuple`2<!T1,!!T2>
            Method2<T2>(int32 x,
                        !!T2 resultB) cil managed
    {
      // Code size       33 (0x21)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  brtrue.s   IL_0005

      IL_0003:  br.s       IL_0007

      IL_0005:  br.s       IL_0014

      IL_0007:  ldarg.0
      IL_0008:  ldfld      !0 class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<!T1>::resultA
      IL_000d:  ldarg.2
      IL_000e:  newobj     instance void class [mscorlib]System.Tuple`2<!T1,!!T2>::.ctor(!0,
                                                                                         !1)
      IL_0013:  ret

      IL_0014:  ldarg.0
      IL_0015:  ldarg.1
      IL_0016:  ldc.i4.1
      IL_0017:  sub
      IL_0018:  ldarg.2
      IL_0019:  tail.
      IL_001b:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<!T1>::Method1<!!0>(int32,
                                                                                                                                                       !!0)
      IL_0020:  ret
    } // end of method AbstractTailCallLoopGenericClassAndMethod`1::Method2

  } // end of class AbstractTailCallLoopGenericClassAndMethod`1

  .class auto ansi serializable sealed nested assembly beforefieldinit clo@223
         extends class Microsoft.FSharp.Core.FSharpFunc`2<int32,bool>
  {
    .method assembly specialname rtspecialname
            instance void  .ctor() cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void class Microsoft.FSharp.Core.FSharpFunc`2<int32,bool>::.ctor()
      IL_0006:  ret
    } // end of method clo@223::.ctor

    .method public strict virtual instance bool
            Invoke(int32 n) cil managed
    {
      // Code size       8 (0x8)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldc.i4.1
      IL_0002:  ceq
      IL_0004:  ldc.i4.0
      IL_0005:  ceq
      IL_0007:  ret
    } // end of method clo@223::Invoke

  } // end of class clo@223

  .class auto ansi serializable sealed nested assembly beforefieldinit 'clo@224-1'
         extends class Microsoft.FSharp.Core.FSharpFunc`2<int32,bool>
  {
    .method assembly specialname rtspecialname
            instance void  .ctor() cil managed
    {
      .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void class Microsoft.FSharp.Core.FSharpFunc`2<int32,bool>::.ctor()
      IL_0006:  ret
    } // end of method 'clo@224-1'::.ctor

    .method public strict virtual instance bool
            Invoke(int32 n) cil managed
    {
      // Code size       5 (0x5)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldc.i4.1
      IL_0002:  ceq
      IL_0004:  ret
    } // end of method 'clo@224-1'::Invoke

  } // end of class 'clo@224-1'

  .class auto ansi serializable sealed nested assembly beforefieldinit 'clo@225-2'
         extends class Microsoft.FSharp.Core.FSharpFunc`2<int32,bool>
  {
    .method assembly specialname rtspecialname
            instance void  .ctor() cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void class Microsoft.FSharp.Core.FSharpFunc`2<int32,bool>::.ctor()
      IL_0006:  ret
    } // end of method 'clo@225-2'::.ctor

    .method public strict virtual instance bool
            Invoke(int32 n) cil managed
    {
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  ldc.i4.2
      IL_0002:  rem
      IL_0003:  ldc.i4.0
      IL_0004:  ceq
      IL_0006:  ret
    } // end of method 'clo@225-2'::Invoke

  } // end of class 'clo@225-2'

  .class interface abstract auto ansi serializable nested public InterfaceTailCallLoopGenericInterface`1<T1>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
    .method public hidebysig abstract virtual
            instance class [mscorlib]System.Tuple`2<!T1,!!T2>
            Method1<T2>(int32 A_1,
                        !!T2 A_2) cil managed
    {
    } // end of method InterfaceTailCallLoopGenericInterface`1::Method1

    .method public hidebysig abstract virtual
            instance class [mscorlib]System.Tuple`2<!T1,!!T2>
            Method2<T2>(int32 A_1,
                        !!T2 A_2) cil managed
    {
    } // end of method InterfaceTailCallLoopGenericInterface`1::Method2

  } // end of class InterfaceTailCallLoopGenericInterface`1

  .class auto ansi serializable nested public StaticTailCallLoop`1<T1>
         extends [mscorlib]System.Object
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
    .field static assembly int32 v5
    .field static assembly int32 init@211
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       9 (0x9)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ldarg.0
      IL_0007:  pop
      IL_0008:  ret
    } // end of method StaticTailCallLoop`1::.ctor

    .method private specialname rtspecialname static
            void  .cctor() cil managed
    {
      // Code size       24 (0x18)
      .maxstack  8
      IL_0000:  call       int32 Fsharp::get_HugeInt()
      IL_0005:  call       int32 class Fsharp/StaticTailCallLoop`1<!T1>::f(int32)
      IL_000a:  stsfld     int32 class Fsharp/StaticTailCallLoop`1<!T1>::v5
      IL_000f:  ldc.i4.2
      IL_0010:  volatile.
      IL_0012:  stsfld     int32 class Fsharp/StaticTailCallLoop`1<!T1>::init@211
      IL_0017:  ret
    } // end of method StaticTailCallLoop`1::.cctor

    .method public specialname static int32
            get_Result() cil managed
    {
      // Code size       38 (0x26)
      .maxstack  8
      IL_0000:  call       int32 Fsharp::get_HugeInt()
      IL_0005:  call       int32 class Fsharp/StaticTailCallLoop`1<!T1>::f(int32)
      IL_000a:  volatile.
      IL_000c:  ldsfld     int32 class Fsharp/StaticTailCallLoop`1<!T1>::init@211
      IL_0011:  ldc.i4.2
      IL_0012:  bge.s      IL_0016

      IL_0014:  br.s       IL_0018

      IL_0016:  br.s       IL_001f

      IL_0018:  call       void Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::FailStaticInit()
      IL_001d:  br.s       IL_001f

      IL_001f:  ldsfld     int32 class Fsharp/StaticTailCallLoop`1<!T1>::v5
      IL_0024:  add
      IL_0025:  ret
    } // end of method StaticTailCallLoop`1::get_Result

    .method assembly static int32  f(int32 x) cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      // Code size       21 (0x15)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  brtrue.s   IL_0005

      IL_0003:  br.s       IL_0007

      IL_0005:  br.s       IL_000e

      IL_0007:  sizeof     !T1
      IL_000d:  ret

      IL_000e:  ldarg.0
      IL_000f:  ldc.i4.1
      IL_0010:  sub
      IL_0011:  starg.s    x
      IL_0013:  br.s       IL_0000
    } // end of method StaticTailCallLoop`1::f

    .property int32 Result()
    {
      .get int32 Fsharp/StaticTailCallLoop`1::get_Result()
    } // end of property StaticTailCallLoop`1::Result
  } // end of class StaticTailCallLoop`1

  .class auto ansi serializable nested public TailCallLoop`1<T1>
         extends [mscorlib]System.Object
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
    .field assembly int32 v5
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       26 (0x1a)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ldarg.0
      IL_0007:  pop
      IL_0008:  ldarg.0
      IL_0009:  ldarg.0
      IL_000a:  ldc.i4     0x989680
      IL_000f:  callvirt   instance int32 class Fsharp/TailCallLoop`1<!T1>::f(int32)
      IL_0014:  stfld      int32 class Fsharp/TailCallLoop`1<!T1>::v5
      IL_0019:  ret
    } // end of method TailCallLoop`1::.ctor

    .method public hidebysig specialname
            instance int32  get_Result() cil managed
    {
      // Code size       19 (0x13)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldc.i4     0x989680
      IL_0006:  callvirt   instance int32 class Fsharp/TailCallLoop`1<!T1>::f(int32)
      IL_000b:  ldarg.0
      IL_000c:  ldfld      int32 class Fsharp/TailCallLoop`1<!T1>::v5
      IL_0011:  add
      IL_0012:  ret
    } // end of method TailCallLoop`1::get_Result

    .method assembly hidebysig instance int32
            f(int32 x) cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      // Code size       23 (0x17)
      .maxstack  2
      .locals init (class Fsharp/TailCallLoop`1<!T1> V_0,
               int32 V_1)
      IL_0000:  ldarg.1
      IL_0001:  brtrue.s   IL_0005

      IL_0003:  br.s       IL_0007

      IL_0005:  br.s       IL_0009

      IL_0007:  ldc.i4.1
      IL_0008:  ret

      IL_0009:  ldarg.0
      IL_000a:  stloc.0
      IL_000b:  ldarg.1
      IL_000c:  ldc.i4.1
      IL_000d:  sub
      IL_000e:  stloc.1
      IL_000f:  ldloc.0
      IL_0010:  ldloc.1
      IL_0011:  starg.s    x
      IL_0013:  starg.s    0
      IL_0015:  br.s       IL_0000
    } // end of method TailCallLoop`1::f

    .property instance int32 Result()
    {
      .get instance int32 Fsharp/TailCallLoop`1::get_Result()
    } // end of property TailCallLoop`1::Result
  } // end of class TailCallLoop`1

  .class auto ansi serializable nested public TailCallLoopGenericClass`1<T1>
         extends [mscorlib]System.Object
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
    .field assembly !T1 resultA
    .method public specialname rtspecialname
            instance void  .ctor(!T1 resultA) cil managed
    {
      // Code size       16 (0x10)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ldarg.0
      IL_0007:  pop
      IL_0008:  ldarg.0
      IL_0009:  ldarg.1
      IL_000a:  stfld      !0 class Fsharp/TailCallLoopGenericClass`1<!T1>::resultA
      IL_000f:  ret
    } // end of method TailCallLoopGenericClass`1::.ctor

    .method public hidebysig instance !T1
            Method1(int32 x) cil managed
    {
      // Code size       26 (0x1a)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  brtrue.s   IL_0005

      IL_0003:  br.s       IL_0007

      IL_0005:  br.s       IL_000e

      IL_0007:  ldarg.0
      IL_0008:  ldfld      !0 class Fsharp/TailCallLoopGenericClass`1<!T1>::resultA
      IL_000d:  ret

      IL_000e:  ldarg.0
      IL_000f:  ldarg.1
      IL_0010:  ldc.i4.1
      IL_0011:  sub
      IL_0012:  tail.
      IL_0014:  callvirt   instance !0 class Fsharp/TailCallLoopGenericClass`1<!T1>::Method2(int32)
      IL_0019:  ret
    } // end of method TailCallLoopGenericClass`1::Method1

    .method public hidebysig instance !T1
            Method2(int32 x) cil managed
    {
      // Code size       26 (0x1a)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  brtrue.s   IL_0005

      IL_0003:  br.s       IL_0007

      IL_0005:  br.s       IL_000e

      IL_0007:  ldarg.0
      IL_0008:  ldfld      !0 class Fsharp/TailCallLoopGenericClass`1<!T1>::resultA
      IL_000d:  ret

      IL_000e:  ldarg.0
      IL_000f:  ldarg.1
      IL_0010:  ldc.i4.1
      IL_0011:  sub
      IL_0012:  tail.
      IL_0014:  callvirt   instance !0 class Fsharp/TailCallLoopGenericClass`1<!T1>::Method1(int32)
      IL_0019:  ret
    } // end of method TailCallLoopGenericClass`1::Method2

    .method public hidebysig specialname
            instance !T1  get_Result() cil managed
    {
      // Code size       14 (0xe)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldc.i4     0x989680
      IL_0006:  tail.
      IL_0008:  callvirt   instance !0 class Fsharp/TailCallLoopGenericClass`1<!T1>::Method1(int32)
      IL_000d:  ret
    } // end of method TailCallLoopGenericClass`1::get_Result

    .property instance !T1 Result()
    {
      .get instance !T1 Fsharp/TailCallLoopGenericClass`1::get_Result()
    } // end of property TailCallLoopGenericClass`1::Result
  } // end of class TailCallLoopGenericClass`1

  .class auto ansi serializable nested public TailCallLoopGenericClassAndMethod`1<T1>
         extends [mscorlib]System.Object
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
    .field assembly !T1 resultA
    .method public specialname rtspecialname
            instance void  .ctor(!T1 resultA) cil managed
    {
      // Code size       16 (0x10)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ldarg.0
      IL_0007:  pop
      IL_0008:  ldarg.0
      IL_0009:  ldarg.1
      IL_000a:  stfld      !0 class Fsharp/TailCallLoopGenericClassAndMethod`1<!T1>::resultA
      IL_000f:  ret
    } // end of method TailCallLoopGenericClassAndMethod`1::.ctor

    .method public hidebysig instance class [mscorlib]System.Tuple`2<!T1,!!T2>
            Method1<T2>(int32 x,
                        !!T2 resultB) cil managed
    {
      // Code size       33 (0x21)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  brtrue.s   IL_0005

      IL_0003:  br.s       IL_0007

      IL_0005:  br.s       IL_0014

      IL_0007:  ldarg.0
      IL_0008:  ldfld      !0 class Fsharp/TailCallLoopGenericClassAndMethod`1<!T1>::resultA
      IL_000d:  ldarg.2
      IL_000e:  newobj     instance void class [mscorlib]System.Tuple`2<!T1,!!T2>::.ctor(!0,
                                                                                         !1)
      IL_0013:  ret

      IL_0014:  ldarg.0
      IL_0015:  ldarg.1
      IL_0016:  ldc.i4.1
      IL_0017:  sub
      IL_0018:  ldarg.2
      IL_0019:  tail.
      IL_001b:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<!T1>::Method2<!!0>(int32,
                                                                                                                                               !!0)
      IL_0020:  ret
    } // end of method TailCallLoopGenericClassAndMethod`1::Method1

    .method public hidebysig instance class [mscorlib]System.Tuple`2<!T1,!!T2>
            Method2<T2>(int32 x,
                        !!T2 resultB) cil managed
    {
      // Code size       33 (0x21)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  brtrue.s   IL_0005

      IL_0003:  br.s       IL_0007

      IL_0005:  br.s       IL_0014

      IL_0007:  ldarg.0
      IL_0008:  ldfld      !0 class Fsharp/TailCallLoopGenericClassAndMethod`1<!T1>::resultA
      IL_000d:  ldarg.2
      IL_000e:  newobj     instance void class [mscorlib]System.Tuple`2<!T1,!!T2>::.ctor(!0,
                                                                                         !1)
      IL_0013:  ret

      IL_0014:  ldarg.0
      IL_0015:  ldarg.1
      IL_0016:  ldc.i4.1
      IL_0017:  sub
      IL_0018:  ldarg.2
      IL_0019:  tail.
      IL_001b:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/TailCallLoopGenericClassAndMethod`1<!T1>::Method1<!!0>(int32,
                                                                                                                                               !!0)
      IL_0020:  ret
    } // end of method TailCallLoopGenericClassAndMethod`1::Method2

  } // end of class TailCallLoopGenericClassAndMethod`1

  .class auto ansi serializable nested public TailCallLoopGenericClassAndMethodAbstractClass`1<T1>
         extends class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<!T1>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
    .field assembly !T1 resultA
    .method public specialname rtspecialname
            instance void  .ctor(!T1 resultA) cil managed
    {
      // Code size       17 (0x11)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldarg.1
      IL_0002:  callvirt   instance void class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<!T1>::.ctor(!0)
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ldarg.0
      IL_000a:  ldarg.1
      IL_000b:  stfld      !0 class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<!T1>::resultA
      IL_0010:  ret
    } // end of method TailCallLoopGenericClassAndMethodAbstractClass`1::.ctor

    .method public hidebysig virtual instance class [mscorlib]System.Tuple`2<!T1,!!T2>
            Method1<T2>(int32 x,
                        !!T2 resultB) cil managed
    {
      // Code size       33 (0x21)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  brtrue.s   IL_0005

      IL_0003:  br.s       IL_0007

      IL_0005:  br.s       IL_0014

      IL_0007:  ldarg.0
      IL_0008:  ldfld      !0 class Fsharp/TailCallLoopGenericClassAndMethodAbstractClass`1<!T1>::resultA
      IL_000d:  ldarg.2
      IL_000e:  newobj     instance void class [mscorlib]System.Tuple`2<!T1,!!T2>::.ctor(!0,
                                                                                         !1)
      IL_0013:  ret

      IL_0014:  ldarg.0
      IL_0015:  ldarg.1
      IL_0016:  ldc.i4.1
      IL_0017:  sub
      IL_0018:  ldarg.2
      IL_0019:  tail.
      IL_001b:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/AbstractTailCallLoopGenericClassAndMethod`1<!T1>::Method2<!!0>(int32,
                                                                                                                                                       !!0)
      IL_0020:  ret
    } // end of method TailCallLoopGenericClassAndMethodAbstractClass`1::Method1

  } // end of class TailCallLoopGenericClassAndMethodAbstractClass`1

  .class auto ansi serializable nested public TailCallLoopImplementGenericInterface`1<T1>
         extends [mscorlib]System.Object
         implements class Fsharp/InterfaceTailCallLoopGenericInterface`1<!T1>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
    .field assembly !T1 resultA
    .method public specialname rtspecialname
            instance void  .ctor(!T1 resultA) cil managed
    {
      // Code size       16 (0x10)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ldarg.0
      IL_0007:  pop
      IL_0008:  ldarg.0
      IL_0009:  ldarg.1
      IL_000a:  stfld      !0 class Fsharp/TailCallLoopImplementGenericInterface`1<!T1>::resultA
      IL_000f:  ret
    } // end of method TailCallLoopImplementGenericInterface`1::.ctor

    .method private hidebysig newslot virtual
            instance class [mscorlib]System.Tuple`2<!T1,!!T2>
            'Fsharp-InterfaceTailCallLoopGenericInterface`1-Method1'<T2>(int32 x,
                                                                         !!T2 resultB) cil managed
    {
      .override  method instance class [mscorlib]System.Tuple`2<!0,!!T2> class Fsharp/InterfaceTailCallLoopGenericInterface`1<!T1>::Method1<[1]>(int32,
                                                                                                                                            !!T2)
      // Code size       33 (0x21)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  brtrue.s   IL_0005

      IL_0003:  br.s       IL_0007

      IL_0005:  br.s       IL_0014

      IL_0007:  ldarg.0
      IL_0008:  ldfld      !0 class Fsharp/TailCallLoopImplementGenericInterface`1<!T1>::resultA
      IL_000d:  ldarg.2
      IL_000e:  newobj     instance void class [mscorlib]System.Tuple`2<!T1,!!T2>::.ctor(!0,
                                                                                         !1)
      IL_0013:  ret

      IL_0014:  ldarg.0
      IL_0015:  ldarg.1
      IL_0016:  ldc.i4.1
      IL_0017:  sub
      IL_0018:  ldarg.2
      IL_0019:  tail.
      IL_001b:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<!T1>::Method2<!!0>(int32,
                                                                                                                                                   !!0)
      IL_0020:  ret
    } // end of method TailCallLoopImplementGenericInterface`1::'Fsharp-InterfaceTailCallLoopGenericInterface`1-Method1'

    .method private hidebysig newslot virtual
            instance class [mscorlib]System.Tuple`2<!T1,!!T2>
            'Fsharp-InterfaceTailCallLoopGenericInterface`1-Method2'<T2>(int32 x,
                                                                         !!T2 resultB) cil managed
    {
      .override  method instance class [mscorlib]System.Tuple`2<!0,!!T2> class Fsharp/InterfaceTailCallLoopGenericInterface`1<!T1>::Method2<[1]>(int32,
                                                                                                                                            !!T2)
      // Code size       33 (0x21)
      .maxstack  8
      IL_0000:  ldarg.1
      IL_0001:  brtrue.s   IL_0005

      IL_0003:  br.s       IL_0007

      IL_0005:  br.s       IL_0014

      IL_0007:  ldarg.0
      IL_0008:  ldfld      !0 class Fsharp/TailCallLoopImplementGenericInterface`1<!T1>::resultA
      IL_000d:  ldarg.2
      IL_000e:  newobj     instance void class [mscorlib]System.Tuple`2<!T1,!!T2>::.ctor(!0,
                                                                                         !1)
      IL_0013:  ret

      IL_0014:  ldarg.0
      IL_0015:  ldarg.1
      IL_0016:  ldc.i4.1
      IL_0017:  sub
      IL_0018:  ldarg.2
      IL_0019:  tail.
      IL_001b:  callvirt   instance class [mscorlib]System.Tuple`2<!0,!!0> class Fsharp/InterfaceTailCallLoopGenericInterface`1<!T1>::Method1<!!0>(int32,
                                                                                                                                                   !!0)
      IL_0020:  ret
    } // end of method TailCallLoopImplementGenericInterface`1::'Fsharp-InterfaceTailCallLoopGenericInterface`1-Method2'

  } // end of class TailCallLoopImplementGenericInterface`1

  .method public specialname static int32
          get_HugeInt() cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       6 (0x6)
    .maxstack  8
    IL_0000:  ldc.i4     0x989680
    IL_0005:  ret
  } // end of method Fsharp::get_HugeInt

  .method public static int32  simpleTail1(int32 x,
                                           int32 acc) cil managed
  {
    .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
    // Code size       21 (0x15)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  brtrue.s   IL_0005

    IL_0003:  br.s       IL_0007

    IL_0005:  br.s       IL_0009

    IL_0007:  ldarg.1
    IL_0008:  ret

    IL_0009:  ldarg.0
    IL_000a:  ldc.i4.1
    IL_000b:  sub
    IL_000c:  ldarg.1
    IL_000d:  ldc.i4.1
    IL_000e:  add
    IL_000f:  starg.s    acc
    IL_0011:  starg.s    x
    IL_0013:  br.s       IL_0000
  } // end of method Fsharp::simpleTail1

  .method public static void  RunTest<a>(string msg,
                                         !!a actual,
                                         !!a expected) cil managed
  {
    .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 03 00 00 00 01 00 00 00 01 00 00 00 01 00
                                                                                                       00 00 00 00 )
    // Code size       68 (0x44)
    .maxstack  2
    .locals init (!!a V_0,
             !!a V_1)
    IL_0000:  ldarg.1
    IL_0001:  stloc.0
    IL_0002:  ldarg.2
    IL_0003:  stloc.1
    IL_0004:  ldloc.0
    IL_0005:  ldloc.1
    IL_0006:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityIntrinsic<!!0>(!!0,
                                                                                                                  !!0)
    IL_000b:  brfalse.s  IL_000f

    IL_000d:  br.s       IL_0011

    IL_000f:  br.s       IL_0022

    IL_0011:  ldarg.0
    IL_0012:  ldstr      ": YES"
    IL_0017:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_001c:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_0021:  ret

    IL_0022:  ldstr      "\n***** "
    IL_0027:  ldarg.0
    IL_0028:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_002d:  ldstr      ": FAIL\n"
    IL_0032:  call       string [mscorlib]System.String::Concat(string,
                                                                string)
    IL_0037:  call       void [mscorlib]System.Console::WriteLine(string)
    IL_003c:  ldc.i4.1
    IL_003d:  call       !!0 Microsoft.FSharp.Core.Operators::Exit<class Microsoft.FSharp.Core.Unit>(int32)
    IL_0042:  pop
    IL_0043:  ret
  } // end of method Fsharp::RunTest

  .method public static int32  simpleTail2(int32 acc,
                                           int32 _arg1) cil managed
  {
    .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
    // Code size       30 (0x1e)
    .maxstack  3
    .locals init (int32 V_0,
             int32 V_1)
    IL_0000:  ldarg.1
    IL_0001:  stloc.0
    IL_0002:  ldloc.0
    IL_0003:  switch     (
                          IL_000e)
    IL_000c:  br.s       IL_0010

    IL_000e:  ldarg.0
    IL_000f:  ret

    IL_0010:  ldloc.0
    IL_0011:  stloc.1
    IL_0012:  ldarg.0
    IL_0013:  ldc.i4.1
    IL_0014:  add
    IL_0015:  ldloc.1
    IL_0016:  ldc.i4.1
    IL_0017:  sub
    IL_0018:  starg.s    _arg1
    IL_001a:  starg.s    acc
    IL_001c:  br.s       IL_0000
  } // end of method Fsharp::simpleTail2

  .method public static bool  mutualTail1IsOdd(int32 x) cil managed
  {
    // Code size       31 (0x1f)
    .maxstack  2
    .locals init (int32 V_0,
             int32 V_1)
    IL_0000:  ldarg.0
    IL_0001:  stloc.0
    IL_0002:  ldloc.0
    IL_0003:  ldc.i4.1
    IL_0004:  sub
    IL_0005:  switch     (
                          IL_0010)
    IL_000e:  br.s       IL_0012

    IL_0010:  ldc.i4.1
    IL_0011:  ret

    IL_0012:  ldloc.0
    IL_0013:  stloc.1
    IL_0014:  ldarg.0
    IL_0015:  ldc.i4.1
    IL_0016:  sub
    IL_0017:  tail.
    IL_0019:  call       bool Fsharp::mutualTail1IsEven(int32)
    IL_001e:  ret
  } // end of method Fsharp::mutualTail1IsOdd

  .method public static bool  mutualTail1IsEven(int32 x) cil managed
  {
    // Code size       37 (0x25)
    .maxstack  2
    .locals init (int32 V_0,
             int32 V_1)
    IL_0000:  ldarg.0
    IL_0001:  stloc.0
    IL_0002:  ldloc.0
    IL_0003:  switch     (
                          IL_0012,
                          IL_0014)
    IL_0010:  br.s       IL_0018

    IL_0012:  br.s       IL_0016

    IL_0014:  ldc.i4.0
    IL_0015:  ret

    IL_0016:  ldc.i4.1
    IL_0017:  ret

    IL_0018:  ldloc.0
    IL_0019:  stloc.1
    IL_001a:  ldarg.0
    IL_001b:  ldc.i4.1
    IL_001c:  sub
    IL_001d:  tail.
    IL_001f:  call       bool Fsharp::mutualTail1IsOdd(int32)
    IL_0024:  ret
  } // end of method Fsharp::mutualTail1IsEven

  .method public static class Fsharp/InterfaceTailCallLoopGenericInterface`1<!!T>
          create<T>(!!T result) cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  newobj     instance void class Fsharp/TailCallLoopImplementGenericInterface`1<!!T>::.ctor(!0)
    IL_0006:  ret
  } // end of method Fsharp::create

  .property int32 HugeInt()
  {
    .get int32 Fsharp::get_HugeInt()
  } // end of property Fsharp::HugeInt
} // end of class Fsharp

.class public abstract auto ansi sealed Microsoft.FSharp.Core.LanguagePrimitives
       extends [mscorlib]System.Object
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 07 00 00 00 00 00 )
  .class auto autochar serializable sealed nested assembly beforefieldinit specialname BoolIEquality@2220
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<bool>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method BoolIEquality@2220::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(bool obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<bool>::GetHashCode(!0)
      // Code size       3 (0x3)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ret
    } // end of method BoolIEquality@2220::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(bool x,
                                                                                   bool y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<bool>::Equals(!0,
                                                                                                                   !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method BoolIEquality@2220::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class BoolIEquality@2220

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname ByteComparer@2306
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<uint8>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method ByteComparer@2306::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(uint8 x,
                                                                             uint8 y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<uint8>::Compare(!0,
                                                                                                              !0)
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  bge.un.s   IL_0007

      IL_0005:  ldc.i4.m1
      IL_0006:  ret

      IL_0007:  ldarg.1
      IL_0008:  ldarg.2
      IL_0009:  cgt.un
      IL_000b:  ret
    } // end of method ByteComparer@2306::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class ByteComparer@2306

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname ByteIEquality@2228
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint8>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method ByteIEquality@2228::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(uint8 obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint8>::GetHashCode(!0)
      // Code size       3 (0x3)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ret
    } // end of method ByteIEquality@2228::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(uint8 x,
                                                                                   uint8 y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint8>::Equals(!0,
                                                                                                                    !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method ByteIEquality@2228::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class ByteIEquality@2228

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname CharComparer@2299
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<char>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method CharComparer@2299::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(char x,
                                                                             char y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<char>::Compare(!0,
                                                                                                             !0)
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  bge.un.s   IL_0007

      IL_0005:  ldc.i4.m1
      IL_0006:  ret

      IL_0007:  ldarg.1
      IL_0008:  ldarg.2
      IL_0009:  cgt.un
      IL_000b:  ret
    } // end of method CharComparer@2299::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class CharComparer@2299

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname CharIEquality@2221
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<char>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method CharIEquality@2221::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(char obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<char>::GetHashCode(!0)
      // Code size       8 (0x8)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldc.i4.s   16
      IL_0004:  shl
      IL_0005:  ldarg.1
      IL_0006:  or
      IL_0007:  ret
    } // end of method CharIEquality@2221::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(char x,
                                                                                   char y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<char>::Equals(!0,
                                                                                                                   !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method CharIEquality@2221::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class CharIEquality@2221

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname DecimalComparer@2313
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<valuetype [mscorlib]System.Decimal>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method DecimalComparer@2313::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(valuetype [mscorlib]System.Decimal x,
                                                                             valuetype [mscorlib]System.Decimal y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<valuetype [mscorlib]System.Decimal>::Compare(!0,
                                                                                                                                           !0)
      // Code size       9 (0x9)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  call       int32 [mscorlib]System.Decimal::Compare(valuetype [mscorlib]System.Decimal,
                                                                   valuetype [mscorlib]System.Decimal)
      IL_0008:  ret
    } // end of method DecimalComparer@2313::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class DecimalComparer@2313

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname DecimalIEquality@2235
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<valuetype [mscorlib]System.Decimal>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method DecimalIEquality@2235::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(valuetype [mscorlib]System.Decimal obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<valuetype [mscorlib]System.Decimal>::GetHashCode(!0)
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  tail.
      IL_0004:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashIntrinsic<valuetype [mscorlib]System.Decimal>(!!0)
      IL_0009:  ret
    } // end of method DecimalIEquality@2235::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(valuetype [mscorlib]System.Decimal x,
                                                                                   valuetype [mscorlib]System.Decimal y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<valuetype [mscorlib]System.Decimal>::Equals(!0,
                                                                                                                                                 !0)
      // Code size       9 (0x9)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  call       bool [mscorlib]System.Decimal::op_Equality(valuetype [mscorlib]System.Decimal,
                                                                      valuetype [mscorlib]System.Decimal)
      IL_0008:  ret
    } // end of method DecimalIEquality@2235::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class DecimalIEquality@2235

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname Float32Comparer@2312
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<float32>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method Float32Comparer@2312::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(float32 x,
                                                                             float32 y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<float32>::Compare(!0,
                                                                                                                !0)
      // Code size       38 (0x26)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  clt
      IL_0005:  brfalse.s  IL_0009

      IL_0007:  ldc.i4.m1
      IL_0008:  ret

      IL_0009:  ldarg.1
      IL_000a:  ldarg.2
      IL_000b:  cgt
      IL_000d:  brfalse.s  IL_0011

      IL_000f:  ldc.i4.1
      IL_0010:  ret

      IL_0011:  ldarg.1
      IL_0012:  ldarg.2
      IL_0013:  ceq
      IL_0015:  brfalse.s  IL_0019

      IL_0017:  ldc.i4.0
      IL_0018:  ret

      IL_0019:  ldarg.2
      IL_001a:  ldarg.2
      IL_001b:  ceq
      IL_001d:  brfalse.s  IL_0021

      IL_001f:  ldc.i4.m1
      IL_0020:  ret

      IL_0021:  ldarg.1
      IL_0022:  ldarg.1
      IL_0023:  ceq
      IL_0025:  ret
    } // end of method Float32Comparer@2312::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class Float32Comparer@2312

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname Float32IEquality@2234
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<float32>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method Float32IEquality@2234::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(float32 obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<float32>::GetHashCode(!0)
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  tail.
      IL_0004:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashIntrinsic<float32>(!!0)
      IL_0009:  ret
    } // end of method Float32IEquality@2234::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(float32 x,
                                                                                   float32 y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<float32>::Equals(!0,
                                                                                                                      !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method Float32IEquality@2234::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class Float32IEquality@2234

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname FloatComparer@2311
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<float64>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method FloatComparer@2311::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(float64 x,
                                                                             float64 y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<float64>::Compare(!0,
                                                                                                                !0)
      // Code size       38 (0x26)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  clt
      IL_0005:  brfalse.s  IL_0009

      IL_0007:  ldc.i4.m1
      IL_0008:  ret

      IL_0009:  ldarg.1
      IL_000a:  ldarg.2
      IL_000b:  cgt
      IL_000d:  brfalse.s  IL_0011

      IL_000f:  ldc.i4.1
      IL_0010:  ret

      IL_0011:  ldarg.1
      IL_0012:  ldarg.2
      IL_0013:  ceq
      IL_0015:  brfalse.s  IL_0019

      IL_0017:  ldc.i4.0
      IL_0018:  ret

      IL_0019:  ldarg.2
      IL_001a:  ldarg.2
      IL_001b:  ceq
      IL_001d:  brfalse.s  IL_0021

      IL_001f:  ldc.i4.m1
      IL_0020:  ret

      IL_0021:  ldarg.1
      IL_0022:  ldarg.1
      IL_0023:  ceq
      IL_0025:  ret
    } // end of method FloatComparer@2311::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class FloatComparer@2311

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname FloatIEquality@2233
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<float64>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method FloatIEquality@2233::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(float64 obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<float64>::GetHashCode(!0)
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  tail.
      IL_0004:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashIntrinsic<float64>(!!0)
      IL_0009:  ret
    } // end of method FloatIEquality@2233::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(float64 x,
                                                                                   float64 y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<float64>::Equals(!0,
                                                                                                                      !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method FloatIEquality@2233::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class FloatIEquality@2233

  .class abstract auto ansi sealed nested public HashCompare
         extends [mscorlib]System.Object
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 07 00 00 00 00 00 )
    .class auto autochar serializable sealed nested assembly beforefieldinit specialname fsEqualityComparerNoHashingER@1662
           extends [mscorlib]System.Object
           implements [mscorlib]System.Collections.IEqualityComparer
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
      .method public specialname rtspecialname
              instance void  .ctor() cil managed
      {
        // Code size       10 (0xa)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
        IL_0007:  ldarg.0
        IL_0008:  pop
        IL_0009:  ret
      } // end of method fsEqualityComparerNoHashingER@1662::.ctor

      .method private hidebysig newslot virtual final
              instance bool  'System-Collections-IEqualityComparer-Equals'(object x,
                                                                           object y) cil managed
      {
        .override [mscorlib]System.Collections.IEqualityComparer::Equals
        // Code size       13 (0xd)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldc.i4.1
        IL_0002:  ldarg.0
        IL_0003:  ldarg.1
        IL_0004:  ldarg.2
        IL_0005:  tail.
        IL_0007:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityObj(bool,
                                                                                                           class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                           object,
                                                                                                           object)
        IL_000c:  ret
      } // end of method fsEqualityComparerNoHashingER@1662::'System-Collections-IEqualityComparer-Equals'

      .method private hidebysig newslot virtual final
              instance int32  'System-Collections-IEqualityComparer-GetHashCode'(object obj) cil managed
      {
        .override [mscorlib]System.Collections.IEqualityComparer::GetHashCode
        // Code size       12 (0xc)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldstr      "notUsedForHashing"
        IL_0006:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor(string)
        IL_000b:  throw
      } // end of method fsEqualityComparerNoHashingER@1662::'System-Collections-IEqualityComparer-GetHashCode'

    } // end of class fsEqualityComparerNoHashingER@1662

    .class auto autochar serializable sealed nested assembly beforefieldinit specialname fsEqualityComparerNoHashingPER@1655
           extends [mscorlib]System.Object
           implements [mscorlib]System.Collections.IEqualityComparer
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
      .method public specialname rtspecialname
              instance void  .ctor() cil managed
      {
        // Code size       10 (0xa)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
        IL_0007:  ldarg.0
        IL_0008:  pop
        IL_0009:  ret
      } // end of method fsEqualityComparerNoHashingPER@1655::.ctor

      .method private hidebysig newslot virtual final
              instance bool  'System-Collections-IEqualityComparer-Equals'(object x,
                                                                           object y) cil managed
      {
        .override [mscorlib]System.Collections.IEqualityComparer::Equals
        // Code size       13 (0xd)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldc.i4.0
        IL_0002:  ldarg.0
        IL_0003:  ldarg.1
        IL_0004:  ldarg.2
        IL_0005:  tail.
        IL_0007:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityObj(bool,
                                                                                                           class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                           object,
                                                                                                           object)
        IL_000c:  ret
      } // end of method fsEqualityComparerNoHashingPER@1655::'System-Collections-IEqualityComparer-Equals'

      .method private hidebysig newslot virtual final
              instance int32  'System-Collections-IEqualityComparer-GetHashCode'(object obj) cil managed
      {
        .override [mscorlib]System.Collections.IEqualityComparer::GetHashCode
        // Code size       12 (0xc)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldstr      "notUsedForHashing"
        IL_0006:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor(string)
        IL_000b:  throw
      } // end of method fsEqualityComparerNoHashingPER@1655::'System-Collections-IEqualityComparer-GetHashCode'

    } // end of class fsEqualityComparerNoHashingPER@1655

    .class auto ansi serializable nested assembly GenericComparer
           extends [mscorlib]System.Object
           implements [mscorlib]System.Collections.IComparer
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
      .field assembly bool throwsOnPER
      .method public specialname rtspecialname
              instance void  .ctor(bool throwsOnPER) cil managed
      {
        // Code size       17 (0x11)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
        IL_0006:  ldarg.0
        IL_0007:  pop
        IL_0008:  nop
        IL_0009:  ldarg.0
        IL_000a:  ldarg.1
        IL_000b:  stfld      bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer::throwsOnPER
        IL_0010:  ret
      } // end of method GenericComparer::.ctor

      .method private hidebysig newslot virtual
              instance int32  'System-Collections-IComparer-Compare'(object x,
                                                                     object y) cil managed
      {
        .override [mscorlib]System.Collections.IComparer::Compare
        // Code size       12 (0xc)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldarg.1
        IL_0003:  ldarg.2
        IL_0004:  tail.
        IL_0006:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericCompare(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                        object,
                                                                                                        object)
        IL_000b:  ret
      } // end of method GenericComparer::'System-Collections-IComparer-Compare'

    } // end of class GenericComparer

    .class auto ansi serializable nested assembly UnlimitedHasherER
           extends [mscorlib]System.Object
           implements [mscorlib]System.Collections.IEqualityComparer
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
      .method public specialname rtspecialname
              instance void  .ctor() cil managed
      {
        // Code size       10 (0xa)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
        IL_0006:  ldarg.0
        IL_0007:  pop
        IL_0008:  nop
        IL_0009:  ret
      } // end of method UnlimitedHasherER::.ctor

      .method private hidebysig newslot virtual
              instance bool  'System-Collections-IEqualityComparer-Equals'(object x,
                                                                           object y) cil managed
      {
        .override [mscorlib]System.Collections.IEqualityComparer::Equals
        // Code size       13 (0xd)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldc.i4.1
        IL_0002:  ldarg.0
        IL_0003:  ldarg.1
        IL_0004:  ldarg.2
        IL_0005:  tail.
        IL_0007:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityObj(bool,
                                                                                                           class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                           object,
                                                                                                           object)
        IL_000c:  ret
      } // end of method UnlimitedHasherER::'System-Collections-IEqualityComparer-Equals'

      .method private hidebysig newslot virtual
              instance int32  'System-Collections-IEqualityComparer-GetHashCode'(object x) cil managed
      {
        .override [mscorlib]System.Collections.IEqualityComparer::GetHashCode
        // Code size       11 (0xb)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldarg.1
        IL_0003:  tail.
        IL_0005:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashParamObj(class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                             object)
        IL_000a:  ret
      } // end of method UnlimitedHasherER::'System-Collections-IEqualityComparer-GetHashCode'

    } // end of class UnlimitedHasherER

    .class auto ansi serializable nested assembly UnlimitedHasherPER
           extends [mscorlib]System.Object
           implements [mscorlib]System.Collections.IEqualityComparer
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
      .method public specialname rtspecialname
              instance void  .ctor() cil managed
      {
        // Code size       10 (0xa)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
        IL_0006:  ldarg.0
        IL_0007:  pop
        IL_0008:  nop
        IL_0009:  ret
      } // end of method UnlimitedHasherPER::.ctor

      .method private hidebysig newslot virtual
              instance bool  'System-Collections-IEqualityComparer-Equals'(object x,
                                                                           object y) cil managed
      {
        .override [mscorlib]System.Collections.IEqualityComparer::Equals
        // Code size       13 (0xd)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldc.i4.0
        IL_0002:  ldarg.0
        IL_0003:  ldarg.1
        IL_0004:  ldarg.2
        IL_0005:  tail.
        IL_0007:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityObj(bool,
                                                                                                           class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                           object,
                                                                                                           object)
        IL_000c:  ret
      } // end of method UnlimitedHasherPER::'System-Collections-IEqualityComparer-Equals'

      .method private hidebysig newslot virtual
              instance int32  'System-Collections-IEqualityComparer-GetHashCode'(object x) cil managed
      {
        .override [mscorlib]System.Collections.IEqualityComparer::GetHashCode
        // Code size       11 (0xb)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldarg.1
        IL_0003:  tail.
        IL_0005:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashParamObj(class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                             object)
        IL_000a:  ret
      } // end of method UnlimitedHasherPER::'System-Collections-IEqualityComparer-GetHashCode'

    } // end of class UnlimitedHasherPER

    .method assembly specialname static class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer
            get_fsComparerER() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  ldsfld     class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer '<StartupCode$FSharp-Core>'.'$Prim-types'::fsComparerER@1182
      IL_0005:  ret
    } // end of method HashCompare::get_fsComparerER

    .method assembly specialname static class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherPER
            get_fsEqualityComparerUnlimitedHashingPER() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  ldsfld     class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherPER '<StartupCode$FSharp-Core>'.'$Prim-types'::fsEqualityComparerUnlimitedHashingPER@1810
      IL_0005:  ret
    } // end of method HashCompare::get_fsEqualityComparerUnlimitedHashingPER

    .method assembly specialname static class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherER
            get_fsEqualityComparerUnlimitedHashingER() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  ldsfld     class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherER '<StartupCode$FSharp-Core>'.'$Prim-types'::fsEqualityComparerUnlimitedHashingER@1807
      IL_0005:  ret
    } // end of method HashCompare::get_fsEqualityComparerUnlimitedHashingER

    .method public static bool  GenericEqualityIntrinsic<T>(!!T x,
                                                            !!T y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
      .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                                  int32) = ( 01 00 66 54 68 69 73 20 66 75 6E 63 74 69 6F 6E   // ..fThis function
                                                                                             20 69 73 20 61 20 70 72 69 6D 69 74 69 76 65 20   //  is a primitive
                                                                                             6C 69 62 72 61 72 79 20 72 6F 75 74 69 6E 65 20   // library routine
                                                                                             75 73 65 64 20 62 79 20 6F 70 74 69 6D 69 7A 65   // used by optimize
                                                                                             64 20 46 23 20 63 6F 64 65 20 61 6E 64 20 73 68   // d F# code and sh
                                                                                             6F 75 6C 64 20 6E 6F 74 20 62 65 20 75 73 65 64   // ould not be used
                                                                                             20 64 69 72 65 63 74 6C 79 B4 04 00 00 01 00 54   //  directly......T
                                                                                             02 08 49 73 48 69 64 64 65 6E 01 )                // ..IsHidden.
      // Code size       27 (0x1b)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldc.i4.0
      IL_0002:  call       class [mscorlib]System.Collections.IEqualityComparer Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_fsEqualityComparerNoHashingPER()
      IL_0007:  ldarg.0
      IL_0008:  box        !!T
      IL_000d:  ldarg.1
      IL_000e:  box        !!T
      IL_0013:  tail.
      IL_0015:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityObj(bool,
                                                                                                         class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                         object,
                                                                                                         object)
      IL_001a:  ret
    } // end of method HashCompare::GenericEqualityIntrinsic

    .method public static int32  GenericHashIntrinsic<T>(!!T input) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                                  int32) = ( 01 00 66 54 68 69 73 20 66 75 6E 63 74 69 6F 6E   // ..fThis function
                                                                                             20 69 73 20 61 20 70 72 69 6D 69 74 69 76 65 20   //  is a primitive
                                                                                             6C 69 62 72 61 72 79 20 72 6F 75 74 69 6E 65 20   // library routine
                                                                                             75 73 65 64 20 62 79 20 6F 70 74 69 6D 69 7A 65   // used by optimize
                                                                                             64 20 46 23 20 63 6F 64 65 20 61 6E 64 20 73 68   // d F# code and sh
                                                                                             6F 75 6C 64 20 6E 6F 74 20 62 65 20 75 73 65 64   // ould not be used
                                                                                             20 64 69 72 65 63 74 6C 79 B4 04 00 00 01 00 54   //  directly......T
                                                                                             02 08 49 73 48 69 64 64 65 6E 01 )                // ..IsHidden.
      // Code size       20 (0x14)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  call       class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherPER Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_fsEqualityComparerUnlimitedHashingPER()
      IL_0006:  ldarg.0
      IL_0007:  box        !!T
      IL_000c:  tail.
      IL_000e:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashParamObj(class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                           object)
      IL_0013:  ret
    } // end of method HashCompare::GenericHashIntrinsic

    .method assembly static !!a  FailGenericComparison<a>(object obj) cil managed
    {
      // Code size       30 (0x1e)
      .maxstack  2
      .locals init (string V_0)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance class [mscorlib]System.Type [mscorlib]System.Object::GetType()
      IL_0007:  callvirt   instance string [mscorlib]System.Object::ToString()
      IL_000c:  stloc.0
      IL_000d:  ldstr      "genericCompareFail1"
      IL_0012:  ldloc.0
      IL_0013:  call       string [mscorlib]System.String::Format(string,
                                                                  object)
      IL_0018:  newobj     instance void [mscorlib]System.ArgumentException::.ctor(string)
      IL_001d:  throw
    } // end of method HashCompare::FailGenericComparison

    .method assembly specialname static class [mscorlib]System.Exception
            get_NaNException() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  ldsfld     class [mscorlib]System.Exception '<StartupCode$FSharp-Core>'.'$Prim-types'::NaNException@951
      IL_0005:  ret
    } // end of method HashCompare::get_NaNException

    .method assembly static int32  GenericCompare$cont@965(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer comp,
                                                           object yobj,
                                                           object xobj,
                                                           class [mscorlib]System.Array arr1,
                                                           class Microsoft.FSharp.Core.Unit unitVar) cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      // Code size       203 (0xcb)
      .maxstack  3
      .locals init (object[] V_0,
               object[] V_1,
               object[] V_2,
               object[] V_3,
               uint8[] V_4,
               uint8[] V_5,
               uint8[] V_6,
               uint8[] V_7,
               class [mscorlib]System.Array V_8,
               class [mscorlib]System.Array V_9)
      IL_0000:  ldarg.3
      IL_0001:  isinst     object[]
      IL_0006:  stloc.0
      IL_0007:  ldloc.0
      IL_0008:  brfalse    IL_0082

      IL_000d:  ldarg.1
      IL_000e:  isinst     object[]
      IL_0013:  stloc.1
      IL_0014:  ldloc.1
      IL_0015:  brfalse.s  IL_0025

      IL_0017:  ldloc.1
      IL_0018:  stloc.2
      IL_0019:  ldloc.0
      IL_001a:  stloc.3
      IL_001b:  nop
      IL_001c:  ldarg.0
      IL_001d:  ldloc.3
      IL_001e:  ldloc.2
      IL_001f:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericComparisonObjArrayWithComparer(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                                             object[],
                                                                                                                             object[])
      IL_0024:  ret

      IL_0025:  ldarg.3
      IL_0026:  isinst     uint8[]
      IL_002b:  stloc.s    V_4
      IL_002d:  ldloc.s    V_4
      IL_002f:  brfalse.s  IL_0070

      IL_0031:  ldarg.1
      IL_0032:  isinst     uint8[]
      IL_0037:  stloc.s    V_5
      IL_0039:  ldloc.s    V_5
      IL_003b:  brfalse.s  IL_004f

      IL_003d:  ldloc.s    V_5
      IL_003f:  stloc.s    V_6
      IL_0041:  ldloc.s    V_4
      IL_0043:  stloc.s    V_7
      IL_0045:  ldloc.s    V_7
      IL_0047:  ldloc.s    V_6
      IL_0049:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericComparisonByteArray(uint8[],
                                                                                                                  uint8[])
      IL_004e:  ret

      IL_004f:  ldarg.1
      IL_0050:  isinst     [mscorlib]System.Array
      IL_0055:  stloc.s    V_8
      IL_0057:  ldloc.s    V_8
      IL_0059:  brfalse.s  IL_0069

      IL_005b:  ldloc.s    V_8
      IL_005d:  stloc.s    V_9
      IL_005f:  ldarg.0
      IL_0060:  ldarg.3
      IL_0061:  ldloc.s    V_9
      IL_0063:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericComparisonArbArrayWithComparer(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                                             class [mscorlib]System.Array,
                                                                                                                             class [mscorlib]System.Array)
      IL_0068:  ret

      IL_0069:  ldarg.2
      IL_006a:  call       !!0 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::FailGenericComparison<int32>(object)
      IL_006f:  ret

      IL_0070:  ldarg.1
      IL_0071:  isinst     [mscorlib]System.Array
      IL_0076:  stloc.s    V_8
      IL_0078:  ldloc.s    V_8
      IL_007a:  brfalse.s  IL_0069

      IL_007c:  ldloc.s    V_8
      IL_007e:  stloc.s    V_9
      IL_0080:  br.s       IL_005f

      IL_0082:  ldarg.3
      IL_0083:  isinst     uint8[]
      IL_0088:  stloc.s    V_4
      IL_008a:  ldloc.s    V_4
      IL_008c:  brfalse.s  IL_00b6

      IL_008e:  ldarg.1
      IL_008f:  isinst     uint8[]
      IL_0094:  stloc.s    V_5
      IL_0096:  ldloc.s    V_5
      IL_0098:  brfalse.s  IL_00a4

      IL_009a:  ldloc.s    V_5
      IL_009c:  ldloc.s    V_4
      IL_009e:  stloc.s    V_7
      IL_00a0:  stloc.s    V_6
      IL_00a2:  br.s       IL_0045

      IL_00a4:  ldarg.1
      IL_00a5:  isinst     [mscorlib]System.Array
      IL_00aa:  stloc.s    V_8
      IL_00ac:  ldloc.s    V_8
      IL_00ae:  brfalse.s  IL_0069

      IL_00b0:  ldloc.s    V_8
      IL_00b2:  stloc.s    V_9
      IL_00b4:  br.s       IL_005f

      IL_00b6:  ldarg.1
      IL_00b7:  isinst     [mscorlib]System.Array
      IL_00bc:  stloc.s    V_8
      IL_00be:  ldloc.s    V_8
      IL_00c0:  brfalse.s  IL_0069

      IL_00c2:  ldloc.s    V_8
      IL_00c4:  stloc.s    V_9
      IL_00c6:  br         IL_005f
    } // end of method HashCompare::GenericCompare$cont@965

    .method assembly static int32  GenericComparisonObjArrayWithComparer(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer comp,
                                                                         object[] x,
                                                                         object[] y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 03 00 00 00 01 00 00 00 01 00 00 00 01 00
                                                                                                         00 00 00 00 )
      // Code size       82 (0x52)
      .maxstack  4
      .locals init (int32 V_0,
               int32 V_1,
               int32 V_2,
               int32 V_3,
               int32 V_4,
               int32 V_5)
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldarg.2
      IL_0006:  ldlen
      IL_0007:  conv.i4
      IL_0008:  stloc.1
      IL_0009:  ldloc.0
      IL_000a:  ldloc.1
      IL_000b:  bge.s      IL_0011

      IL_000d:  ldc.i4.m1
      IL_000e:  nop
      IL_000f:  br.s       IL_0016

      IL_0011:  ldloc.0
      IL_0012:  ldloc.1
      IL_0013:  cgt
      IL_0015:  nop
      IL_0016:  stloc.2
      IL_0017:  ldloc.2
      IL_0018:  brfalse.s  IL_001c

      IL_001a:  ldloc.2
      IL_001b:  ret

      IL_001c:  ldc.i4.0
      IL_001d:  stloc.3
      IL_001e:  ldc.i4.0
      IL_001f:  stloc.s    V_4
      IL_0021:  ldloc.3
      IL_0022:  ldloc.0
      IL_0023:  bge.s      IL_004f

      IL_0025:  ldarg.0
      IL_0026:  ldarg.1
      IL_0027:  ldloc.3
      IL_0028:  ldelem     [mscorlib]System.Object
      IL_002d:  ldarg.2
      IL_002e:  ldloc.3
      IL_002f:  ldelem     [mscorlib]System.Object
      IL_0034:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericCompare(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                      object,
                                                                                                      object)
      IL_0039:  stloc.s    V_5
      IL_003b:  ldloc.s    V_5
      IL_003d:  brfalse.s  IL_0048

      IL_003f:  ldloc.s    V_5
      IL_0041:  stloc.s    V_4
      IL_0043:  ldloc.0
      IL_0044:  stloc.3
      IL_0045:  nop
      IL_0046:  br.s       IL_0021

      IL_0048:  ldloc.3
      IL_0049:  ldc.i4.1
      IL_004a:  add
      IL_004b:  stloc.3
      IL_004c:  nop
      IL_004d:  br.s       IL_0021

      IL_004f:  ldloc.s    V_4
      IL_0051:  ret
    } // end of method HashCompare::GenericComparisonObjArrayWithComparer

    .method assembly static int32  GenericComparisonByteArray(uint8[] x,
                                                              uint8[] y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
      // Code size       97 (0x61)
      .maxstack  2
      .locals init (int32 V_0,
               int32 V_1,
               int32 V_2,
               int32 V_3,
               int32 V_4,
               int32 V_5,
               uint8 V_6,
               uint8 V_7)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldarg.1
      IL_0006:  ldlen
      IL_0007:  conv.i4
      IL_0008:  stloc.1
      IL_0009:  ldloc.0
      IL_000a:  ldloc.1
      IL_000b:  bge.s      IL_0011

      IL_000d:  ldc.i4.m1
      IL_000e:  nop
      IL_000f:  br.s       IL_0016

      IL_0011:  ldloc.0
      IL_0012:  ldloc.1
      IL_0013:  cgt
      IL_0015:  nop
      IL_0016:  stloc.2
      IL_0017:  ldloc.2
      IL_0018:  brfalse.s  IL_001c

      IL_001a:  ldloc.2
      IL_001b:  ret

      IL_001c:  ldc.i4.0
      IL_001d:  stloc.3
      IL_001e:  ldc.i4.0
      IL_001f:  stloc.s    V_4
      IL_0021:  ldloc.3
      IL_0022:  ldloc.0
      IL_0023:  bge.s      IL_005e

      IL_0025:  ldarg.0
      IL_0026:  ldloc.3
      IL_0027:  ldelem     [mscorlib]System.Byte
      IL_002c:  stloc.s    V_6
      IL_002e:  ldarg.1
      IL_002f:  ldloc.3
      IL_0030:  ldelem     [mscorlib]System.Byte
      IL_0035:  stloc.s    V_7
      IL_0037:  ldloc.s    V_6
      IL_0039:  ldloc.s    V_7
      IL_003b:  bge.s      IL_0041

      IL_003d:  ldc.i4.m1
      IL_003e:  nop
      IL_003f:  br.s       IL_0048

      IL_0041:  ldloc.s    V_6
      IL_0043:  ldloc.s    V_7
      IL_0045:  cgt
      IL_0047:  nop
      IL_0048:  stloc.s    V_5
      IL_004a:  ldloc.s    V_5
      IL_004c:  brfalse.s  IL_0057

      IL_004e:  ldloc.s    V_5
      IL_0050:  stloc.s    V_4
      IL_0052:  ldloc.0
      IL_0053:  stloc.3
      IL_0054:  nop
      IL_0055:  br.s       IL_0021

      IL_0057:  ldloc.3
      IL_0058:  ldc.i4.1
      IL_0059:  add
      IL_005a:  stloc.3
      IL_005b:  nop
      IL_005c:  br.s       IL_0021

      IL_005e:  ldloc.s    V_4
      IL_0060:  ret
    } // end of method HashCompare::GenericComparisonByteArray

    .method assembly static int32  GenericComparisonArbArrayWithComparer(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer comp,
                                                                         class [mscorlib]System.Array x,
                                                                         class [mscorlib]System.Array y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 03 00 00 00 01 00 00 00 01 00 00 00 01 00
                                                                                                         00 00 00 00 )
      // Code size       517 (0x205)
      .maxstack  10
      .locals init (int64 V_0,
               int64 V_1,
               int32 V_2,
               int64 V_3,
               int64 V_4,
               int32 V_5,
               class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer V_6,
               class [mscorlib]System.Array V_7,
               class [mscorlib]System.Array V_8,
               int64 V_9,
               int64 V_10,
               int64 V_11,
               int64 V_12,
               int32 V_13,
               int32 V_14,
               int64 V_15,
               int64 V_16,
               int64 V_17,
               int64 V_18,
               int64 V_19,
               int64 V_20,
               int64[] V_21,
               class [mscorlib]System.Array V_22,
               class [mscorlib]System.Array V_23,
               int32 V_24,
               int64[] V_25)
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_0007:  ldc.i4.1
      IL_0008:  bne.un.s   IL_0016

      IL_000a:  ldarg.2
      IL_000b:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_0010:  ldc.i4.1
      IL_0011:  ceq
      IL_0013:  nop
      IL_0014:  br.s       IL_0018

      IL_0016:  ldc.i4.0
      IL_0017:  nop
      IL_0018:  brfalse    IL_0092

      IL_001d:  ldarg.1
      IL_001e:  callvirt   instance int64 [mscorlib]System.Array::get_LongLength()
      IL_0023:  stloc.0
      IL_0024:  ldarg.2
      IL_0025:  callvirt   instance int64 [mscorlib]System.Array::get_LongLength()
      IL_002a:  stloc.1
      IL_002b:  ldloc.0
      IL_002c:  ldloc.1
      IL_002d:  bge.s      IL_0033

      IL_002f:  ldc.i4.m1
      IL_0030:  nop
      IL_0031:  br.s       IL_0038

      IL_0033:  ldloc.0
      IL_0034:  ldloc.1
      IL_0035:  cgt
      IL_0037:  nop
      IL_0038:  stloc.2
      IL_0039:  ldloc.2
      IL_003a:  brfalse.s  IL_003e

      IL_003c:  ldloc.2
      IL_003d:  ret

      IL_003e:  ldarg.1
      IL_003f:  ldc.i4.0
      IL_0040:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_0045:  conv.i8
      IL_0046:  stloc.3
      IL_0047:  ldarg.2
      IL_0048:  ldc.i4.0
      IL_0049:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_004e:  conv.i8
      IL_004f:  stloc.s    V_4
      IL_0051:  ldloc.3
      IL_0052:  ldloc.s    V_4
      IL_0054:  bge.s      IL_005a

      IL_0056:  ldc.i4.m1
      IL_0057:  nop
      IL_0058:  br.s       IL_0060

      IL_005a:  ldloc.3
      IL_005b:  ldloc.s    V_4
      IL_005d:  cgt
      IL_005f:  nop
      IL_0060:  stloc.s    V_5
      IL_0062:  ldloc.s    V_5
      IL_0064:  brfalse.s  IL_0069

      IL_0066:  ldloc.s    V_5
      IL_0068:  ret

      IL_0069:  ldarg.0
      IL_006a:  stloc.s    V_6
      IL_006c:  ldarg.1
      IL_006d:  stloc.s    V_7
      IL_006f:  ldarg.2
      IL_0070:  stloc.s    V_8
      IL_0072:  ldloc.0
      IL_0073:  stloc.s    V_9
      IL_0075:  ldloc.3
      IL_0076:  stloc.s    V_10
      IL_0078:  ldloc.s    V_4
      IL_007a:  stloc.s    V_11
      IL_007c:  ldarg.0
      IL_007d:  ldarg.1
      IL_007e:  ldarg.2
      IL_007f:  ldloc.0
      IL_0080:  ldloc.3
      IL_0081:  ldloc.s    V_4
      IL_0083:  ldc.i8     0x0
      IL_008c:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::check@1081(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                  class [mscorlib]System.Array,
                                                                                                  class [mscorlib]System.Array,
                                                                                                  int64,
                                                                                                  int64,
                                                                                                  int64,
                                                                                                  int64)
      IL_0091:  ret

      IL_0092:  ldarg.1
      IL_0093:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_0098:  ldc.i4.2
      IL_0099:  bne.un.s   IL_00a7

      IL_009b:  ldarg.2
      IL_009c:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_00a1:  ldc.i4.2
      IL_00a2:  ceq
      IL_00a4:  nop
      IL_00a5:  br.s       IL_00a9

      IL_00a7:  ldc.i4.0
      IL_00a8:  nop
      IL_00a9:  brfalse    IL_0191

      IL_00ae:  ldarg.1
      IL_00af:  ldc.i4.0
      IL_00b0:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_00b5:  stloc.0
      IL_00b6:  ldarg.2
      IL_00b7:  ldc.i4.0
      IL_00b8:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_00bd:  stloc.1
      IL_00be:  ldloc.0
      IL_00bf:  ldloc.1
      IL_00c0:  bge.s      IL_00c6

      IL_00c2:  ldc.i4.m1
      IL_00c3:  nop
      IL_00c4:  br.s       IL_00cb

      IL_00c6:  ldloc.0
      IL_00c7:  ldloc.1
      IL_00c8:  cgt
      IL_00ca:  nop
      IL_00cb:  stloc.2
      IL_00cc:  ldloc.2
      IL_00cd:  brfalse.s  IL_00d1

      IL_00cf:  ldloc.2
      IL_00d0:  ret

      IL_00d1:  ldarg.1
      IL_00d2:  ldc.i4.1
      IL_00d3:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_00d8:  stloc.3
      IL_00d9:  ldarg.2
      IL_00da:  ldc.i4.1
      IL_00db:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_00e0:  stloc.s    V_4
      IL_00e2:  ldloc.3
      IL_00e3:  ldloc.s    V_4
      IL_00e5:  bge.s      IL_00eb

      IL_00e7:  ldc.i4.m1
      IL_00e8:  nop
      IL_00e9:  br.s       IL_00f1

      IL_00eb:  ldloc.3
      IL_00ec:  ldloc.s    V_4
      IL_00ee:  cgt
      IL_00f0:  nop
      IL_00f1:  stloc.s    V_5
      IL_00f3:  ldloc.s    V_5
      IL_00f5:  brfalse.s  IL_00fa

      IL_00f7:  ldloc.s    V_5
      IL_00f9:  ret

      IL_00fa:  ldarg.1
      IL_00fb:  ldc.i4.0
      IL_00fc:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_0101:  conv.i8
      IL_0102:  stloc.s    V_9
      IL_0104:  ldarg.1
      IL_0105:  ldc.i4.1
      IL_0106:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_010b:  conv.i8
      IL_010c:  stloc.s    V_10
      IL_010e:  ldarg.2
      IL_010f:  ldc.i4.0
      IL_0110:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_0115:  conv.i8
      IL_0116:  stloc.s    V_11
      IL_0118:  ldarg.2
      IL_0119:  ldc.i4.1
      IL_011a:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_011f:  conv.i8
      IL_0120:  stloc.s    V_12
      IL_0122:  ldloc.s    V_9
      IL_0124:  ldloc.s    V_11
      IL_0126:  bge.s      IL_012c

      IL_0128:  ldc.i4.m1
      IL_0129:  nop
      IL_012a:  br.s       IL_0133

      IL_012c:  ldloc.s    V_9
      IL_012e:  ldloc.s    V_11
      IL_0130:  cgt
      IL_0132:  nop
      IL_0133:  stloc.s    V_13
      IL_0135:  ldloc.s    V_13
      IL_0137:  brfalse.s  IL_013c

      IL_0139:  ldloc.s    V_13
      IL_013b:  ret

      IL_013c:  ldloc.s    V_10
      IL_013e:  ldloc.s    V_12
      IL_0140:  bge.s      IL_0146

      IL_0142:  ldc.i4.m1
      IL_0143:  nop
      IL_0144:  br.s       IL_014d

      IL_0146:  ldloc.s    V_10
      IL_0148:  ldloc.s    V_12
      IL_014a:  cgt
      IL_014c:  nop
      IL_014d:  stloc.s    V_14
      IL_014f:  ldloc.s    V_14
      IL_0151:  brfalse.s  IL_0156

      IL_0153:  ldloc.s    V_14
      IL_0155:  ret

      IL_0156:  ldarg.0
      IL_0157:  stloc.s    V_6
      IL_0159:  ldarg.1
      IL_015a:  stloc.s    V_7
      IL_015c:  ldarg.2
      IL_015d:  stloc.s    V_8
      IL_015f:  ldloc.0
      IL_0160:  stloc.s    V_15
      IL_0162:  ldloc.3
      IL_0163:  stloc.s    V_16
      IL_0165:  ldloc.s    V_9
      IL_0167:  stloc.s    V_17
      IL_0169:  ldloc.s    V_10
      IL_016b:  stloc.s    V_18
      IL_016d:  ldloc.s    V_11
      IL_016f:  stloc.s    V_19
      IL_0171:  ldloc.s    V_12
      IL_0173:  stloc.s    V_20
      IL_0175:  ldarg.0
      IL_0176:  ldarg.1
      IL_0177:  ldarg.2
      IL_0178:  ldloc.0
      IL_0179:  ldloc.3
      IL_017a:  ldloc.s    V_9
      IL_017c:  ldloc.s    V_10
      IL_017e:  ldloc.s    V_11
      IL_0180:  ldloc.s    V_12
      IL_0182:  ldc.i8     0x0
      IL_018b:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::check0@1103(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                   class [mscorlib]System.Array,
                                                                                                   class [mscorlib]System.Array,
                                                                                                   int64,
                                                                                                   int64,
                                                                                                   int64,
                                                                                                   int64,
                                                                                                   int64,
                                                                                                   int64,
                                                                                                   int64)
      IL_0190:  ret

      IL_0191:  ldarg.1
      IL_0192:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_0197:  stloc.s    V_5
      IL_0199:  ldarg.2
      IL_019a:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_019f:  stloc.s    V_13
      IL_01a1:  ldloc.s    V_5
      IL_01a3:  ldloc.s    V_13
      IL_01a5:  bge.s      IL_01ab

      IL_01a7:  ldc.i4.m1
      IL_01a8:  nop
      IL_01a9:  br.s       IL_01b2

      IL_01ab:  ldloc.s    V_5
      IL_01ad:  ldloc.s    V_13
      IL_01af:  cgt
      IL_01b1:  nop
      IL_01b2:  stloc.2
      IL_01b3:  ldloc.2
      IL_01b4:  brfalse.s  IL_01b8

      IL_01b6:  ldloc.2
      IL_01b7:  ret

      IL_01b8:  ldarg.1
      IL_01b9:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_01be:  stloc.s    V_5
      IL_01c0:  ldarg.1
      IL_01c1:  stloc.s    V_7
      IL_01c3:  ldarg.2
      IL_01c4:  stloc.s    V_8
      IL_01c6:  ldloc.s    V_5
      IL_01c8:  stloc.s    V_13
      IL_01ca:  ldarg.1
      IL_01cb:  ldarg.2
      IL_01cc:  ldloc.s    V_5
      IL_01ce:  ldc.i4.0
      IL_01cf:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::precheck@1118(class [mscorlib]System.Array,
                                                                                                     class [mscorlib]System.Array,
                                                                                                     int32,
                                                                                                     int32)
      IL_01d4:  stloc.s    V_14
      IL_01d6:  ldloc.s    V_14
      IL_01d8:  brfalse.s  IL_01dd

      IL_01da:  ldloc.s    V_14
      IL_01dc:  ret

      IL_01dd:  ldloc.s    V_5
      IL_01df:  newarr     [mscorlib]System.Int64
      IL_01e4:  stloc.s    V_21
      IL_01e6:  ldarg.0
      IL_01e7:  stloc.s    V_6
      IL_01e9:  ldarg.1
      IL_01ea:  stloc.s    V_22
      IL_01ec:  ldarg.2
      IL_01ed:  stloc.s    V_23
      IL_01ef:  ldloc.s    V_5
      IL_01f1:  stloc.s    V_24
      IL_01f3:  ldloc.s    V_21
      IL_01f5:  stloc.s    V_25
      IL_01f7:  ldarg.0
      IL_01f8:  ldarg.1
      IL_01f9:  ldarg.2
      IL_01fa:  ldloc.s    V_5
      IL_01fc:  ldloc.s    V_21
      IL_01fe:  ldc.i4.0
      IL_01ff:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::'check@1137-1'(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                      class [mscorlib]System.Array,
                                                                                                      class [mscorlib]System.Array,
                                                                                                      int32,
                                                                                                      int64[],
                                                                                                      int32)
      IL_0204:  ret
    } // end of method HashCompare::GenericComparisonArbArrayWithComparer

    .method assembly static int32  GenericCompare(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer comp,
                                                  object xobj,
                                                  object yobj) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 02 00 00 00 00 00 )
      // Code size       1046 (0x416)
      .maxstack  5
      .locals init (string V_0,
               string V_1,
               string V_2,
               string V_3,
               class [mscorlib]System.Array V_4,
               class [mscorlib]System.Array V_5,
               class [mscorlib]System.Collections.IStructuralComparable V_6,
               class [mscorlib]System.Collections.IStructuralComparable V_7,
               class [mscorlib]System.IComparable V_8,
               class [mscorlib]System.IComparable V_9,
               float32 V_10,
               float32 V_11,
               float64 V_12,
               float64 V_13,
               class [mscorlib]System.Collections.IStructuralComparable V_14,
               int32 V_15,
               class [mscorlib]System.IComparable V_16,
               native uint V_17,
               native uint V_18,
               native int V_19,
               native int V_20)
      IL_0000:  ldarg.1
      IL_0001:  brfalse    IL_040f

      IL_0006:  ldarg.2
      IL_0007:  brfalse    IL_040d

      IL_000c:  ldarg.1
      IL_000d:  isinst     [mscorlib]System.String
      IL_0012:  stloc.0
      IL_0013:  ldloc.0
      IL_0014:  brfalse    IL_02a5

      IL_0019:  ldarg.2
      IL_001a:  isinst     [mscorlib]System.String
      IL_001f:  stloc.1
      IL_0020:  ldloc.1
      IL_0021:  brfalse.s  IL_0030

      IL_0023:  ldloc.1
      IL_0024:  stloc.2
      IL_0025:  ldloc.0
      IL_0026:  stloc.3
      IL_0027:  nop
      IL_0028:  ldloc.3
      IL_0029:  ldloc.2
      IL_002a:  call       int32 [mscorlib]System.String::CompareOrdinal(string,
                                                                         string)
      IL_002f:  ret

      IL_0030:  ldarg.1
      IL_0031:  isinst     [mscorlib]System.Array
      IL_0036:  stloc.s    V_4
      IL_0038:  ldloc.s    V_4
      IL_003a:  brfalse.s  IL_004e

      IL_003c:  ldloc.s    V_4
      IL_003e:  stloc.s    V_5
      IL_0040:  ldarg.0
      IL_0041:  ldarg.2
      IL_0042:  ldarg.1
      IL_0043:  ldloc.s    V_5
      IL_0045:  ldnull
      IL_0046:  tail.
      IL_0048:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericCompare$cont@965(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                               object,
                                                                                                               object,
                                                                                                               class [mscorlib]System.Array,
                                                                                                               class Microsoft.FSharp.Core.Unit)
      IL_004d:  ret

      IL_004e:  ldarg.1
      IL_004f:  isinst     [mscorlib]System.Collections.IStructuralComparable
      IL_0054:  stloc.s    V_6
      IL_0056:  ldloc.s    V_6
      IL_0058:  brfalse.s  IL_006a

      IL_005a:  ldloc.s    V_6
      IL_005c:  stloc.s    V_7
      IL_005e:  ldloc.s    V_7
      IL_0060:  ldarg.2
      IL_0061:  ldarg.0
      IL_0062:  tail.
      IL_0064:  callvirt   instance int32 [mscorlib]System.Collections.IStructuralComparable::CompareTo(object,
                                                                                                        class [mscorlib]System.Collections.IComparer)
      IL_0069:  ret

      IL_006a:  ldarg.1
      IL_006b:  isinst     [mscorlib]System.IComparable
      IL_0070:  stloc.s    V_8
      IL_0072:  ldloc.s    V_8
      IL_0074:  brfalse    IL_0142

      IL_0079:  ldloc.s    V_8
      IL_007b:  stloc.s    V_9
      IL_007d:  ldarg.0
      IL_007e:  ldfld      bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer::throwsOnPER
      IL_0083:  brfalse    IL_0136

      IL_0088:  ldarg.1
      IL_0089:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float64>(object)
      IL_008e:  brtrue.s   IL_00d8

      IL_0090:  ldarg.1
      IL_0091:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float32>(object)
      IL_0096:  brtrue.s   IL_009e

      IL_0098:  nop
      IL_0099:  br         IL_0137

      IL_009e:  ldarg.2
      IL_009f:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float32>(object)
      IL_00a4:  brtrue.s   IL_00a8

      IL_00a6:  br.s       IL_0098

      IL_00a8:  ldarg.2
      IL_00a9:  unbox.any  [mscorlib]System.Single
      IL_00ae:  stloc.s    V_10
      IL_00b0:  ldarg.1
      IL_00b1:  unbox.any  [mscorlib]System.Single
      IL_00b6:  stloc.s    V_11
      IL_00b8:  ldloc.s    V_11
      IL_00ba:  call       bool [mscorlib]System.Single::IsNaN(float32)
      IL_00bf:  brfalse.s  IL_00c5

      IL_00c1:  ldc.i4.1
      IL_00c2:  nop
      IL_00c3:  br.s       IL_00cd

      IL_00c5:  ldloc.s    V_10
      IL_00c7:  call       bool [mscorlib]System.Single::IsNaN(float32)
      IL_00cc:  nop
      IL_00cd:  brfalse.s  IL_00d5

      IL_00cf:  call       class [mscorlib]System.Exception Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_NaNException()
      IL_00d4:  throw

      IL_00d5:  nop
      IL_00d6:  br.s       IL_0137

      IL_00d8:  ldarg.2
      IL_00d9:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float64>(object)
      IL_00de:  brtrue.s   IL_0106

      IL_00e0:  ldarg.1
      IL_00e1:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float32>(object)
      IL_00e6:  brtrue.s   IL_00ea

      IL_00e8:  br.s       IL_0098

      IL_00ea:  ldarg.2
      IL_00eb:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float32>(object)
      IL_00f0:  brtrue.s   IL_00f4

      IL_00f2:  br.s       IL_0098

      IL_00f4:  ldarg.2
      IL_00f5:  unbox.any  [mscorlib]System.Single
      IL_00fa:  ldarg.1
      IL_00fb:  unbox.any  [mscorlib]System.Single
      IL_0100:  stloc.s    V_11
      IL_0102:  stloc.s    V_10
      IL_0104:  br.s       IL_00b8

      IL_0106:  ldarg.2
      IL_0107:  unbox.any  [mscorlib]System.Double
      IL_010c:  stloc.s    V_12
      IL_010e:  ldarg.1
      IL_010f:  unbox.any  [mscorlib]System.Double
      IL_0114:  stloc.s    V_13
      IL_0116:  ldloc.s    V_13
      IL_0118:  call       bool [mscorlib]System.Double::IsNaN(float64)
      IL_011d:  brfalse.s  IL_0123

      IL_011f:  ldc.i4.1
      IL_0120:  nop
      IL_0121:  br.s       IL_012b

      IL_0123:  ldloc.s    V_12
      IL_0125:  call       bool [mscorlib]System.Double::IsNaN(float64)
      IL_012a:  nop
      IL_012b:  brfalse.s  IL_0133

      IL_012d:  call       class [mscorlib]System.Exception Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_NaNException()
      IL_0132:  throw

      IL_0133:  nop
      IL_0134:  br.s       IL_0137

      IL_0136:  nop
      IL_0137:  ldloc.s    V_9
      IL_0139:  ldarg.2
      IL_013a:  tail.
      IL_013c:  callvirt   instance int32 [mscorlib]System.IComparable::CompareTo(object)
      IL_0141:  ret

      IL_0142:  ldarg.1
      IL_0143:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<native int>(object)
      IL_0148:  brtrue     IL_01fc

      IL_014d:  ldarg.1
      IL_014e:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<native uint>(object)
      IL_0153:  brtrue.s   IL_01b1

      IL_0155:  ldarg.2
      IL_0156:  isinst     [mscorlib]System.Collections.IStructuralComparable
      IL_015b:  stloc.s    V_7
      IL_015d:  ldloc.s    V_7
      IL_015f:  brfalse.s  IL_0180

      IL_0161:  ldloc.s    V_7
      IL_0163:  stloc.s    V_14
      IL_0165:  ldloc.s    V_14
      IL_0167:  ldarg.1
      IL_0168:  ldarg.0
      IL_0169:  callvirt   instance int32 [mscorlib]System.Collections.IStructuralComparable::CompareTo(object,
                                                                                                        class [mscorlib]System.Collections.IComparer)
      IL_016e:  stloc.s    V_15
      IL_0170:  ldloc.s    V_15
      IL_0172:  ldc.i4.0
      IL_0173:  bge.s      IL_0177

      IL_0175:  ldc.i4.1
      IL_0176:  ret

      IL_0177:  ldloc.s    V_15
      IL_0179:  ldc.i4.0
      IL_017a:  ble.s      IL_017e

      IL_017c:  ldc.i4.m1
      IL_017d:  ret

      IL_017e:  ldc.i4.0
      IL_017f:  ret

      IL_0180:  ldarg.2
      IL_0181:  isinst     [mscorlib]System.IComparable
      IL_0186:  stloc.s    V_9
      IL_0188:  ldloc.s    V_9
      IL_018a:  brfalse.s  IL_01aa

      IL_018c:  ldloc.s    V_9
      IL_018e:  stloc.s    V_16
      IL_0190:  ldloc.s    V_16
      IL_0192:  ldarg.1
      IL_0193:  callvirt   instance int32 [mscorlib]System.IComparable::CompareTo(object)
      IL_0198:  stloc.s    V_15
      IL_019a:  ldloc.s    V_15
      IL_019c:  ldc.i4.0
      IL_019d:  bge.s      IL_01a1

      IL_019f:  ldc.i4.1
      IL_01a0:  ret

      IL_01a1:  ldloc.s    V_15
      IL_01a3:  ldc.i4.0
      IL_01a4:  ble.s      IL_01a8

      IL_01a6:  ldc.i4.m1
      IL_01a7:  ret

      IL_01a8:  ldc.i4.0
      IL_01a9:  ret

      IL_01aa:  ldarg.1
      IL_01ab:  call       !!0 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::FailGenericComparison<int32>(object)
      IL_01b0:  ret

      IL_01b1:  ldarg.2
      IL_01b2:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<native uint>(object)
      IL_01b7:  brtrue.s   IL_01dd

      IL_01b9:  ldarg.2
      IL_01ba:  isinst     [mscorlib]System.Collections.IStructuralComparable
      IL_01bf:  stloc.s    V_7
      IL_01c1:  ldloc.s    V_7
      IL_01c3:  brfalse.s  IL_01cb

      IL_01c5:  ldloc.s    V_7
      IL_01c7:  stloc.s    V_14
      IL_01c9:  br.s       IL_0165

      IL_01cb:  ldarg.2
      IL_01cc:  isinst     [mscorlib]System.IComparable
      IL_01d1:  stloc.s    V_9
      IL_01d3:  ldloc.s    V_9
      IL_01d5:  brfalse.s  IL_01aa

      IL_01d7:  ldloc.s    V_9
      IL_01d9:  stloc.s    V_16
      IL_01db:  br.s       IL_0190

      IL_01dd:  ldarg.2
      IL_01de:  unbox.any  [mscorlib]System.UIntPtr
      IL_01e3:  stloc.s    V_17
      IL_01e5:  ldarg.1
      IL_01e6:  unbox.any  [mscorlib]System.UIntPtr
      IL_01eb:  stloc.s    V_18
      IL_01ed:  ldloc.s    V_18
      IL_01ef:  ldloc.s    V_17
      IL_01f1:  bge.un.s   IL_01f5

      IL_01f3:  ldc.i4.m1
      IL_01f4:  ret

      IL_01f5:  ldloc.s    V_18
      IL_01f7:  ldloc.s    V_17
      IL_01f9:  cgt.un
      IL_01fb:  ret

      IL_01fc:  ldarg.2
      IL_01fd:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<native int>(object)
      IL_0202:  brtrue     IL_0286

      IL_0207:  ldarg.1
      IL_0208:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<native uint>(object)
      IL_020d:  brtrue.s   IL_023c

      IL_020f:  ldarg.2
      IL_0210:  isinst     [mscorlib]System.Collections.IStructuralComparable
      IL_0215:  stloc.s    V_7
      IL_0217:  ldloc.s    V_7
      IL_0219:  brfalse.s  IL_0224

      IL_021b:  ldloc.s    V_7
      IL_021d:  stloc.s    V_14
      IL_021f:  br         IL_0165

      IL_0224:  ldarg.2
      IL_0225:  isinst     [mscorlib]System.IComparable
      IL_022a:  stloc.s    V_9
      IL_022c:  ldloc.s    V_9
      IL_022e:  brfalse    IL_01aa

      IL_0233:  ldloc.s    V_9
      IL_0235:  stloc.s    V_16
      IL_0237:  br         IL_0190

      IL_023c:  ldarg.2
      IL_023d:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<native uint>(object)
      IL_0242:  brtrue.s   IL_0271

      IL_0244:  ldarg.2
      IL_0245:  isinst     [mscorlib]System.Collections.IStructuralComparable
      IL_024a:  stloc.s    V_7
      IL_024c:  ldloc.s    V_7
      IL_024e:  brfalse.s  IL_0259

      IL_0250:  ldloc.s    V_7
      IL_0252:  stloc.s    V_14
      IL_0254:  br         IL_0165

      IL_0259:  ldarg.2
      IL_025a:  isinst     [mscorlib]System.IComparable
      IL_025f:  stloc.s    V_9
      IL_0261:  ldloc.s    V_9
      IL_0263:  brfalse    IL_01aa

      IL_0268:  ldloc.s    V_9
      IL_026a:  stloc.s    V_16
      IL_026c:  br         IL_0190

      IL_0271:  ldarg.2
      IL_0272:  unbox.any  [mscorlib]System.UIntPtr
      IL_0277:  ldarg.1
      IL_0278:  unbox.any  [mscorlib]System.UIntPtr
      IL_027d:  stloc.s    V_18
      IL_027f:  stloc.s    V_17
      IL_0281:  br         IL_01ed

      IL_0286:  ldarg.2
      IL_0287:  unbox.any  [mscorlib]System.IntPtr
      IL_028c:  stloc.s    V_19
      IL_028e:  ldarg.1
      IL_028f:  unbox.any  [mscorlib]System.IntPtr
      IL_0294:  stloc.s    V_20
      IL_0296:  ldloc.s    V_20
      IL_0298:  ldloc.s    V_19
      IL_029a:  bge.s      IL_029e

      IL_029c:  ldc.i4.m1
      IL_029d:  ret

      IL_029e:  ldloc.s    V_20
      IL_02a0:  ldloc.s    V_19
      IL_02a2:  cgt
      IL_02a4:  ret

      IL_02a5:  ldarg.1
      IL_02a6:  isinst     [mscorlib]System.Array
      IL_02ab:  stloc.s    V_4
      IL_02ad:  ldloc.s    V_4
      IL_02af:  brfalse.s  IL_02ba

      IL_02b1:  ldloc.s    V_4
      IL_02b3:  stloc.s    V_5
      IL_02b5:  br         IL_0040

      IL_02ba:  ldarg.1
      IL_02bb:  isinst     [mscorlib]System.Collections.IStructuralComparable
      IL_02c0:  stloc.s    V_6
      IL_02c2:  ldloc.s    V_6
      IL_02c4:  brfalse.s  IL_02cf

      IL_02c6:  ldloc.s    V_6
      IL_02c8:  stloc.s    V_7
      IL_02ca:  br         IL_005e

      IL_02cf:  ldarg.1
      IL_02d0:  isinst     [mscorlib]System.IComparable
      IL_02d5:  stloc.s    V_8
      IL_02d7:  ldloc.s    V_8
      IL_02d9:  brfalse.s  IL_02e4

      IL_02db:  ldloc.s    V_8
      IL_02dd:  stloc.s    V_9
      IL_02df:  br         IL_007d

      IL_02e4:  ldarg.1
      IL_02e5:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<native int>(object)
      IL_02ea:  brtrue     IL_036e

      IL_02ef:  ldarg.1
      IL_02f0:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<native uint>(object)
      IL_02f5:  brtrue.s   IL_0324

      IL_02f7:  ldarg.2
      IL_02f8:  isinst     [mscorlib]System.Collections.IStructuralComparable
      IL_02fd:  stloc.s    V_7
      IL_02ff:  ldloc.s    V_7
      IL_0301:  brfalse.s  IL_030c

      IL_0303:  ldloc.s    V_7
      IL_0305:  stloc.s    V_14
      IL_0307:  br         IL_0165

      IL_030c:  ldarg.2
      IL_030d:  isinst     [mscorlib]System.IComparable
      IL_0312:  stloc.s    V_9
      IL_0314:  ldloc.s    V_9
      IL_0316:  brfalse    IL_01aa

      IL_031b:  ldloc.s    V_9
      IL_031d:  stloc.s    V_16
      IL_031f:  br         IL_0190

      IL_0324:  ldarg.2
      IL_0325:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<native uint>(object)
      IL_032a:  brtrue.s   IL_0359

      IL_032c:  ldarg.2
      IL_032d:  isinst     [mscorlib]System.Collections.IStructuralComparable
      IL_0332:  stloc.s    V_7
      IL_0334:  ldloc.s    V_7
      IL_0336:  brfalse.s  IL_0341

      IL_0338:  ldloc.s    V_7
      IL_033a:  stloc.s    V_14
      IL_033c:  br         IL_0165

      IL_0341:  ldarg.2
      IL_0342:  isinst     [mscorlib]System.IComparable
      IL_0347:  stloc.s    V_9
      IL_0349:  ldloc.s    V_9
      IL_034b:  brfalse    IL_01aa

      IL_0350:  ldloc.s    V_9
      IL_0352:  stloc.s    V_16
      IL_0354:  br         IL_0190

      IL_0359:  ldarg.2
      IL_035a:  unbox.any  [mscorlib]System.UIntPtr
      IL_035f:  ldarg.1
      IL_0360:  unbox.any  [mscorlib]System.UIntPtr
      IL_0365:  stloc.s    V_18
      IL_0367:  stloc.s    V_17
      IL_0369:  br         IL_01ed

      IL_036e:  ldarg.2
      IL_036f:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<native int>(object)
      IL_0374:  brtrue     IL_03f8

      IL_0379:  ldarg.1
      IL_037a:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<native uint>(object)
      IL_037f:  brtrue.s   IL_03ae

      IL_0381:  ldarg.2
      IL_0382:  isinst     [mscorlib]System.Collections.IStructuralComparable
      IL_0387:  stloc.s    V_7
      IL_0389:  ldloc.s    V_7
      IL_038b:  brfalse.s  IL_0396

      IL_038d:  ldloc.s    V_7
      IL_038f:  stloc.s    V_14
      IL_0391:  br         IL_0165

      IL_0396:  ldarg.2
      IL_0397:  isinst     [mscorlib]System.IComparable
      IL_039c:  stloc.s    V_9
      IL_039e:  ldloc.s    V_9
      IL_03a0:  brfalse    IL_01aa

      IL_03a5:  ldloc.s    V_9
      IL_03a7:  stloc.s    V_16
      IL_03a9:  br         IL_0190

      IL_03ae:  ldarg.2
      IL_03af:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<native uint>(object)
      IL_03b4:  brtrue.s   IL_03e3

      IL_03b6:  ldarg.2
      IL_03b7:  isinst     [mscorlib]System.Collections.IStructuralComparable
      IL_03bc:  stloc.s    V_7
      IL_03be:  ldloc.s    V_7
      IL_03c0:  brfalse.s  IL_03cb

      IL_03c2:  ldloc.s    V_7
      IL_03c4:  stloc.s    V_14
      IL_03c6:  br         IL_0165

      IL_03cb:  ldarg.2
      IL_03cc:  isinst     [mscorlib]System.IComparable
      IL_03d1:  stloc.s    V_9
      IL_03d3:  ldloc.s    V_9
      IL_03d5:  brfalse    IL_01aa

      IL_03da:  ldloc.s    V_9
      IL_03dc:  stloc.s    V_16
      IL_03de:  br         IL_0190

      IL_03e3:  ldarg.2
      IL_03e4:  unbox.any  [mscorlib]System.UIntPtr
      IL_03e9:  ldarg.1
      IL_03ea:  unbox.any  [mscorlib]System.UIntPtr
      IL_03ef:  stloc.s    V_18
      IL_03f1:  stloc.s    V_17
      IL_03f3:  br         IL_01ed

      IL_03f8:  ldarg.2
      IL_03f9:  unbox.any  [mscorlib]System.IntPtr
      IL_03fe:  ldarg.1
      IL_03ff:  unbox.any  [mscorlib]System.IntPtr
      IL_0404:  stloc.s    V_20
      IL_0406:  stloc.s    V_19
      IL_0408:  br         IL_0296

      IL_040d:  ldc.i4.1
      IL_040e:  ret

      IL_040f:  ldarg.2
      IL_0410:  brfalse.s  IL_0414

      IL_0412:  ldc.i4.m1
      IL_0413:  ret

      IL_0414:  ldc.i4.0
      IL_0415:  ret
    } // end of method HashCompare::GenericCompare

    .method assembly static int32  check@1081(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer comp,
                                              class [mscorlib]System.Array x,
                                              class [mscorlib]System.Array y,
                                              int64 lenx,
                                              int64 basex,
                                              int64 basey,
                                              int64 i) cil managed
    {
      // Code size       78 (0x4e)
      .maxstack  8
      .locals init (int32 V_0)
      IL_0000:  nop
      IL_0001:  ldarg.s    i
      IL_0003:  ldarg.3
      IL_0004:  blt.s      IL_0008

      IL_0006:  ldc.i4.0
      IL_0007:  ret

      IL_0008:  ldarg.0
      IL_0009:  ldarg.1
      IL_000a:  ldarg.s    i
      IL_000c:  ldarg.s    basex
      IL_000e:  add
      IL_000f:  callvirt   instance object [mscorlib]System.Array::GetValue(int64)
      IL_0014:  ldarg.2
      IL_0015:  ldarg.s    i
      IL_0017:  ldarg.s    basey
      IL_0019:  add
      IL_001a:  callvirt   instance object [mscorlib]System.Array::GetValue(int64)
      IL_001f:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericCompare(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                      object,
                                                                                                      object)
      IL_0024:  stloc.0
      IL_0025:  ldloc.0
      IL_0026:  brfalse.s  IL_002a

      IL_0028:  ldloc.0
      IL_0029:  ret

      IL_002a:  ldarg.0
      IL_002b:  ldarg.1
      IL_002c:  ldarg.2
      IL_002d:  ldarg.3
      IL_002e:  ldarg.s    basex
      IL_0030:  ldarg.s    basey
      IL_0032:  ldarg.s    i
      IL_0034:  ldc.i8     0x1
      IL_003d:  add
      IL_003e:  starg.s    i
      IL_0040:  starg.s    basey
      IL_0042:  starg.s    basex
      IL_0044:  starg.s    lenx
      IL_0046:  starg.s    y
      IL_0048:  starg.s    x
      IL_004a:  starg.s    comp
      IL_004c:  br.s       IL_0000
    } // end of method HashCompare::check@1081

    .method assembly static int32  check1@1104(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer comp,
                                               class [mscorlib]System.Array x,
                                               class [mscorlib]System.Array y,
                                               int64 lenx1,
                                               int64 basex0,
                                               int64 basex1,
                                               int64 basey0,
                                               int64 basey1,
                                               int64 i,
                                               int64 j) cil managed
    {
      // Code size       100 (0x64)
      .maxstack  11
      .locals init (int32 V_0)
      IL_0000:  nop
      IL_0001:  ldarg.s    j
      IL_0003:  ldarg.3
      IL_0004:  blt.s      IL_0008

      IL_0006:  ldc.i4.0
      IL_0007:  ret

      IL_0008:  ldarg.0
      IL_0009:  ldarg.1
      IL_000a:  ldarg.s    i
      IL_000c:  ldarg.s    basex0
      IL_000e:  add
      IL_000f:  ldarg.s    j
      IL_0011:  ldarg.s    basex1
      IL_0013:  add
      IL_0014:  callvirt   instance object [mscorlib]System.Array::GetValue(int64,
                                                                            int64)
      IL_0019:  ldarg.2
      IL_001a:  ldarg.s    i
      IL_001c:  ldarg.s    basey0
      IL_001e:  add
      IL_001f:  ldarg.s    j
      IL_0021:  ldarg.s    basey1
      IL_0023:  add
      IL_0024:  callvirt   instance object [mscorlib]System.Array::GetValue(int64,
                                                                            int64)
      IL_0029:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericCompare(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                      object,
                                                                                                      object)
      IL_002e:  stloc.0
      IL_002f:  ldloc.0
      IL_0030:  brfalse.s  IL_0034

      IL_0032:  ldloc.0
      IL_0033:  ret

      IL_0034:  ldarg.0
      IL_0035:  ldarg.1
      IL_0036:  ldarg.2
      IL_0037:  ldarg.3
      IL_0038:  ldarg.s    basex0
      IL_003a:  ldarg.s    basex1
      IL_003c:  ldarg.s    basey0
      IL_003e:  ldarg.s    basey1
      IL_0040:  ldarg.s    i
      IL_0042:  ldarg.s    j
      IL_0044:  ldc.i8     0x1
      IL_004d:  add
      IL_004e:  starg.s    j
      IL_0050:  starg.s    i
      IL_0052:  starg.s    basey1
      IL_0054:  starg.s    basey0
      IL_0056:  starg.s    basex1
      IL_0058:  starg.s    basex0
      IL_005a:  starg.s    lenx1
      IL_005c:  starg.s    y
      IL_005e:  starg.s    x
      IL_0060:  starg.s    comp
      IL_0062:  br.s       IL_0000
    } // end of method HashCompare::check1@1104

    .method assembly static int32  check0@1103(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer comp,
                                               class [mscorlib]System.Array x,
                                               class [mscorlib]System.Array y,
                                               int64 lenx0,
                                               int64 lenx1,
                                               int64 basex0,
                                               int64 basex1,
                                               int64 basey0,
                                               int64 basey1,
                                               int64 i) cil managed
    {
      // Code size       126 (0x7e)
      .maxstack  11
      .locals init (class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer V_0,
               class [mscorlib]System.Array V_1,
               class [mscorlib]System.Array V_2,
               int64 V_3,
               int64 V_4,
               int64 V_5,
               int64 V_6,
               int64 V_7,
               int64 V_8,
               int32 V_9)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  stloc.0
      IL_0003:  ldarg.1
      IL_0004:  stloc.1
      IL_0005:  ldarg.2
      IL_0006:  stloc.2
      IL_0007:  ldarg.s    lenx1
      IL_0009:  stloc.3
      IL_000a:  ldarg.s    basex0
      IL_000c:  stloc.s    V_4
      IL_000e:  ldarg.s    basex1
      IL_0010:  stloc.s    V_5
      IL_0012:  ldarg.s    basey0
      IL_0014:  stloc.s    V_6
      IL_0016:  ldarg.s    basey1
      IL_0018:  stloc.s    V_7
      IL_001a:  ldarg.s    i
      IL_001c:  stloc.s    V_8
      IL_001e:  ldarg.s    i
      IL_0020:  ldarg.3
      IL_0021:  blt.s      IL_0025

      IL_0023:  ldc.i4.0
      IL_0024:  ret

      IL_0025:  ldarg.0
      IL_0026:  ldarg.1
      IL_0027:  ldarg.2
      IL_0028:  ldarg.s    lenx1
      IL_002a:  ldarg.s    basex0
      IL_002c:  ldarg.s    basex1
      IL_002e:  ldarg.s    basey0
      IL_0030:  ldarg.s    basey1
      IL_0032:  ldarg.s    i
      IL_0034:  ldc.i8     0x0
      IL_003d:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::check1@1104(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                   class [mscorlib]System.Array,
                                                                                                   class [mscorlib]System.Array,
                                                                                                   int64,
                                                                                                   int64,
                                                                                                   int64,
                                                                                                   int64,
                                                                                                   int64,
                                                                                                   int64,
                                                                                                   int64)
      IL_0042:  stloc.s    V_9
      IL_0044:  ldloc.s    V_9
      IL_0046:  brfalse.s  IL_004b

      IL_0048:  ldloc.s    V_9
      IL_004a:  ret

      IL_004b:  ldarg.0
      IL_004c:  ldarg.1
      IL_004d:  ldarg.2
      IL_004e:  ldarg.3
      IL_004f:  ldarg.s    lenx1
      IL_0051:  ldarg.s    basex0
      IL_0053:  ldarg.s    basex1
      IL_0055:  ldarg.s    basey0
      IL_0057:  ldarg.s    basey1
      IL_0059:  ldarg.s    i
      IL_005b:  ldc.i8     0x1
      IL_0064:  add
      IL_0065:  starg.s    i
      IL_0067:  starg.s    basey1
      IL_0069:  starg.s    basey0
      IL_006b:  starg.s    basex1
      IL_006d:  starg.s    basex0
      IL_006f:  starg.s    lenx1
      IL_0071:  starg.s    lenx0
      IL_0073:  starg.s    y
      IL_0075:  starg.s    x
      IL_0077:  starg.s    comp
      IL_0079:  br         IL_0000
    } // end of method HashCompare::check0@1103

    .method assembly static int32  precheck@1118(class [mscorlib]System.Array x,
                                                 class [mscorlib]System.Array y,
                                                 int32 ndims,
                                                 int32 k) cil managed
    {
      // Code size       99 (0x63)
      .maxstack  5
      .locals init (int32 V_0,
               int64 V_1,
               int64 V_2,
               int32 V_3,
               int32 V_4,
               int32 V_5)
      IL_0000:  nop
      IL_0001:  ldarg.3
      IL_0002:  ldarg.2
      IL_0003:  blt.s      IL_0007

      IL_0005:  ldc.i4.0
      IL_0006:  ret

      IL_0007:  ldarg.0
      IL_0008:  ldarg.3
      IL_0009:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_000e:  stloc.1
      IL_000f:  ldarg.1
      IL_0010:  ldarg.3
      IL_0011:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_0016:  stloc.2
      IL_0017:  ldloc.1
      IL_0018:  ldloc.2
      IL_0019:  bge.s      IL_001f

      IL_001b:  ldc.i4.m1
      IL_001c:  nop
      IL_001d:  br.s       IL_0024

      IL_001f:  ldloc.1
      IL_0020:  ldloc.2
      IL_0021:  cgt
      IL_0023:  nop
      IL_0024:  stloc.0
      IL_0025:  ldloc.0
      IL_0026:  brfalse.s  IL_002a

      IL_0028:  ldloc.0
      IL_0029:  ret

      IL_002a:  ldarg.0
      IL_002b:  ldarg.3
      IL_002c:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_0031:  stloc.s    V_4
      IL_0033:  ldarg.1
      IL_0034:  ldarg.3
      IL_0035:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_003a:  stloc.s    V_5
      IL_003c:  ldloc.s    V_4
      IL_003e:  ldloc.s    V_5
      IL_0040:  bge.s      IL_0046

      IL_0042:  ldc.i4.m1
      IL_0043:  nop
      IL_0044:  br.s       IL_004d

      IL_0046:  ldloc.s    V_4
      IL_0048:  ldloc.s    V_5
      IL_004a:  cgt
      IL_004c:  nop
      IL_004d:  stloc.3
      IL_004e:  ldloc.3
      IL_004f:  brfalse.s  IL_0053

      IL_0051:  ldloc.3
      IL_0052:  ret

      IL_0053:  ldarg.0
      IL_0054:  ldarg.1
      IL_0055:  ldarg.2
      IL_0056:  ldarg.3
      IL_0057:  ldc.i4.1
      IL_0058:  add
      IL_0059:  starg.s    k
      IL_005b:  starg.s    ndims
      IL_005d:  starg.s    y
      IL_005f:  starg.s    x
      IL_0061:  br.s       IL_0000
    } // end of method HashCompare::precheck@1118

    .method assembly static int32  checkN@1128(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer comp,
                                               class [mscorlib]System.Array x,
                                               class [mscorlib]System.Array y,
                                               int32 ndims,
                                               int64[] idxs,
                                               int32 k,
                                               int64 baseIdx,
                                               int64 i,
                                               int64 lim) cil managed
    {
      // Code size       124 (0x7c)
      .maxstack  9
      .locals init (int32 V_0)
      IL_0000:  nop
      IL_0001:  ldarg.s    i
      IL_0003:  ldarg.s    lim
      IL_0005:  blt.s      IL_0009

      IL_0007:  ldc.i4.0
      IL_0008:  ret

      IL_0009:  ldarg.s    idxs
      IL_000b:  ldarg.s    k
      IL_000d:  ldarg.s    baseIdx
      IL_000f:  ldarg.s    i
      IL_0011:  add
      IL_0012:  stelem     [mscorlib]System.Int64
      IL_0017:  ldarg.s    k
      IL_0019:  ldarg.3
      IL_001a:  ldc.i4.1
      IL_001b:  sub
      IL_001c:  bne.un.s   IL_0037

      IL_001e:  ldarg.0
      IL_001f:  ldarg.1
      IL_0020:  ldarg.s    idxs
      IL_0022:  callvirt   instance object [mscorlib]System.Array::GetValue(int64[])
      IL_0027:  ldarg.2
      IL_0028:  ldarg.s    idxs
      IL_002a:  callvirt   instance object [mscorlib]System.Array::GetValue(int64[])
      IL_002f:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericCompare(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                      object,
                                                                                                      object)
      IL_0034:  nop
      IL_0035:  br.s       IL_0047

      IL_0037:  ldarg.0
      IL_0038:  ldarg.1
      IL_0039:  ldarg.2
      IL_003a:  ldarg.3
      IL_003b:  ldarg.s    idxs
      IL_003d:  ldarg.s    k
      IL_003f:  ldc.i4.1
      IL_0040:  add
      IL_0041:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::'check@1137-1'(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                      class [mscorlib]System.Array,
                                                                                                      class [mscorlib]System.Array,
                                                                                                      int32,
                                                                                                      int64[],
                                                                                                      int32)
      IL_0046:  nop
      IL_0047:  stloc.0
      IL_0048:  ldloc.0
      IL_0049:  brfalse.s  IL_004d

      IL_004b:  ldloc.0
      IL_004c:  ret

      IL_004d:  ldarg.0
      IL_004e:  ldarg.1
      IL_004f:  ldarg.2
      IL_0050:  ldarg.3
      IL_0051:  ldarg.s    idxs
      IL_0053:  ldarg.s    k
      IL_0055:  ldarg.s    baseIdx
      IL_0057:  ldarg.s    i
      IL_0059:  ldc.i8     0x1
      IL_0062:  add
      IL_0063:  ldarg.s    lim
      IL_0065:  starg.s    lim
      IL_0067:  starg.s    i
      IL_0069:  starg.s    baseIdx
      IL_006b:  starg.s    k
      IL_006d:  starg.s    idxs
      IL_006f:  starg.s    ndims
      IL_0071:  starg.s    y
      IL_0073:  starg.s    x
      IL_0075:  starg.s    comp
      IL_0077:  br         IL_0000
    } // end of method HashCompare::checkN@1128

    .method assembly static int32  'check@1137-1'(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer comp,
                                                  class [mscorlib]System.Array x,
                                                  class [mscorlib]System.Array y,
                                                  int32 ndims,
                                                  int64[] idxs,
                                                  int32 k) cil managed
    {
      // Code size       50 (0x32)
      .maxstack  10
      .locals init (int32 V_0)
      IL_0000:  nop
      IL_0001:  ldarg.s    k
      IL_0003:  ldarg.3
      IL_0004:  blt.s      IL_0008

      IL_0006:  ldc.i4.0
      IL_0007:  ret

      IL_0008:  ldarg.1
      IL_0009:  ldarg.s    k
      IL_000b:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_0010:  stloc.0
      IL_0011:  ldarg.0
      IL_0012:  ldarg.1
      IL_0013:  ldarg.2
      IL_0014:  ldarg.3
      IL_0015:  ldarg.s    idxs
      IL_0017:  ldarg.s    k
      IL_0019:  ldloc.0
      IL_001a:  conv.i8
      IL_001b:  ldc.i8     0x0
      IL_0024:  ldarg.1
      IL_0025:  ldarg.s    k
      IL_0027:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_002c:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::checkN@1128(class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer,
                                                                                                   class [mscorlib]System.Array,
                                                                                                   class [mscorlib]System.Array,
                                                                                                   int32,
                                                                                                   int64[],
                                                                                                   int32,
                                                                                                   int64,
                                                                                                   int64,
                                                                                                   int64)
      IL_0031:  ret
    } // end of method HashCompare::'check@1137-1'

    .method public static int32  GenericComparisonWithComparerIntrinsic<T>(class [mscorlib]System.Collections.IComparer comp,
                                                                           !!T x,
                                                                           !!T y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 03 00 00 00 01 00 00 00 01 00 00 00 01 00
                                                                                                         00 00 00 00 )
      .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                                  int32) = ( 01 00 66 54 68 69 73 20 66 75 6E 63 74 69 6F 6E   // ..fThis function
                                                                                             20 69 73 20 61 20 70 72 69 6D 69 74 69 76 65 20   //  is a primitive
                                                                                             6C 69 62 72 61 72 79 20 72 6F 75 74 69 6E 65 20   // library routine
                                                                                             75 73 65 64 20 62 79 20 6F 70 74 69 6D 69 7A 65   // used by optimize
                                                                                             64 20 46 23 20 63 6F 64 65 20 61 6E 64 20 73 68   // d F# code and sh
                                                                                             6F 75 6C 64 20 6E 6F 74 20 62 65 20 75 73 65 64   // ould not be used
                                                                                             20 64 69 72 65 63 74 6C 79 B4 04 00 00 01 00 54   //  directly......T
                                                                                             02 08 49 73 48 69 64 64 65 6E 01 )                // ..IsHidden.
      // Code size       22 (0x16)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldarg.1
      IL_0003:  box        !!T
      IL_0008:  ldarg.2
      IL_0009:  box        !!T
      IL_000e:  tail.
      IL_0010:  callvirt   instance int32 [mscorlib]System.Collections.IComparer::Compare(object,
                                                                                          object)
      IL_0015:  ret
    } // end of method HashCompare::GenericComparisonWithComparerIntrinsic

    .method assembly static bool  GenericEqualityByteArray(uint8[] x,
                                                           uint8[] y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
      // Code size       68 (0x44)
      .maxstack  3
      .locals init (int32 V_0,
               int32 V_1,
               bool V_2,
               int32 V_3,
               bool V_4,
               bool V_5)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldarg.1
      IL_0006:  ldlen
      IL_0007:  conv.i4
      IL_0008:  stloc.1
      IL_0009:  ldloc.0
      IL_000a:  ldloc.1
      IL_000b:  ceq
      IL_000d:  stloc.2
      IL_000e:  ldloc.2
      IL_000f:  brtrue.s   IL_0013

      IL_0011:  ldloc.2
      IL_0012:  ret

      IL_0013:  ldc.i4.0
      IL_0014:  stloc.3
      IL_0015:  ldc.i4.1
      IL_0016:  stloc.s    V_4
      IL_0018:  ldloc.3
      IL_0019:  ldloc.0
      IL_001a:  bge.s      IL_0041

      IL_001c:  ldarg.0
      IL_001d:  ldloc.3
      IL_001e:  ldelem     [mscorlib]System.Byte
      IL_0023:  ldarg.1
      IL_0024:  ldloc.3
      IL_0025:  ldelem     [mscorlib]System.Byte
      IL_002a:  ceq
      IL_002c:  stloc.s    V_5
      IL_002e:  ldloc.s    V_5
      IL_0030:  brtrue.s   IL_003a

      IL_0032:  ldc.i4.0
      IL_0033:  stloc.s    V_4
      IL_0035:  ldloc.0
      IL_0036:  stloc.3
      IL_0037:  nop
      IL_0038:  br.s       IL_0018

      IL_003a:  ldloc.3
      IL_003b:  ldc.i4.1
      IL_003c:  add
      IL_003d:  stloc.3
      IL_003e:  nop
      IL_003f:  br.s       IL_0018

      IL_0041:  ldloc.s    V_4
      IL_0043:  ret
    } // end of method HashCompare::GenericEqualityByteArray

    .method assembly static bool  GenericEqualityInt32Array(int32[] x,
                                                            int32[] y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
      // Code size       68 (0x44)
      .maxstack  3
      .locals init (int32 V_0,
               int32 V_1,
               bool V_2,
               int32 V_3,
               bool V_4,
               bool V_5)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldarg.1
      IL_0006:  ldlen
      IL_0007:  conv.i4
      IL_0008:  stloc.1
      IL_0009:  ldloc.0
      IL_000a:  ldloc.1
      IL_000b:  ceq
      IL_000d:  stloc.2
      IL_000e:  ldloc.2
      IL_000f:  brtrue.s   IL_0013

      IL_0011:  ldloc.2
      IL_0012:  ret

      IL_0013:  ldc.i4.0
      IL_0014:  stloc.3
      IL_0015:  ldc.i4.1
      IL_0016:  stloc.s    V_4
      IL_0018:  ldloc.3
      IL_0019:  ldloc.0
      IL_001a:  bge.s      IL_0041

      IL_001c:  ldarg.0
      IL_001d:  ldloc.3
      IL_001e:  ldelem     [mscorlib]System.Int32
      IL_0023:  ldarg.1
      IL_0024:  ldloc.3
      IL_0025:  ldelem     [mscorlib]System.Int32
      IL_002a:  ceq
      IL_002c:  stloc.s    V_5
      IL_002e:  ldloc.s    V_5
      IL_0030:  brtrue.s   IL_003a

      IL_0032:  ldc.i4.0
      IL_0033:  stloc.s    V_4
      IL_0035:  ldloc.0
      IL_0036:  stloc.3
      IL_0037:  nop
      IL_0038:  br.s       IL_0018

      IL_003a:  ldloc.3
      IL_003b:  ldc.i4.1
      IL_003c:  add
      IL_003d:  stloc.3
      IL_003e:  nop
      IL_003f:  br.s       IL_0018

      IL_0041:  ldloc.s    V_4
      IL_0043:  ret
    } // end of method HashCompare::GenericEqualityInt32Array

    .method assembly static bool  f32eq@1421(bool er,
                                             float32 x,
                                             float32 y) cil managed
    {
      // Code size       39 (0x27)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  brfalse.s  IL_000e

      IL_0004:  ldarg.1
      IL_0005:  ldarg.1
      IL_0006:  ceq
      IL_0008:  ldc.i4.0
      IL_0009:  ceq
      IL_000b:  nop
      IL_000c:  br.s       IL_0010

      IL_000e:  ldc.i4.0
      IL_000f:  nop
      IL_0010:  brfalse.s  IL_001c

      IL_0012:  ldarg.2
      IL_0013:  ldarg.2
      IL_0014:  ceq
      IL_0016:  ldc.i4.0
      IL_0017:  ceq
      IL_0019:  nop
      IL_001a:  br.s       IL_001e

      IL_001c:  ldc.i4.0
      IL_001d:  nop
      IL_001e:  brfalse.s  IL_0022

      IL_0020:  ldc.i4.1
      IL_0021:  ret

      IL_0022:  ldarg.1
      IL_0023:  ldarg.2
      IL_0024:  ceq
      IL_0026:  ret
    } // end of method HashCompare::f32eq@1421

    .method assembly static bool  GenericEqualitySingleArray(bool er,
                                                             float32[] x,
                                                             float32[] y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 03 00 00 00 01 00 00 00 01 00 00 00 01 00
                                                                                                         00 00 00 00 )
      // Code size       72 (0x48)
      .maxstack  4
      .locals init (int32 V_0,
               int32 V_1,
               bool V_2,
               int32 V_3,
               bool V_4,
               bool V_5)
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldarg.2
      IL_0006:  ldlen
      IL_0007:  conv.i4
      IL_0008:  stloc.1
      IL_0009:  ldloc.0
      IL_000a:  ldloc.1
      IL_000b:  ceq
      IL_000d:  stloc.2
      IL_000e:  ldloc.2
      IL_000f:  brtrue.s   IL_0013

      IL_0011:  ldloc.2
      IL_0012:  ret

      IL_0013:  ldc.i4.0
      IL_0014:  stloc.3
      IL_0015:  ldc.i4.1
      IL_0016:  stloc.s    V_4
      IL_0018:  ldloc.3
      IL_0019:  ldloc.0
      IL_001a:  bge.s      IL_0045

      IL_001c:  ldarg.0
      IL_001d:  ldarg.1
      IL_001e:  ldloc.3
      IL_001f:  ldelem     [mscorlib]System.Single
      IL_0024:  ldarg.2
      IL_0025:  ldloc.3
      IL_0026:  ldelem     [mscorlib]System.Single
      IL_002b:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::f32eq@1421(bool,
                                                                                                 float32,
                                                                                                 float32)
      IL_0030:  stloc.s    V_5
      IL_0032:  ldloc.s    V_5
      IL_0034:  brtrue.s   IL_003e

      IL_0036:  ldc.i4.0
      IL_0037:  stloc.s    V_4
      IL_0039:  ldloc.0
      IL_003a:  stloc.3
      IL_003b:  nop
      IL_003c:  br.s       IL_0018

      IL_003e:  ldloc.3
      IL_003f:  ldc.i4.1
      IL_0040:  add
      IL_0041:  stloc.3
      IL_0042:  nop
      IL_0043:  br.s       IL_0018

      IL_0045:  ldloc.s    V_4
      IL_0047:  ret
    } // end of method HashCompare::GenericEqualitySingleArray

    .method assembly static bool  feq@1438(bool er,
                                           float64 x,
                                           float64 y) cil managed
    {
      // Code size       39 (0x27)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  brfalse.s  IL_000e

      IL_0004:  ldarg.1
      IL_0005:  ldarg.1
      IL_0006:  ceq
      IL_0008:  ldc.i4.0
      IL_0009:  ceq
      IL_000b:  nop
      IL_000c:  br.s       IL_0010

      IL_000e:  ldc.i4.0
      IL_000f:  nop
      IL_0010:  brfalse.s  IL_001c

      IL_0012:  ldarg.2
      IL_0013:  ldarg.2
      IL_0014:  ceq
      IL_0016:  ldc.i4.0
      IL_0017:  ceq
      IL_0019:  nop
      IL_001a:  br.s       IL_001e

      IL_001c:  ldc.i4.0
      IL_001d:  nop
      IL_001e:  brfalse.s  IL_0022

      IL_0020:  ldc.i4.1
      IL_0021:  ret

      IL_0022:  ldarg.1
      IL_0023:  ldarg.2
      IL_0024:  ceq
      IL_0026:  ret
    } // end of method HashCompare::feq@1438

    .method assembly static bool  GenericEqualityDoubleArray(bool er,
                                                             float64[] x,
                                                             float64[] y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 03 00 00 00 01 00 00 00 01 00 00 00 01 00
                                                                                                         00 00 00 00 )
      // Code size       72 (0x48)
      .maxstack  4
      .locals init (int32 V_0,
               int32 V_1,
               bool V_2,
               int32 V_3,
               bool V_4,
               bool V_5)
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldarg.2
      IL_0006:  ldlen
      IL_0007:  conv.i4
      IL_0008:  stloc.1
      IL_0009:  ldloc.0
      IL_000a:  ldloc.1
      IL_000b:  ceq
      IL_000d:  stloc.2
      IL_000e:  ldloc.2
      IL_000f:  brtrue.s   IL_0013

      IL_0011:  ldloc.2
      IL_0012:  ret

      IL_0013:  ldc.i4.0
      IL_0014:  stloc.3
      IL_0015:  ldc.i4.1
      IL_0016:  stloc.s    V_4
      IL_0018:  ldloc.3
      IL_0019:  ldloc.0
      IL_001a:  bge.s      IL_0045

      IL_001c:  ldarg.0
      IL_001d:  ldarg.1
      IL_001e:  ldloc.3
      IL_001f:  ldelem     [mscorlib]System.Double
      IL_0024:  ldarg.2
      IL_0025:  ldloc.3
      IL_0026:  ldelem     [mscorlib]System.Double
      IL_002b:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::feq@1438(bool,
                                                                                               float64,
                                                                                               float64)
      IL_0030:  stloc.s    V_5
      IL_0032:  ldloc.s    V_5
      IL_0034:  brtrue.s   IL_003e

      IL_0036:  ldc.i4.0
      IL_0037:  stloc.s    V_4
      IL_0039:  ldloc.0
      IL_003a:  stloc.3
      IL_003b:  nop
      IL_003c:  br.s       IL_0018

      IL_003e:  ldloc.3
      IL_003f:  ldc.i4.1
      IL_0040:  add
      IL_0041:  stloc.3
      IL_0042:  nop
      IL_0043:  br.s       IL_0018

      IL_0045:  ldloc.s    V_4
      IL_0047:  ret
    } // end of method HashCompare::GenericEqualityDoubleArray

    .method assembly static bool  GenericEqualityCharArray(char[] x,
                                                           char[] y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
      // Code size       68 (0x44)
      .maxstack  3
      .locals init (int32 V_0,
               int32 V_1,
               bool V_2,
               int32 V_3,
               bool V_4,
               bool V_5)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldarg.1
      IL_0006:  ldlen
      IL_0007:  conv.i4
      IL_0008:  stloc.1
      IL_0009:  ldloc.0
      IL_000a:  ldloc.1
      IL_000b:  ceq
      IL_000d:  stloc.2
      IL_000e:  ldloc.2
      IL_000f:  brtrue.s   IL_0013

      IL_0011:  ldloc.2
      IL_0012:  ret

      IL_0013:  ldc.i4.0
      IL_0014:  stloc.3
      IL_0015:  ldc.i4.1
      IL_0016:  stloc.s    V_4
      IL_0018:  ldloc.3
      IL_0019:  ldloc.0
      IL_001a:  bge.s      IL_0041

      IL_001c:  ldarg.0
      IL_001d:  ldloc.3
      IL_001e:  ldelem     [mscorlib]System.Char
      IL_0023:  ldarg.1
      IL_0024:  ldloc.3
      IL_0025:  ldelem     [mscorlib]System.Char
      IL_002a:  ceq
      IL_002c:  stloc.s    V_5
      IL_002e:  ldloc.s    V_5
      IL_0030:  brtrue.s   IL_003a

      IL_0032:  ldc.i4.0
      IL_0033:  stloc.s    V_4
      IL_0035:  ldloc.0
      IL_0036:  stloc.3
      IL_0037:  nop
      IL_0038:  br.s       IL_0018

      IL_003a:  ldloc.3
      IL_003b:  ldc.i4.1
      IL_003c:  add
      IL_003d:  stloc.3
      IL_003e:  nop
      IL_003f:  br.s       IL_0018

      IL_0041:  ldloc.s    V_4
      IL_0043:  ret
    } // end of method HashCompare::GenericEqualityCharArray

    .method assembly static bool  GenericEqualityInt64Array(int64[] x,
                                                            int64[] y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
      // Code size       68 (0x44)
      .maxstack  3
      .locals init (int32 V_0,
               int32 V_1,
               bool V_2,
               int32 V_3,
               bool V_4,
               bool V_5)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldarg.1
      IL_0006:  ldlen
      IL_0007:  conv.i4
      IL_0008:  stloc.1
      IL_0009:  ldloc.0
      IL_000a:  ldloc.1
      IL_000b:  ceq
      IL_000d:  stloc.2
      IL_000e:  ldloc.2
      IL_000f:  brtrue.s   IL_0013

      IL_0011:  ldloc.2
      IL_0012:  ret

      IL_0013:  ldc.i4.0
      IL_0014:  stloc.3
      IL_0015:  ldc.i4.1
      IL_0016:  stloc.s    V_4
      IL_0018:  ldloc.3
      IL_0019:  ldloc.0
      IL_001a:  bge.s      IL_0041

      IL_001c:  ldarg.0
      IL_001d:  ldloc.3
      IL_001e:  ldelem     [mscorlib]System.Int64
      IL_0023:  ldarg.1
      IL_0024:  ldloc.3
      IL_0025:  ldelem     [mscorlib]System.Int64
      IL_002a:  ceq
      IL_002c:  stloc.s    V_5
      IL_002e:  ldloc.s    V_5
      IL_0030:  brtrue.s   IL_003a

      IL_0032:  ldc.i4.0
      IL_0033:  stloc.s    V_4
      IL_0035:  ldloc.0
      IL_0036:  stloc.3
      IL_0037:  nop
      IL_0038:  br.s       IL_0018

      IL_003a:  ldloc.3
      IL_003b:  ldc.i4.1
      IL_003c:  add
      IL_003d:  stloc.3
      IL_003e:  nop
      IL_003f:  br.s       IL_0018

      IL_0041:  ldloc.s    V_4
      IL_0043:  ret
    } // end of method HashCompare::GenericEqualityInt64Array

    .method assembly static bool  GenericEqualityObj$cont@1498(bool er,
                                                               class [mscorlib]System.Collections.IEqualityComparer iec,
                                                               object yobj,
                                                               object xobj,
                                                               class [mscorlib]System.Array arr1,
                                                               class Microsoft.FSharp.Core.Unit unitVar) cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      // Code size       8020 (0x1f54)
      .maxstack  4
      .locals init (object[] V_0,
               object[] V_1,
               object[] V_2,
               object[] V_3,
               uint8[] V_4,
               uint8[] V_5,
               uint8[] V_6,
               uint8[] V_7,
               int32[] V_8,
               int32[] V_9,
               int32[] V_10,
               int32[] V_11,
               int64[] V_12,
               int64[] V_13,
               int64[] V_14,
               int64[] V_15,
               char[] V_16,
               char[] V_17,
               char[] V_18,
               char[] V_19,
               float32[] V_20,
               float32[] V_21,
               float32[] V_22,
               float32[] V_23,
               float64[] V_24,
               float64[] V_25,
               float64[] V_26,
               float64[] V_27,
               class [mscorlib]System.Array V_28,
               class [mscorlib]System.Array V_29)
      IL_0000:  ldarg.s    arr1
      IL_0002:  isinst     object[]
      IL_0007:  stloc.0
      IL_0008:  ldloc.0
      IL_0009:  brfalse    IL_0fcd

      IL_000e:  ldarg.2
      IL_000f:  isinst     object[]
      IL_0014:  stloc.1
      IL_0015:  ldloc.1
      IL_0016:  brfalse.s  IL_0027

      IL_0018:  ldloc.1
      IL_0019:  stloc.2
      IL_001a:  ldloc.0
      IL_001b:  stloc.3
      IL_001c:  nop
      IL_001d:  ldarg.0
      IL_001e:  ldarg.1
      IL_001f:  ldloc.3
      IL_0020:  ldloc.2
      IL_0021:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityObjArray(bool,
                                                                                                              class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                              object[],
                                                                                                              object[])
      IL_0026:  ret

      IL_0027:  ldarg.s    arr1
      IL_0029:  isinst     uint8[]
      IL_002e:  stloc.s    V_4
      IL_0030:  ldloc.s    V_4
      IL_0032:  brfalse    IL_081e

      IL_0037:  ldarg.2
      IL_0038:  isinst     uint8[]
      IL_003d:  stloc.s    V_5
      IL_003f:  ldloc.s    V_5
      IL_0041:  brfalse.s  IL_0055

      IL_0043:  ldloc.s    V_5
      IL_0045:  stloc.s    V_6
      IL_0047:  ldloc.s    V_4
      IL_0049:  stloc.s    V_7
      IL_004b:  ldloc.s    V_7
      IL_004d:  ldloc.s    V_6
      IL_004f:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityByteArray(uint8[],
                                                                                                               uint8[])
      IL_0054:  ret

      IL_0055:  ldarg.s    arr1
      IL_0057:  isinst     int32[]
      IL_005c:  stloc.s    V_8
      IL_005e:  ldloc.s    V_8
      IL_0060:  brfalse    IL_045b

      IL_0065:  ldarg.2
      IL_0066:  isinst     int32[]
      IL_006b:  stloc.s    V_9
      IL_006d:  ldloc.s    V_9
      IL_006f:  brfalse.s  IL_0083

      IL_0071:  ldloc.s    V_9
      IL_0073:  stloc.s    V_10
      IL_0075:  ldloc.s    V_8
      IL_0077:  stloc.s    V_11
      IL_0079:  ldloc.s    V_11
      IL_007b:  ldloc.s    V_10
      IL_007d:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityInt32Array(int32[],
                                                                                                                int32[])
      IL_0082:  ret

      IL_0083:  ldarg.s    arr1
      IL_0085:  isinst     int64[]
      IL_008a:  stloc.s    V_12
      IL_008c:  ldloc.s    V_12
      IL_008e:  brfalse    IL_028e

      IL_0093:  ldarg.2
      IL_0094:  isinst     int64[]
      IL_0099:  stloc.s    V_13
      IL_009b:  ldloc.s    V_13
      IL_009d:  brfalse.s  IL_00b1

      IL_009f:  ldloc.s    V_13
      IL_00a1:  stloc.s    V_14
      IL_00a3:  ldloc.s    V_12
      IL_00a5:  stloc.s    V_15
      IL_00a7:  ldloc.s    V_15
      IL_00a9:  ldloc.s    V_14
      IL_00ab:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityInt64Array(int64[],
                                                                                                                int64[])
      IL_00b0:  ret

      IL_00b1:  ldarg.s    arr1
      IL_00b3:  isinst     char[]
      IL_00b8:  stloc.s    V_16
      IL_00ba:  ldloc.s    V_16
      IL_00bc:  brfalse    IL_01bc

      IL_00c1:  ldarg.2
      IL_00c2:  isinst     char[]
      IL_00c7:  stloc.s    V_17
      IL_00c9:  ldloc.s    V_17
      IL_00cb:  brfalse.s  IL_00df

      IL_00cd:  ldloc.s    V_17
      IL_00cf:  stloc.s    V_18
      IL_00d1:  ldloc.s    V_16
      IL_00d3:  stloc.s    V_19
      IL_00d5:  ldloc.s    V_19
      IL_00d7:  ldloc.s    V_18
      IL_00d9:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityCharArray(char[],
                                                                                                               char[])
      IL_00de:  ret

      IL_00df:  ldarg.s    arr1
      IL_00e1:  isinst     float32[]
      IL_00e6:  stloc.s    V_20
      IL_00e8:  ldloc.s    V_20
      IL_00ea:  brfalse    IL_0172

      IL_00ef:  ldarg.2
      IL_00f0:  isinst     float32[]
      IL_00f5:  stloc.s    V_21
      IL_00f7:  ldloc.s    V_21
      IL_00f9:  brfalse.s  IL_010e

      IL_00fb:  ldloc.s    V_21
      IL_00fd:  stloc.s    V_22
      IL_00ff:  ldloc.s    V_20
      IL_0101:  stloc.s    V_23
      IL_0103:  ldarg.0
      IL_0104:  ldloc.s    V_23
      IL_0106:  ldloc.s    V_22
      IL_0108:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualitySingleArray(bool,
                                                                                                                 float32[],
                                                                                                                 float32[])
      IL_010d:  ret

      IL_010e:  ldarg.s    arr1
      IL_0110:  isinst     float64[]
      IL_0115:  stloc.s    V_24
      IL_0117:  ldloc.s    V_24
      IL_0119:  brfalse.s  IL_0160

      IL_011b:  ldarg.2
      IL_011c:  isinst     float64[]
      IL_0121:  stloc.s    V_25
      IL_0123:  ldloc.s    V_25
      IL_0125:  brfalse.s  IL_013a

      IL_0127:  ldloc.s    V_25
      IL_0129:  stloc.s    V_26
      IL_012b:  ldloc.s    V_24
      IL_012d:  stloc.s    V_27
      IL_012f:  ldarg.0
      IL_0130:  ldloc.s    V_27
      IL_0132:  ldloc.s    V_26
      IL_0134:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityDoubleArray(bool,
                                                                                                                 float64[],
                                                                                                                 float64[])
      IL_0139:  ret

      IL_013a:  ldarg.2
      IL_013b:  isinst     [mscorlib]System.Array
      IL_0140:  stloc.s    V_28
      IL_0142:  ldloc.s    V_28
      IL_0144:  brfalse.s  IL_0156

      IL_0146:  ldloc.s    V_28
      IL_0148:  stloc.s    V_29
      IL_014a:  ldarg.0
      IL_014b:  ldarg.1
      IL_014c:  ldarg.s    arr1
      IL_014e:  ldloc.s    V_29
      IL_0150:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityArbArray(bool,
                                                                                                              class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                              class [mscorlib]System.Array,
                                                                                                              class [mscorlib]System.Array)
      IL_0155:  ret

      IL_0156:  ldarg.3
      IL_0157:  ldarg.2
      IL_0158:  tail.
      IL_015a:  callvirt   instance bool [mscorlib]System.Object::Equals(object)
      IL_015f:  ret

      IL_0160:  ldarg.2
      IL_0161:  isinst     [mscorlib]System.Array
      IL_0166:  stloc.s    V_28
      IL_0168:  ldloc.s    V_28
      IL_016a:  brfalse.s  IL_0156

      IL_016c:  ldloc.s    V_28
      IL_016e:  stloc.s    V_29
      IL_0170:  br.s       IL_014a

      IL_0172:  ldarg.s    arr1
      IL_0174:  isinst     float64[]
      IL_0179:  stloc.s    V_24
      IL_017b:  ldloc.s    V_24
      IL_017d:  brfalse.s  IL_01a7

      IL_017f:  ldarg.2
      IL_0180:  isinst     float64[]
      IL_0185:  stloc.s    V_25
      IL_0187:  ldloc.s    V_25
      IL_0189:  brfalse.s  IL_0195

      IL_018b:  ldloc.s    V_25
      IL_018d:  ldloc.s    V_24
      IL_018f:  stloc.s    V_27
      IL_0191:  stloc.s    V_26
      IL_0193:  br.s       IL_012f

      IL_0195:  ldarg.2
      IL_0196:  isinst     [mscorlib]System.Array
      IL_019b:  stloc.s    V_28
      IL_019d:  ldloc.s    V_28
      IL_019f:  brfalse.s  IL_0156

      IL_01a1:  ldloc.s    V_28
      IL_01a3:  stloc.s    V_29
      IL_01a5:  br.s       IL_014a

      IL_01a7:  ldarg.2
      IL_01a8:  isinst     [mscorlib]System.Array
      IL_01ad:  stloc.s    V_28
      IL_01af:  ldloc.s    V_28
      IL_01b1:  brfalse.s  IL_0156

      IL_01b3:  ldloc.s    V_28
      IL_01b5:  stloc.s    V_29
      IL_01b7:  br         IL_014a

      IL_01bc:  ldarg.s    arr1
      IL_01be:  isinst     float32[]
      IL_01c3:  stloc.s    V_20
      IL_01c5:  ldloc.s    V_20
      IL_01c7:  brfalse.s  IL_0238

      IL_01c9:  ldarg.2
      IL_01ca:  isinst     float32[]
      IL_01cf:  stloc.s    V_21
      IL_01d1:  ldloc.s    V_21
      IL_01d3:  brfalse.s  IL_01e2

      IL_01d5:  ldloc.s    V_21
      IL_01d7:  ldloc.s    V_20
      IL_01d9:  stloc.s    V_23
      IL_01db:  stloc.s    V_22
      IL_01dd:  br         IL_0103

      IL_01e2:  ldarg.s    arr1
      IL_01e4:  isinst     float64[]
      IL_01e9:  stloc.s    V_24
      IL_01eb:  ldloc.s    V_24
      IL_01ed:  brfalse.s  IL_0220

      IL_01ef:  ldarg.2
      IL_01f0:  isinst     float64[]
      IL_01f5:  stloc.s    V_25
      IL_01f7:  ldloc.s    V_25
      IL_01f9:  brfalse.s  IL_0208

      IL_01fb:  ldloc.s    V_25
      IL_01fd:  ldloc.s    V_24
      IL_01ff:  stloc.s    V_27
      IL_0201:  stloc.s    V_26
      IL_0203:  br         IL_012f

      IL_0208:  ldarg.2
      IL_0209:  isinst     [mscorlib]System.Array
      IL_020e:  stloc.s    V_28
      IL_0210:  ldloc.s    V_28
      IL_0212:  brfalse    IL_0156

      IL_0217:  ldloc.s    V_28
      IL_0219:  stloc.s    V_29
      IL_021b:  br         IL_014a

      IL_0220:  ldarg.2
      IL_0221:  isinst     [mscorlib]System.Array
      IL_0226:  stloc.s    V_28
      IL_0228:  ldloc.s    V_28
      IL_022a:  brfalse    IL_0156

      IL_022f:  ldloc.s    V_28
      IL_0231:  stloc.s    V_29
      IL_0233:  br         IL_014a

      IL_0238:  ldarg.s    arr1
      IL_023a:  isinst     float64[]
      IL_023f:  stloc.s    V_24
      IL_0241:  ldloc.s    V_24
      IL_0243:  brfalse.s  IL_0276

      IL_0245:  ldarg.2
      IL_0246:  isinst     float64[]
      IL_024b:  stloc.s    V_25
      IL_024d:  ldloc.s    V_25
      IL_024f:  brfalse.s  IL_025e

      IL_0251:  ldloc.s    V_25
      IL_0253:  ldloc.s    V_24
      IL_0255:  stloc.s    V_27
      IL_0257:  stloc.s    V_26
      IL_0259:  br         IL_012f

      IL_025e:  ldarg.2
      IL_025f:  isinst     [mscorlib]System.Array
      IL_0264:  stloc.s    V_28
      IL_0266:  ldloc.s    V_28
      IL_0268:  brfalse    IL_0156

      IL_026d:  ldloc.s    V_28
      IL_026f:  stloc.s    V_29
      IL_0271:  br         IL_014a

      IL_0276:  ldarg.2
      IL_0277:  isinst     [mscorlib]System.Array
      IL_027c:  stloc.s    V_28
      IL_027e:  ldloc.s    V_28
      IL_0280:  brfalse    IL_0156

      IL_0285:  ldloc.s    V_28
      IL_0287:  stloc.s    V_29
      IL_0289:  br         IL_014a

      IL_028e:  ldarg.s    arr1
      IL_0290:  isinst     char[]
      IL_0295:  stloc.s    V_16
      IL_0297:  ldloc.s    V_16
      IL_0299:  brfalse    IL_0389

      IL_029e:  ldarg.2
      IL_029f:  isinst     char[]
      IL_02a4:  stloc.s    V_17
      IL_02a6:  ldloc.s    V_17
      IL_02a8:  brfalse.s  IL_02b7

      IL_02aa:  ldloc.s    V_17
      IL_02ac:  ldloc.s    V_16
      IL_02ae:  stloc.s    V_19
      IL_02b0:  stloc.s    V_18
      IL_02b2:  br         IL_00d5

      IL_02b7:  ldarg.s    arr1
      IL_02b9:  isinst     float32[]
      IL_02be:  stloc.s    V_20
      IL_02c0:  ldloc.s    V_20
      IL_02c2:  brfalse.s  IL_0333

      IL_02c4:  ldarg.2
      IL_02c5:  isinst     float32[]
      IL_02ca:  stloc.s    V_21
      IL_02cc:  ldloc.s    V_21
      IL_02ce:  brfalse.s  IL_02dd

      IL_02d0:  ldloc.s    V_21
      IL_02d2:  ldloc.s    V_20
      IL_02d4:  stloc.s    V_23
      IL_02d6:  stloc.s    V_22
      IL_02d8:  br         IL_0103

      IL_02dd:  ldarg.s    arr1
      IL_02df:  isinst     float64[]
      IL_02e4:  stloc.s    V_24
      IL_02e6:  ldloc.s    V_24
      IL_02e8:  brfalse.s  IL_031b

      IL_02ea:  ldarg.2
      IL_02eb:  isinst     float64[]
      IL_02f0:  stloc.s    V_25
      IL_02f2:  ldloc.s    V_25
      IL_02f4:  brfalse.s  IL_0303

      IL_02f6:  ldloc.s    V_25
      IL_02f8:  ldloc.s    V_24
      IL_02fa:  stloc.s    V_27
      IL_02fc:  stloc.s    V_26
      IL_02fe:  br         IL_012f

      IL_0303:  ldarg.2
      IL_0304:  isinst     [mscorlib]System.Array
      IL_0309:  stloc.s    V_28
      IL_030b:  ldloc.s    V_28
      IL_030d:  brfalse    IL_0156

      IL_0312:  ldloc.s    V_28
      IL_0314:  stloc.s    V_29
      IL_0316:  br         IL_014a

      IL_031b:  ldarg.2
      IL_031c:  isinst     [mscorlib]System.Array
      IL_0321:  stloc.s    V_28
      IL_0323:  ldloc.s    V_28
      IL_0325:  brfalse    IL_0156

      IL_032a:  ldloc.s    V_28
      IL_032c:  stloc.s    V_29
      IL_032e:  br         IL_014a

      IL_0333:  ldarg.s    arr1
      IL_0335:  isinst     float64[]
      IL_033a:  stloc.s    V_24
      IL_033c:  ldloc.s    V_24
      IL_033e:  brfalse.s  IL_0371

      IL_0340:  ldarg.2
      IL_0341:  isinst     float64[]
      IL_0346:  stloc.s    V_25
      IL_0348:  ldloc.s    V_25
      IL_034a:  brfalse.s  IL_0359

      IL_034c:  ldloc.s    V_25
      IL_034e:  ldloc.s    V_24
      IL_0350:  stloc.s    V_27
      IL_0352:  stloc.s    V_26
      IL_0354:  br         IL_012f

      IL_0359:  ldarg.2
      IL_035a:  isinst     [mscorlib]System.Array
      IL_035f:  stloc.s    V_28
      IL_0361:  ldloc.s    V_28
      IL_0363:  brfalse    IL_0156

      IL_0368:  ldloc.s    V_28
      IL_036a:  stloc.s    V_29
      IL_036c:  br         IL_014a

      IL_0371:  ldarg.2
      IL_0372:  isinst     [mscorlib]System.Array
      IL_0377:  stloc.s    V_28
      IL_0379:  ldloc.s    V_28
      IL_037b:  brfalse    IL_0156

      IL_0380:  ldloc.s    V_28
      IL_0382:  stloc.s    V_29
      IL_0384:  br         IL_014a

      IL_0389:  ldarg.s    arr1
      IL_038b:  isinst     float32[]
      IL_0390:  stloc.s    V_20
      IL_0392:  ldloc.s    V_20
      IL_0394:  brfalse.s  IL_0405

      IL_0396:  ldarg.2
      IL_0397:  isinst     float32[]
      IL_039c:  stloc.s    V_21
      IL_039e:  ldloc.s    V_21
      IL_03a0:  brfalse.s  IL_03af

      IL_03a2:  ldloc.s    V_21
      IL_03a4:  ldloc.s    V_20
      IL_03a6:  stloc.s    V_23
      IL_03a8:  stloc.s    V_22
      IL_03aa:  br         IL_0103

      IL_03af:  ldarg.s    arr1
      IL_03b1:  isinst     float64[]
      IL_03b6:  stloc.s    V_24
      IL_03b8:  ldloc.s    V_24
      IL_03ba:  brfalse.s  IL_03ed

      IL_03bc:  ldarg.2
      IL_03bd:  isinst     float64[]
      IL_03c2:  stloc.s    V_25
      IL_03c4:  ldloc.s    V_25
      IL_03c6:  brfalse.s  IL_03d5

      IL_03c8:  ldloc.s    V_25
      IL_03ca:  ldloc.s    V_24
      IL_03cc:  stloc.s    V_27
      IL_03ce:  stloc.s    V_26
      IL_03d0:  br         IL_012f

      IL_03d5:  ldarg.2
      IL_03d6:  isinst     [mscorlib]System.Array
      IL_03db:  stloc.s    V_28
      IL_03dd:  ldloc.s    V_28
      IL_03df:  brfalse    IL_0156

      IL_03e4:  ldloc.s    V_28
      IL_03e6:  stloc.s    V_29
      IL_03e8:  br         IL_014a

      IL_03ed:  ldarg.2
      IL_03ee:  isinst     [mscorlib]System.Array
      IL_03f3:  stloc.s    V_28
      IL_03f5:  ldloc.s    V_28
      IL_03f7:  brfalse    IL_0156

      IL_03fc:  ldloc.s    V_28
      IL_03fe:  stloc.s    V_29
      IL_0400:  br         IL_014a

      IL_0405:  ldarg.s    arr1
      IL_0407:  isinst     float64[]
      IL_040c:  stloc.s    V_24
      IL_040e:  ldloc.s    V_24
      IL_0410:  brfalse.s  IL_0443

      IL_0412:  ldarg.2
      IL_0413:  isinst     float64[]
      IL_0418:  stloc.s    V_25
      IL_041a:  ldloc.s    V_25
      IL_041c:  brfalse.s  IL_042b

      IL_041e:  ldloc.s    V_25
      IL_0420:  ldloc.s    V_24
      IL_0422:  stloc.s    V_27
      IL_0424:  stloc.s    V_26
      IL_0426:  br         IL_012f

      IL_042b:  ldarg.2
      IL_042c:  isinst     [mscorlib]System.Array
      IL_0431:  stloc.s    V_28
      IL_0433:  ldloc.s    V_28
      IL_0435:  brfalse    IL_0156

      IL_043a:  ldloc.s    V_28
      IL_043c:  stloc.s    V_29
      IL_043e:  br         IL_014a

      IL_0443:  ldarg.2
      IL_0444:  isinst     [mscorlib]System.Array
      IL_0449:  stloc.s    V_28
      IL_044b:  ldloc.s    V_28
      IL_044d:  brfalse    IL_0156

      IL_0452:  ldloc.s    V_28
      IL_0454:  stloc.s    V_29
      IL_0456:  br         IL_014a

      IL_045b:  ldarg.s    arr1
      IL_045d:  isinst     int64[]
      IL_0462:  stloc.s    V_12
      IL_0464:  ldloc.s    V_12
      IL_0466:  brfalse    IL_0651

      IL_046b:  ldarg.2
      IL_046c:  isinst     int64[]
      IL_0471:  stloc.s    V_13
      IL_0473:  ldloc.s    V_13
      IL_0475:  brfalse.s  IL_0484

      IL_0477:  ldloc.s    V_13
      IL_0479:  ldloc.s    V_12
      IL_047b:  stloc.s    V_15
      IL_047d:  stloc.s    V_14
      IL_047f:  br         IL_00a7

      IL_0484:  ldarg.s    arr1
      IL_0486:  isinst     char[]
      IL_048b:  stloc.s    V_16
      IL_048d:  ldloc.s    V_16
      IL_048f:  brfalse    IL_057f

      IL_0494:  ldarg.2
      IL_0495:  isinst     char[]
      IL_049a:  stloc.s    V_17
      IL_049c:  ldloc.s    V_17
      IL_049e:  brfalse.s  IL_04ad

      IL_04a0:  ldloc.s    V_17
      IL_04a2:  ldloc.s    V_16
      IL_04a4:  stloc.s    V_19
      IL_04a6:  stloc.s    V_18
      IL_04a8:  br         IL_00d5

      IL_04ad:  ldarg.s    arr1
      IL_04af:  isinst     float32[]
      IL_04b4:  stloc.s    V_20
      IL_04b6:  ldloc.s    V_20
      IL_04b8:  brfalse.s  IL_0529

      IL_04ba:  ldarg.2
      IL_04bb:  isinst     float32[]
      IL_04c0:  stloc.s    V_21
      IL_04c2:  ldloc.s    V_21
      IL_04c4:  brfalse.s  IL_04d3

      IL_04c6:  ldloc.s    V_21
      IL_04c8:  ldloc.s    V_20
      IL_04ca:  stloc.s    V_23
      IL_04cc:  stloc.s    V_22
      IL_04ce:  br         IL_0103

      IL_04d3:  ldarg.s    arr1
      IL_04d5:  isinst     float64[]
      IL_04da:  stloc.s    V_24
      IL_04dc:  ldloc.s    V_24
      IL_04de:  brfalse.s  IL_0511

      IL_04e0:  ldarg.2
      IL_04e1:  isinst     float64[]
      IL_04e6:  stloc.s    V_25
      IL_04e8:  ldloc.s    V_25
      IL_04ea:  brfalse.s  IL_04f9

      IL_04ec:  ldloc.s    V_25
      IL_04ee:  ldloc.s    V_24
      IL_04f0:  stloc.s    V_27
      IL_04f2:  stloc.s    V_26
      IL_04f4:  br         IL_012f

      IL_04f9:  ldarg.2
      IL_04fa:  isinst     [mscorlib]System.Array
      IL_04ff:  stloc.s    V_28
      IL_0501:  ldloc.s    V_28
      IL_0503:  brfalse    IL_0156

      IL_0508:  ldloc.s    V_28
      IL_050a:  stloc.s    V_29
      IL_050c:  br         IL_014a

      IL_0511:  ldarg.2
      IL_0512:  isinst     [mscorlib]System.Array
      IL_0517:  stloc.s    V_28
      IL_0519:  ldloc.s    V_28
      IL_051b:  brfalse    IL_0156

      IL_0520:  ldloc.s    V_28
      IL_0522:  stloc.s    V_29
      IL_0524:  br         IL_014a

      IL_0529:  ldarg.s    arr1
      IL_052b:  isinst     float64[]
      IL_0530:  stloc.s    V_24
      IL_0532:  ldloc.s    V_24
      IL_0534:  brfalse.s  IL_0567

      IL_0536:  ldarg.2
      IL_0537:  isinst     float64[]
      IL_053c:  stloc.s    V_25
      IL_053e:  ldloc.s    V_25
      IL_0540:  brfalse.s  IL_054f

      IL_0542:  ldloc.s    V_25
      IL_0544:  ldloc.s    V_24
      IL_0546:  stloc.s    V_27
      IL_0548:  stloc.s    V_26
      IL_054a:  br         IL_012f

      IL_054f:  ldarg.2
      IL_0550:  isinst     [mscorlib]System.Array
      IL_0555:  stloc.s    V_28
      IL_0557:  ldloc.s    V_28
      IL_0559:  brfalse    IL_0156

      IL_055e:  ldloc.s    V_28
      IL_0560:  stloc.s    V_29
      IL_0562:  br         IL_014a

      IL_0567:  ldarg.2
      IL_0568:  isinst     [mscorlib]System.Array
      IL_056d:  stloc.s    V_28
      IL_056f:  ldloc.s    V_28
      IL_0571:  brfalse    IL_0156

      IL_0576:  ldloc.s    V_28
      IL_0578:  stloc.s    V_29
      IL_057a:  br         IL_014a

      IL_057f:  ldarg.s    arr1
      IL_0581:  isinst     float32[]
      IL_0586:  stloc.s    V_20
      IL_0588:  ldloc.s    V_20
      IL_058a:  brfalse.s  IL_05fb

      IL_058c:  ldarg.2
      IL_058d:  isinst     float32[]
      IL_0592:  stloc.s    V_21
      IL_0594:  ldloc.s    V_21
      IL_0596:  brfalse.s  IL_05a5

      IL_0598:  ldloc.s    V_21
      IL_059a:  ldloc.s    V_20
      IL_059c:  stloc.s    V_23
      IL_059e:  stloc.s    V_22
      IL_05a0:  br         IL_0103

      IL_05a5:  ldarg.s    arr1
      IL_05a7:  isinst     float64[]
      IL_05ac:  stloc.s    V_24
      IL_05ae:  ldloc.s    V_24
      IL_05b0:  brfalse.s  IL_05e3

      IL_05b2:  ldarg.2
      IL_05b3:  isinst     float64[]
      IL_05b8:  stloc.s    V_25
      IL_05ba:  ldloc.s    V_25
      IL_05bc:  brfalse.s  IL_05cb

      IL_05be:  ldloc.s    V_25
      IL_05c0:  ldloc.s    V_24
      IL_05c2:  stloc.s    V_27
      IL_05c4:  stloc.s    V_26
      IL_05c6:  br         IL_012f

      IL_05cb:  ldarg.2
      IL_05cc:  isinst     [mscorlib]System.Array
      IL_05d1:  stloc.s    V_28
      IL_05d3:  ldloc.s    V_28
      IL_05d5:  brfalse    IL_0156

      IL_05da:  ldloc.s    V_28
      IL_05dc:  stloc.s    V_29
      IL_05de:  br         IL_014a

      IL_05e3:  ldarg.2
      IL_05e4:  isinst     [mscorlib]System.Array
      IL_05e9:  stloc.s    V_28
      IL_05eb:  ldloc.s    V_28
      IL_05ed:  brfalse    IL_0156

      IL_05f2:  ldloc.s    V_28
      IL_05f4:  stloc.s    V_29
      IL_05f6:  br         IL_014a

      IL_05fb:  ldarg.s    arr1
      IL_05fd:  isinst     float64[]
      IL_0602:  stloc.s    V_24
      IL_0604:  ldloc.s    V_24
      IL_0606:  brfalse.s  IL_0639

      IL_0608:  ldarg.2
      IL_0609:  isinst     float64[]
      IL_060e:  stloc.s    V_25
      IL_0610:  ldloc.s    V_25
      IL_0612:  brfalse.s  IL_0621

      IL_0614:  ldloc.s    V_25
      IL_0616:  ldloc.s    V_24
      IL_0618:  stloc.s    V_27
      IL_061a:  stloc.s    V_26
      IL_061c:  br         IL_012f

      IL_0621:  ldarg.2
      IL_0622:  isinst     [mscorlib]System.Array
      IL_0627:  stloc.s    V_28
      IL_0629:  ldloc.s    V_28
      IL_062b:  brfalse    IL_0156

      IL_0630:  ldloc.s    V_28
      IL_0632:  stloc.s    V_29
      IL_0634:  br         IL_014a

      IL_0639:  ldarg.2
      IL_063a:  isinst     [mscorlib]System.Array
      IL_063f:  stloc.s    V_28
      IL_0641:  ldloc.s    V_28
      IL_0643:  brfalse    IL_0156

      IL_0648:  ldloc.s    V_28
      IL_064a:  stloc.s    V_29
      IL_064c:  br         IL_014a

      IL_0651:  ldarg.s    arr1
      IL_0653:  isinst     char[]
      IL_0658:  stloc.s    V_16
      IL_065a:  ldloc.s    V_16
      IL_065c:  brfalse    IL_074c

      IL_0661:  ldarg.2
      IL_0662:  isinst     char[]
      IL_0667:  stloc.s    V_17
      IL_0669:  ldloc.s    V_17
      IL_066b:  brfalse.s  IL_067a

      IL_066d:  ldloc.s    V_17
      IL_066f:  ldloc.s    V_16
      IL_0671:  stloc.s    V_19
      IL_0673:  stloc.s    V_18
      IL_0675:  br         IL_00d5

      IL_067a:  ldarg.s    arr1
      IL_067c:  isinst     float32[]
      IL_0681:  stloc.s    V_20
      IL_0683:  ldloc.s    V_20
      IL_0685:  brfalse.s  IL_06f6

      IL_0687:  ldarg.2
      IL_0688:  isinst     float32[]
      IL_068d:  stloc.s    V_21
      IL_068f:  ldloc.s    V_21
      IL_0691:  brfalse.s  IL_06a0

      IL_0693:  ldloc.s    V_21
      IL_0695:  ldloc.s    V_20
      IL_0697:  stloc.s    V_23
      IL_0699:  stloc.s    V_22
      IL_069b:  br         IL_0103

      IL_06a0:  ldarg.s    arr1
      IL_06a2:  isinst     float64[]
      IL_06a7:  stloc.s    V_24
      IL_06a9:  ldloc.s    V_24
      IL_06ab:  brfalse.s  IL_06de

      IL_06ad:  ldarg.2
      IL_06ae:  isinst     float64[]
      IL_06b3:  stloc.s    V_25
      IL_06b5:  ldloc.s    V_25
      IL_06b7:  brfalse.s  IL_06c6

      IL_06b9:  ldloc.s    V_25
      IL_06bb:  ldloc.s    V_24
      IL_06bd:  stloc.s    V_27
      IL_06bf:  stloc.s    V_26
      IL_06c1:  br         IL_012f

      IL_06c6:  ldarg.2
      IL_06c7:  isinst     [mscorlib]System.Array
      IL_06cc:  stloc.s    V_28
      IL_06ce:  ldloc.s    V_28
      IL_06d0:  brfalse    IL_0156

      IL_06d5:  ldloc.s    V_28
      IL_06d7:  stloc.s    V_29
      IL_06d9:  br         IL_014a

      IL_06de:  ldarg.2
      IL_06df:  isinst     [mscorlib]System.Array
      IL_06e4:  stloc.s    V_28
      IL_06e6:  ldloc.s    V_28
      IL_06e8:  brfalse    IL_0156

      IL_06ed:  ldloc.s    V_28
      IL_06ef:  stloc.s    V_29
      IL_06f1:  br         IL_014a

      IL_06f6:  ldarg.s    arr1
      IL_06f8:  isinst     float64[]
      IL_06fd:  stloc.s    V_24
      IL_06ff:  ldloc.s    V_24
      IL_0701:  brfalse.s  IL_0734

      IL_0703:  ldarg.2
      IL_0704:  isinst     float64[]
      IL_0709:  stloc.s    V_25
      IL_070b:  ldloc.s    V_25
      IL_070d:  brfalse.s  IL_071c

      IL_070f:  ldloc.s    V_25
      IL_0711:  ldloc.s    V_24
      IL_0713:  stloc.s    V_27
      IL_0715:  stloc.s    V_26
      IL_0717:  br         IL_012f

      IL_071c:  ldarg.2
      IL_071d:  isinst     [mscorlib]System.Array
      IL_0722:  stloc.s    V_28
      IL_0724:  ldloc.s    V_28
      IL_0726:  brfalse    IL_0156

      IL_072b:  ldloc.s    V_28
      IL_072d:  stloc.s    V_29
      IL_072f:  br         IL_014a

      IL_0734:  ldarg.2
      IL_0735:  isinst     [mscorlib]System.Array
      IL_073a:  stloc.s    V_28
      IL_073c:  ldloc.s    V_28
      IL_073e:  brfalse    IL_0156

      IL_0743:  ldloc.s    V_28
      IL_0745:  stloc.s    V_29
      IL_0747:  br         IL_014a

      IL_074c:  ldarg.s    arr1
      IL_074e:  isinst     float32[]
      IL_0753:  stloc.s    V_20
      IL_0755:  ldloc.s    V_20
      IL_0757:  brfalse.s  IL_07c8

      IL_0759:  ldarg.2
      IL_075a:  isinst     float32[]
      IL_075f:  stloc.s    V_21
      IL_0761:  ldloc.s    V_21
      IL_0763:  brfalse.s  IL_0772

      IL_0765:  ldloc.s    V_21
      IL_0767:  ldloc.s    V_20
      IL_0769:  stloc.s    V_23
      IL_076b:  stloc.s    V_22
      IL_076d:  br         IL_0103

      IL_0772:  ldarg.s    arr1
      IL_0774:  isinst     float64[]
      IL_0779:  stloc.s    V_24
      IL_077b:  ldloc.s    V_24
      IL_077d:  brfalse.s  IL_07b0

      IL_077f:  ldarg.2
      IL_0780:  isinst     float64[]
      IL_0785:  stloc.s    V_25
      IL_0787:  ldloc.s    V_25
      IL_0789:  brfalse.s  IL_0798

      IL_078b:  ldloc.s    V_25
      IL_078d:  ldloc.s    V_24
      IL_078f:  stloc.s    V_27
      IL_0791:  stloc.s    V_26
      IL_0793:  br         IL_012f

      IL_0798:  ldarg.2
      IL_0799:  isinst     [mscorlib]System.Array
      IL_079e:  stloc.s    V_28
      IL_07a0:  ldloc.s    V_28
      IL_07a2:  brfalse    IL_0156

      IL_07a7:  ldloc.s    V_28
      IL_07a9:  stloc.s    V_29
      IL_07ab:  br         IL_014a

      IL_07b0:  ldarg.2
      IL_07b1:  isinst     [mscorlib]System.Array
      IL_07b6:  stloc.s    V_28
      IL_07b8:  ldloc.s    V_28
      IL_07ba:  brfalse    IL_0156

      IL_07bf:  ldloc.s    V_28
      IL_07c1:  stloc.s    V_29
      IL_07c3:  br         IL_014a

      IL_07c8:  ldarg.s    arr1
      IL_07ca:  isinst     float64[]
      IL_07cf:  stloc.s    V_24
      IL_07d1:  ldloc.s    V_24
      IL_07d3:  brfalse.s  IL_0806

      IL_07d5:  ldarg.2
      IL_07d6:  isinst     float64[]
      IL_07db:  stloc.s    V_25
      IL_07dd:  ldloc.s    V_25
      IL_07df:  brfalse.s  IL_07ee

      IL_07e1:  ldloc.s    V_25
      IL_07e3:  ldloc.s    V_24
      IL_07e5:  stloc.s    V_27
      IL_07e7:  stloc.s    V_26
      IL_07e9:  br         IL_012f

      IL_07ee:  ldarg.2
      IL_07ef:  isinst     [mscorlib]System.Array
      IL_07f4:  stloc.s    V_28
      IL_07f6:  ldloc.s    V_28
      IL_07f8:  brfalse    IL_0156

      IL_07fd:  ldloc.s    V_28
      IL_07ff:  stloc.s    V_29
      IL_0801:  br         IL_014a

      IL_0806:  ldarg.2
      IL_0807:  isinst     [mscorlib]System.Array
      IL_080c:  stloc.s    V_28
      IL_080e:  ldloc.s    V_28
      IL_0810:  brfalse    IL_0156

      IL_0815:  ldloc.s    V_28
      IL_0817:  stloc.s    V_29
      IL_0819:  br         IL_014a

      IL_081e:  ldarg.s    arr1
      IL_0820:  isinst     int32[]
      IL_0825:  stloc.s    V_8
      IL_0827:  ldloc.s    V_8
      IL_0829:  brfalse    IL_0c0a

      IL_082e:  ldarg.2
      IL_082f:  isinst     int32[]
      IL_0834:  stloc.s    V_9
      IL_0836:  ldloc.s    V_9
      IL_0838:  brfalse.s  IL_0847

      IL_083a:  ldloc.s    V_9
      IL_083c:  ldloc.s    V_8
      IL_083e:  stloc.s    V_11
      IL_0840:  stloc.s    V_10
      IL_0842:  br         IL_0079

      IL_0847:  ldarg.s    arr1
      IL_0849:  isinst     int64[]
      IL_084e:  stloc.s    V_12
      IL_0850:  ldloc.s    V_12
      IL_0852:  brfalse    IL_0a3d

      IL_0857:  ldarg.2
      IL_0858:  isinst     int64[]
      IL_085d:  stloc.s    V_13
      IL_085f:  ldloc.s    V_13
      IL_0861:  brfalse.s  IL_0870

      IL_0863:  ldloc.s    V_13
      IL_0865:  ldloc.s    V_12
      IL_0867:  stloc.s    V_15
      IL_0869:  stloc.s    V_14
      IL_086b:  br         IL_00a7

      IL_0870:  ldarg.s    arr1
      IL_0872:  isinst     char[]
      IL_0877:  stloc.s    V_16
      IL_0879:  ldloc.s    V_16
      IL_087b:  brfalse    IL_096b

      IL_0880:  ldarg.2
      IL_0881:  isinst     char[]
      IL_0886:  stloc.s    V_17
      IL_0888:  ldloc.s    V_17
      IL_088a:  brfalse.s  IL_0899

      IL_088c:  ldloc.s    V_17
      IL_088e:  ldloc.s    V_16
      IL_0890:  stloc.s    V_19
      IL_0892:  stloc.s    V_18
      IL_0894:  br         IL_00d5

      IL_0899:  ldarg.s    arr1
      IL_089b:  isinst     float32[]
      IL_08a0:  stloc.s    V_20
      IL_08a2:  ldloc.s    V_20
      IL_08a4:  brfalse.s  IL_0915

      IL_08a6:  ldarg.2
      IL_08a7:  isinst     float32[]
      IL_08ac:  stloc.s    V_21
      IL_08ae:  ldloc.s    V_21
      IL_08b0:  brfalse.s  IL_08bf

      IL_08b2:  ldloc.s    V_21
      IL_08b4:  ldloc.s    V_20
      IL_08b6:  stloc.s    V_23
      IL_08b8:  stloc.s    V_22
      IL_08ba:  br         IL_0103

      IL_08bf:  ldarg.s    arr1
      IL_08c1:  isinst     float64[]
      IL_08c6:  stloc.s    V_24
      IL_08c8:  ldloc.s    V_24
      IL_08ca:  brfalse.s  IL_08fd

      IL_08cc:  ldarg.2
      IL_08cd:  isinst     float64[]
      IL_08d2:  stloc.s    V_25
      IL_08d4:  ldloc.s    V_25
      IL_08d6:  brfalse.s  IL_08e5

      IL_08d8:  ldloc.s    V_25
      IL_08da:  ldloc.s    V_24
      IL_08dc:  stloc.s    V_27
      IL_08de:  stloc.s    V_26
      IL_08e0:  br         IL_012f

      IL_08e5:  ldarg.2
      IL_08e6:  isinst     [mscorlib]System.Array
      IL_08eb:  stloc.s    V_28
      IL_08ed:  ldloc.s    V_28
      IL_08ef:  brfalse    IL_0156

      IL_08f4:  ldloc.s    V_28
      IL_08f6:  stloc.s    V_29
      IL_08f8:  br         IL_014a

      IL_08fd:  ldarg.2
      IL_08fe:  isinst     [mscorlib]System.Array
      IL_0903:  stloc.s    V_28
      IL_0905:  ldloc.s    V_28
      IL_0907:  brfalse    IL_0156

      IL_090c:  ldloc.s    V_28
      IL_090e:  stloc.s    V_29
      IL_0910:  br         IL_014a

      IL_0915:  ldarg.s    arr1
      IL_0917:  isinst     float64[]
      IL_091c:  stloc.s    V_24
      IL_091e:  ldloc.s    V_24
      IL_0920:  brfalse.s  IL_0953

      IL_0922:  ldarg.2
      IL_0923:  isinst     float64[]
      IL_0928:  stloc.s    V_25
      IL_092a:  ldloc.s    V_25
      IL_092c:  brfalse.s  IL_093b

      IL_092e:  ldloc.s    V_25
      IL_0930:  ldloc.s    V_24
      IL_0932:  stloc.s    V_27
      IL_0934:  stloc.s    V_26
      IL_0936:  br         IL_012f

      IL_093b:  ldarg.2
      IL_093c:  isinst     [mscorlib]System.Array
      IL_0941:  stloc.s    V_28
      IL_0943:  ldloc.s    V_28
      IL_0945:  brfalse    IL_0156

      IL_094a:  ldloc.s    V_28
      IL_094c:  stloc.s    V_29
      IL_094e:  br         IL_014a

      IL_0953:  ldarg.2
      IL_0954:  isinst     [mscorlib]System.Array
      IL_0959:  stloc.s    V_28
      IL_095b:  ldloc.s    V_28
      IL_095d:  brfalse    IL_0156

      IL_0962:  ldloc.s    V_28
      IL_0964:  stloc.s    V_29
      IL_0966:  br         IL_014a

      IL_096b:  ldarg.s    arr1
      IL_096d:  isinst     float32[]
      IL_0972:  stloc.s    V_20
      IL_0974:  ldloc.s    V_20
      IL_0976:  brfalse.s  IL_09e7

      IL_0978:  ldarg.2
      IL_0979:  isinst     float32[]
      IL_097e:  stloc.s    V_21
      IL_0980:  ldloc.s    V_21
      IL_0982:  brfalse.s  IL_0991

      IL_0984:  ldloc.s    V_21
      IL_0986:  ldloc.s    V_20
      IL_0988:  stloc.s    V_23
      IL_098a:  stloc.s    V_22
      IL_098c:  br         IL_0103

      IL_0991:  ldarg.s    arr1
      IL_0993:  isinst     float64[]
      IL_0998:  stloc.s    V_24
      IL_099a:  ldloc.s    V_24
      IL_099c:  brfalse.s  IL_09cf

      IL_099e:  ldarg.2
      IL_099f:  isinst     float64[]
      IL_09a4:  stloc.s    V_25
      IL_09a6:  ldloc.s    V_25
      IL_09a8:  brfalse.s  IL_09b7

      IL_09aa:  ldloc.s    V_25
      IL_09ac:  ldloc.s    V_24
      IL_09ae:  stloc.s    V_27
      IL_09b0:  stloc.s    V_26
      IL_09b2:  br         IL_012f

      IL_09b7:  ldarg.2
      IL_09b8:  isinst     [mscorlib]System.Array
      IL_09bd:  stloc.s    V_28
      IL_09bf:  ldloc.s    V_28
      IL_09c1:  brfalse    IL_0156

      IL_09c6:  ldloc.s    V_28
      IL_09c8:  stloc.s    V_29
      IL_09ca:  br         IL_014a

      IL_09cf:  ldarg.2
      IL_09d0:  isinst     [mscorlib]System.Array
      IL_09d5:  stloc.s    V_28
      IL_09d7:  ldloc.s    V_28
      IL_09d9:  brfalse    IL_0156

      IL_09de:  ldloc.s    V_28
      IL_09e0:  stloc.s    V_29
      IL_09e2:  br         IL_014a

      IL_09e7:  ldarg.s    arr1
      IL_09e9:  isinst     float64[]
      IL_09ee:  stloc.s    V_24
      IL_09f0:  ldloc.s    V_24
      IL_09f2:  brfalse.s  IL_0a25

      IL_09f4:  ldarg.2
      IL_09f5:  isinst     float64[]
      IL_09fa:  stloc.s    V_25
      IL_09fc:  ldloc.s    V_25
      IL_09fe:  brfalse.s  IL_0a0d

      IL_0a00:  ldloc.s    V_25
      IL_0a02:  ldloc.s    V_24
      IL_0a04:  stloc.s    V_27
      IL_0a06:  stloc.s    V_26
      IL_0a08:  br         IL_012f

      IL_0a0d:  ldarg.2
      IL_0a0e:  isinst     [mscorlib]System.Array
      IL_0a13:  stloc.s    V_28
      IL_0a15:  ldloc.s    V_28
      IL_0a17:  brfalse    IL_0156

      IL_0a1c:  ldloc.s    V_28
      IL_0a1e:  stloc.s    V_29
      IL_0a20:  br         IL_014a

      IL_0a25:  ldarg.2
      IL_0a26:  isinst     [mscorlib]System.Array
      IL_0a2b:  stloc.s    V_28
      IL_0a2d:  ldloc.s    V_28
      IL_0a2f:  brfalse    IL_0156

      IL_0a34:  ldloc.s    V_28
      IL_0a36:  stloc.s    V_29
      IL_0a38:  br         IL_014a

      IL_0a3d:  ldarg.s    arr1
      IL_0a3f:  isinst     char[]
      IL_0a44:  stloc.s    V_16
      IL_0a46:  ldloc.s    V_16
      IL_0a48:  brfalse    IL_0b38

      IL_0a4d:  ldarg.2
      IL_0a4e:  isinst     char[]
      IL_0a53:  stloc.s    V_17
      IL_0a55:  ldloc.s    V_17
      IL_0a57:  brfalse.s  IL_0a66

      IL_0a59:  ldloc.s    V_17
      IL_0a5b:  ldloc.s    V_16
      IL_0a5d:  stloc.s    V_19
      IL_0a5f:  stloc.s    V_18
      IL_0a61:  br         IL_00d5

      IL_0a66:  ldarg.s    arr1
      IL_0a68:  isinst     float32[]
      IL_0a6d:  stloc.s    V_20
      IL_0a6f:  ldloc.s    V_20
      IL_0a71:  brfalse.s  IL_0ae2

      IL_0a73:  ldarg.2
      IL_0a74:  isinst     float32[]
      IL_0a79:  stloc.s    V_21
      IL_0a7b:  ldloc.s    V_21
      IL_0a7d:  brfalse.s  IL_0a8c

      IL_0a7f:  ldloc.s    V_21
      IL_0a81:  ldloc.s    V_20
      IL_0a83:  stloc.s    V_23
      IL_0a85:  stloc.s    V_22
      IL_0a87:  br         IL_0103

      IL_0a8c:  ldarg.s    arr1
      IL_0a8e:  isinst     float64[]
      IL_0a93:  stloc.s    V_24
      IL_0a95:  ldloc.s    V_24
      IL_0a97:  brfalse.s  IL_0aca

      IL_0a99:  ldarg.2
      IL_0a9a:  isinst     float64[]
      IL_0a9f:  stloc.s    V_25
      IL_0aa1:  ldloc.s    V_25
      IL_0aa3:  brfalse.s  IL_0ab2

      IL_0aa5:  ldloc.s    V_25
      IL_0aa7:  ldloc.s    V_24
      IL_0aa9:  stloc.s    V_27
      IL_0aab:  stloc.s    V_26
      IL_0aad:  br         IL_012f

      IL_0ab2:  ldarg.2
      IL_0ab3:  isinst     [mscorlib]System.Array
      IL_0ab8:  stloc.s    V_28
      IL_0aba:  ldloc.s    V_28
      IL_0abc:  brfalse    IL_0156

      IL_0ac1:  ldloc.s    V_28
      IL_0ac3:  stloc.s    V_29
      IL_0ac5:  br         IL_014a

      IL_0aca:  ldarg.2
      IL_0acb:  isinst     [mscorlib]System.Array
      IL_0ad0:  stloc.s    V_28
      IL_0ad2:  ldloc.s    V_28
      IL_0ad4:  brfalse    IL_0156

      IL_0ad9:  ldloc.s    V_28
      IL_0adb:  stloc.s    V_29
      IL_0add:  br         IL_014a

      IL_0ae2:  ldarg.s    arr1
      IL_0ae4:  isinst     float64[]
      IL_0ae9:  stloc.s    V_24
      IL_0aeb:  ldloc.s    V_24
      IL_0aed:  brfalse.s  IL_0b20

      IL_0aef:  ldarg.2
      IL_0af0:  isinst     float64[]
      IL_0af5:  stloc.s    V_25
      IL_0af7:  ldloc.s    V_25
      IL_0af9:  brfalse.s  IL_0b08

      IL_0afb:  ldloc.s    V_25
      IL_0afd:  ldloc.s    V_24
      IL_0aff:  stloc.s    V_27
      IL_0b01:  stloc.s    V_26
      IL_0b03:  br         IL_012f

      IL_0b08:  ldarg.2
      IL_0b09:  isinst     [mscorlib]System.Array
      IL_0b0e:  stloc.s    V_28
      IL_0b10:  ldloc.s    V_28
      IL_0b12:  brfalse    IL_0156

      IL_0b17:  ldloc.s    V_28
      IL_0b19:  stloc.s    V_29
      IL_0b1b:  br         IL_014a

      IL_0b20:  ldarg.2
      IL_0b21:  isinst     [mscorlib]System.Array
      IL_0b26:  stloc.s    V_28
      IL_0b28:  ldloc.s    V_28
      IL_0b2a:  brfalse    IL_0156

      IL_0b2f:  ldloc.s    V_28
      IL_0b31:  stloc.s    V_29
      IL_0b33:  br         IL_014a

      IL_0b38:  ldarg.s    arr1
      IL_0b3a:  isinst     float32[]
      IL_0b3f:  stloc.s    V_20
      IL_0b41:  ldloc.s    V_20
      IL_0b43:  brfalse.s  IL_0bb4

      IL_0b45:  ldarg.2
      IL_0b46:  isinst     float32[]
      IL_0b4b:  stloc.s    V_21
      IL_0b4d:  ldloc.s    V_21
      IL_0b4f:  brfalse.s  IL_0b5e

      IL_0b51:  ldloc.s    V_21
      IL_0b53:  ldloc.s    V_20
      IL_0b55:  stloc.s    V_23
      IL_0b57:  stloc.s    V_22
      IL_0b59:  br         IL_0103

      IL_0b5e:  ldarg.s    arr1
      IL_0b60:  isinst     float64[]
      IL_0b65:  stloc.s    V_24
      IL_0b67:  ldloc.s    V_24
      IL_0b69:  brfalse.s  IL_0b9c

      IL_0b6b:  ldarg.2
      IL_0b6c:  isinst     float64[]
      IL_0b71:  stloc.s    V_25
      IL_0b73:  ldloc.s    V_25
      IL_0b75:  brfalse.s  IL_0b84

      IL_0b77:  ldloc.s    V_25
      IL_0b79:  ldloc.s    V_24
      IL_0b7b:  stloc.s    V_27
      IL_0b7d:  stloc.s    V_26
      IL_0b7f:  br         IL_012f

      IL_0b84:  ldarg.2
      IL_0b85:  isinst     [mscorlib]System.Array
      IL_0b8a:  stloc.s    V_28
      IL_0b8c:  ldloc.s    V_28
      IL_0b8e:  brfalse    IL_0156

      IL_0b93:  ldloc.s    V_28
      IL_0b95:  stloc.s    V_29
      IL_0b97:  br         IL_014a

      IL_0b9c:  ldarg.2
      IL_0b9d:  isinst     [mscorlib]System.Array
      IL_0ba2:  stloc.s    V_28
      IL_0ba4:  ldloc.s    V_28
      IL_0ba6:  brfalse    IL_0156

      IL_0bab:  ldloc.s    V_28
      IL_0bad:  stloc.s    V_29
      IL_0baf:  br         IL_014a

      IL_0bb4:  ldarg.s    arr1
      IL_0bb6:  isinst     float64[]
      IL_0bbb:  stloc.s    V_24
      IL_0bbd:  ldloc.s    V_24
      IL_0bbf:  brfalse.s  IL_0bf2

      IL_0bc1:  ldarg.2
      IL_0bc2:  isinst     float64[]
      IL_0bc7:  stloc.s    V_25
      IL_0bc9:  ldloc.s    V_25
      IL_0bcb:  brfalse.s  IL_0bda

      IL_0bcd:  ldloc.s    V_25
      IL_0bcf:  ldloc.s    V_24
      IL_0bd1:  stloc.s    V_27
      IL_0bd3:  stloc.s    V_26
      IL_0bd5:  br         IL_012f

      IL_0bda:  ldarg.2
      IL_0bdb:  isinst     [mscorlib]System.Array
      IL_0be0:  stloc.s    V_28
      IL_0be2:  ldloc.s    V_28
      IL_0be4:  brfalse    IL_0156

      IL_0be9:  ldloc.s    V_28
      IL_0beb:  stloc.s    V_29
      IL_0bed:  br         IL_014a

      IL_0bf2:  ldarg.2
      IL_0bf3:  isinst     [mscorlib]System.Array
      IL_0bf8:  stloc.s    V_28
      IL_0bfa:  ldloc.s    V_28
      IL_0bfc:  brfalse    IL_0156

      IL_0c01:  ldloc.s    V_28
      IL_0c03:  stloc.s    V_29
      IL_0c05:  br         IL_014a

      IL_0c0a:  ldarg.s    arr1
      IL_0c0c:  isinst     int64[]
      IL_0c11:  stloc.s    V_12
      IL_0c13:  ldloc.s    V_12
      IL_0c15:  brfalse    IL_0e00

      IL_0c1a:  ldarg.2
      IL_0c1b:  isinst     int64[]
      IL_0c20:  stloc.s    V_13
      IL_0c22:  ldloc.s    V_13
      IL_0c24:  brfalse.s  IL_0c33

      IL_0c26:  ldloc.s    V_13
      IL_0c28:  ldloc.s    V_12
      IL_0c2a:  stloc.s    V_15
      IL_0c2c:  stloc.s    V_14
      IL_0c2e:  br         IL_00a7

      IL_0c33:  ldarg.s    arr1
      IL_0c35:  isinst     char[]
      IL_0c3a:  stloc.s    V_16
      IL_0c3c:  ldloc.s    V_16
      IL_0c3e:  brfalse    IL_0d2e

      IL_0c43:  ldarg.2
      IL_0c44:  isinst     char[]
      IL_0c49:  stloc.s    V_17
      IL_0c4b:  ldloc.s    V_17
      IL_0c4d:  brfalse.s  IL_0c5c

      IL_0c4f:  ldloc.s    V_17
      IL_0c51:  ldloc.s    V_16
      IL_0c53:  stloc.s    V_19
      IL_0c55:  stloc.s    V_18
      IL_0c57:  br         IL_00d5

      IL_0c5c:  ldarg.s    arr1
      IL_0c5e:  isinst     float32[]
      IL_0c63:  stloc.s    V_20
      IL_0c65:  ldloc.s    V_20
      IL_0c67:  brfalse.s  IL_0cd8

      IL_0c69:  ldarg.2
      IL_0c6a:  isinst     float32[]
      IL_0c6f:  stloc.s    V_21
      IL_0c71:  ldloc.s    V_21
      IL_0c73:  brfalse.s  IL_0c82

      IL_0c75:  ldloc.s    V_21
      IL_0c77:  ldloc.s    V_20
      IL_0c79:  stloc.s    V_23
      IL_0c7b:  stloc.s    V_22
      IL_0c7d:  br         IL_0103

      IL_0c82:  ldarg.s    arr1
      IL_0c84:  isinst     float64[]
      IL_0c89:  stloc.s    V_24
      IL_0c8b:  ldloc.s    V_24
      IL_0c8d:  brfalse.s  IL_0cc0

      IL_0c8f:  ldarg.2
      IL_0c90:  isinst     float64[]
      IL_0c95:  stloc.s    V_25
      IL_0c97:  ldloc.s    V_25
      IL_0c99:  brfalse.s  IL_0ca8

      IL_0c9b:  ldloc.s    V_25
      IL_0c9d:  ldloc.s    V_24
      IL_0c9f:  stloc.s    V_27
      IL_0ca1:  stloc.s    V_26
      IL_0ca3:  br         IL_012f

      IL_0ca8:  ldarg.2
      IL_0ca9:  isinst     [mscorlib]System.Array
      IL_0cae:  stloc.s    V_28
      IL_0cb0:  ldloc.s    V_28
      IL_0cb2:  brfalse    IL_0156

      IL_0cb7:  ldloc.s    V_28
      IL_0cb9:  stloc.s    V_29
      IL_0cbb:  br         IL_014a

      IL_0cc0:  ldarg.2
      IL_0cc1:  isinst     [mscorlib]System.Array
      IL_0cc6:  stloc.s    V_28
      IL_0cc8:  ldloc.s    V_28
      IL_0cca:  brfalse    IL_0156

      IL_0ccf:  ldloc.s    V_28
      IL_0cd1:  stloc.s    V_29
      IL_0cd3:  br         IL_014a

      IL_0cd8:  ldarg.s    arr1
      IL_0cda:  isinst     float64[]
      IL_0cdf:  stloc.s    V_24
      IL_0ce1:  ldloc.s    V_24
      IL_0ce3:  brfalse.s  IL_0d16

      IL_0ce5:  ldarg.2
      IL_0ce6:  isinst     float64[]
      IL_0ceb:  stloc.s    V_25
      IL_0ced:  ldloc.s    V_25
      IL_0cef:  brfalse.s  IL_0cfe

      IL_0cf1:  ldloc.s    V_25
      IL_0cf3:  ldloc.s    V_24
      IL_0cf5:  stloc.s    V_27
      IL_0cf7:  stloc.s    V_26
      IL_0cf9:  br         IL_012f

      IL_0cfe:  ldarg.2
      IL_0cff:  isinst     [mscorlib]System.Array
      IL_0d04:  stloc.s    V_28
      IL_0d06:  ldloc.s    V_28
      IL_0d08:  brfalse    IL_0156

      IL_0d0d:  ldloc.s    V_28
      IL_0d0f:  stloc.s    V_29
      IL_0d11:  br         IL_014a

      IL_0d16:  ldarg.2
      IL_0d17:  isinst     [mscorlib]System.Array
      IL_0d1c:  stloc.s    V_28
      IL_0d1e:  ldloc.s    V_28
      IL_0d20:  brfalse    IL_0156

      IL_0d25:  ldloc.s    V_28
      IL_0d27:  stloc.s    V_29
      IL_0d29:  br         IL_014a

      IL_0d2e:  ldarg.s    arr1
      IL_0d30:  isinst     float32[]
      IL_0d35:  stloc.s    V_20
      IL_0d37:  ldloc.s    V_20
      IL_0d39:  brfalse.s  IL_0daa

      IL_0d3b:  ldarg.2
      IL_0d3c:  isinst     float32[]
      IL_0d41:  stloc.s    V_21
      IL_0d43:  ldloc.s    V_21
      IL_0d45:  brfalse.s  IL_0d54

      IL_0d47:  ldloc.s    V_21
      IL_0d49:  ldloc.s    V_20
      IL_0d4b:  stloc.s    V_23
      IL_0d4d:  stloc.s    V_22
      IL_0d4f:  br         IL_0103

      IL_0d54:  ldarg.s    arr1
      IL_0d56:  isinst     float64[]
      IL_0d5b:  stloc.s    V_24
      IL_0d5d:  ldloc.s    V_24
      IL_0d5f:  brfalse.s  IL_0d92

      IL_0d61:  ldarg.2
      IL_0d62:  isinst     float64[]
      IL_0d67:  stloc.s    V_25
      IL_0d69:  ldloc.s    V_25
      IL_0d6b:  brfalse.s  IL_0d7a

      IL_0d6d:  ldloc.s    V_25
      IL_0d6f:  ldloc.s    V_24
      IL_0d71:  stloc.s    V_27
      IL_0d73:  stloc.s    V_26
      IL_0d75:  br         IL_012f

      IL_0d7a:  ldarg.2
      IL_0d7b:  isinst     [mscorlib]System.Array
      IL_0d80:  stloc.s    V_28
      IL_0d82:  ldloc.s    V_28
      IL_0d84:  brfalse    IL_0156

      IL_0d89:  ldloc.s    V_28
      IL_0d8b:  stloc.s    V_29
      IL_0d8d:  br         IL_014a

      IL_0d92:  ldarg.2
      IL_0d93:  isinst     [mscorlib]System.Array
      IL_0d98:  stloc.s    V_28
      IL_0d9a:  ldloc.s    V_28
      IL_0d9c:  brfalse    IL_0156

      IL_0da1:  ldloc.s    V_28
      IL_0da3:  stloc.s    V_29
      IL_0da5:  br         IL_014a

      IL_0daa:  ldarg.s    arr1
      IL_0dac:  isinst     float64[]
      IL_0db1:  stloc.s    V_24
      IL_0db3:  ldloc.s    V_24
      IL_0db5:  brfalse.s  IL_0de8

      IL_0db7:  ldarg.2
      IL_0db8:  isinst     float64[]
      IL_0dbd:  stloc.s    V_25
      IL_0dbf:  ldloc.s    V_25
      IL_0dc1:  brfalse.s  IL_0dd0

      IL_0dc3:  ldloc.s    V_25
      IL_0dc5:  ldloc.s    V_24
      IL_0dc7:  stloc.s    V_27
      IL_0dc9:  stloc.s    V_26
      IL_0dcb:  br         IL_012f

      IL_0dd0:  ldarg.2
      IL_0dd1:  isinst     [mscorlib]System.Array
      IL_0dd6:  stloc.s    V_28
      IL_0dd8:  ldloc.s    V_28
      IL_0dda:  brfalse    IL_0156

      IL_0ddf:  ldloc.s    V_28
      IL_0de1:  stloc.s    V_29
      IL_0de3:  br         IL_014a

      IL_0de8:  ldarg.2
      IL_0de9:  isinst     [mscorlib]System.Array
      IL_0dee:  stloc.s    V_28
      IL_0df0:  ldloc.s    V_28
      IL_0df2:  brfalse    IL_0156

      IL_0df7:  ldloc.s    V_28
      IL_0df9:  stloc.s    V_29
      IL_0dfb:  br         IL_014a

      IL_0e00:  ldarg.s    arr1
      IL_0e02:  isinst     char[]
      IL_0e07:  stloc.s    V_16
      IL_0e09:  ldloc.s    V_16
      IL_0e0b:  brfalse    IL_0efb

      IL_0e10:  ldarg.2
      IL_0e11:  isinst     char[]
      IL_0e16:  stloc.s    V_17
      IL_0e18:  ldloc.s    V_17
      IL_0e1a:  brfalse.s  IL_0e29

      IL_0e1c:  ldloc.s    V_17
      IL_0e1e:  ldloc.s    V_16
      IL_0e20:  stloc.s    V_19
      IL_0e22:  stloc.s    V_18
      IL_0e24:  br         IL_00d5

      IL_0e29:  ldarg.s    arr1
      IL_0e2b:  isinst     float32[]
      IL_0e30:  stloc.s    V_20
      IL_0e32:  ldloc.s    V_20
      IL_0e34:  brfalse.s  IL_0ea5

      IL_0e36:  ldarg.2
      IL_0e37:  isinst     float32[]
      IL_0e3c:  stloc.s    V_21
      IL_0e3e:  ldloc.s    V_21
      IL_0e40:  brfalse.s  IL_0e4f

      IL_0e42:  ldloc.s    V_21
      IL_0e44:  ldloc.s    V_20
      IL_0e46:  stloc.s    V_23
      IL_0e48:  stloc.s    V_22
      IL_0e4a:  br         IL_0103

      IL_0e4f:  ldarg.s    arr1
      IL_0e51:  isinst     float64[]
      IL_0e56:  stloc.s    V_24
      IL_0e58:  ldloc.s    V_24
      IL_0e5a:  brfalse.s  IL_0e8d

      IL_0e5c:  ldarg.2
      IL_0e5d:  isinst     float64[]
      IL_0e62:  stloc.s    V_25
      IL_0e64:  ldloc.s    V_25
      IL_0e66:  brfalse.s  IL_0e75

      IL_0e68:  ldloc.s    V_25
      IL_0e6a:  ldloc.s    V_24
      IL_0e6c:  stloc.s    V_27
      IL_0e6e:  stloc.s    V_26
      IL_0e70:  br         IL_012f

      IL_0e75:  ldarg.2
      IL_0e76:  isinst     [mscorlib]System.Array
      IL_0e7b:  stloc.s    V_28
      IL_0e7d:  ldloc.s    V_28
      IL_0e7f:  brfalse    IL_0156

      IL_0e84:  ldloc.s    V_28
      IL_0e86:  stloc.s    V_29
      IL_0e88:  br         IL_014a

      IL_0e8d:  ldarg.2
      IL_0e8e:  isinst     [mscorlib]System.Array
      IL_0e93:  stloc.s    V_28
      IL_0e95:  ldloc.s    V_28
      IL_0e97:  brfalse    IL_0156

      IL_0e9c:  ldloc.s    V_28
      IL_0e9e:  stloc.s    V_29
      IL_0ea0:  br         IL_014a

      IL_0ea5:  ldarg.s    arr1
      IL_0ea7:  isinst     float64[]
      IL_0eac:  stloc.s    V_24
      IL_0eae:  ldloc.s    V_24
      IL_0eb0:  brfalse.s  IL_0ee3

      IL_0eb2:  ldarg.2
      IL_0eb3:  isinst     float64[]
      IL_0eb8:  stloc.s    V_25
      IL_0eba:  ldloc.s    V_25
      IL_0ebc:  brfalse.s  IL_0ecb

      IL_0ebe:  ldloc.s    V_25
      IL_0ec0:  ldloc.s    V_24
      IL_0ec2:  stloc.s    V_27
      IL_0ec4:  stloc.s    V_26
      IL_0ec6:  br         IL_012f

      IL_0ecb:  ldarg.2
      IL_0ecc:  isinst     [mscorlib]System.Array
      IL_0ed1:  stloc.s    V_28
      IL_0ed3:  ldloc.s    V_28
      IL_0ed5:  brfalse    IL_0156

      IL_0eda:  ldloc.s    V_28
      IL_0edc:  stloc.s    V_29
      IL_0ede:  br         IL_014a

      IL_0ee3:  ldarg.2
      IL_0ee4:  isinst     [mscorlib]System.Array
      IL_0ee9:  stloc.s    V_28
      IL_0eeb:  ldloc.s    V_28
      IL_0eed:  brfalse    IL_0156

      IL_0ef2:  ldloc.s    V_28
      IL_0ef4:  stloc.s    V_29
      IL_0ef6:  br         IL_014a

      IL_0efb:  ldarg.s    arr1
      IL_0efd:  isinst     float32[]
      IL_0f02:  stloc.s    V_20
      IL_0f04:  ldloc.s    V_20
      IL_0f06:  brfalse.s  IL_0f77

      IL_0f08:  ldarg.2
      IL_0f09:  isinst     float32[]
      IL_0f0e:  stloc.s    V_21
      IL_0f10:  ldloc.s    V_21
      IL_0f12:  brfalse.s  IL_0f21

      IL_0f14:  ldloc.s    V_21
      IL_0f16:  ldloc.s    V_20
      IL_0f18:  stloc.s    V_23
      IL_0f1a:  stloc.s    V_22
      IL_0f1c:  br         IL_0103

      IL_0f21:  ldarg.s    arr1
      IL_0f23:  isinst     float64[]
      IL_0f28:  stloc.s    V_24
      IL_0f2a:  ldloc.s    V_24
      IL_0f2c:  brfalse.s  IL_0f5f

      IL_0f2e:  ldarg.2
      IL_0f2f:  isinst     float64[]
      IL_0f34:  stloc.s    V_25
      IL_0f36:  ldloc.s    V_25
      IL_0f38:  brfalse.s  IL_0f47

      IL_0f3a:  ldloc.s    V_25
      IL_0f3c:  ldloc.s    V_24
      IL_0f3e:  stloc.s    V_27
      IL_0f40:  stloc.s    V_26
      IL_0f42:  br         IL_012f

      IL_0f47:  ldarg.2
      IL_0f48:  isinst     [mscorlib]System.Array
      IL_0f4d:  stloc.s    V_28
      IL_0f4f:  ldloc.s    V_28
      IL_0f51:  brfalse    IL_0156

      IL_0f56:  ldloc.s    V_28
      IL_0f58:  stloc.s    V_29
      IL_0f5a:  br         IL_014a

      IL_0f5f:  ldarg.2
      IL_0f60:  isinst     [mscorlib]System.Array
      IL_0f65:  stloc.s    V_28
      IL_0f67:  ldloc.s    V_28
      IL_0f69:  brfalse    IL_0156

      IL_0f6e:  ldloc.s    V_28
      IL_0f70:  stloc.s    V_29
      IL_0f72:  br         IL_014a

      IL_0f77:  ldarg.s    arr1
      IL_0f79:  isinst     float64[]
      IL_0f7e:  stloc.s    V_24
      IL_0f80:  ldloc.s    V_24
      IL_0f82:  brfalse.s  IL_0fb5

      IL_0f84:  ldarg.2
      IL_0f85:  isinst     float64[]
      IL_0f8a:  stloc.s    V_25
      IL_0f8c:  ldloc.s    V_25
      IL_0f8e:  brfalse.s  IL_0f9d

      IL_0f90:  ldloc.s    V_25
      IL_0f92:  ldloc.s    V_24
      IL_0f94:  stloc.s    V_27
      IL_0f96:  stloc.s    V_26
      IL_0f98:  br         IL_012f

      IL_0f9d:  ldarg.2
      IL_0f9e:  isinst     [mscorlib]System.Array
      IL_0fa3:  stloc.s    V_28
      IL_0fa5:  ldloc.s    V_28
      IL_0fa7:  brfalse    IL_0156

      IL_0fac:  ldloc.s    V_28
      IL_0fae:  stloc.s    V_29
      IL_0fb0:  br         IL_014a

      IL_0fb5:  ldarg.2
      IL_0fb6:  isinst     [mscorlib]System.Array
      IL_0fbb:  stloc.s    V_28
      IL_0fbd:  ldloc.s    V_28
      IL_0fbf:  brfalse    IL_0156

      IL_0fc4:  ldloc.s    V_28
      IL_0fc6:  stloc.s    V_29
      IL_0fc8:  br         IL_014a

      IL_0fcd:  ldarg.s    arr1
      IL_0fcf:  isinst     uint8[]
      IL_0fd4:  stloc.s    V_4
      IL_0fd6:  ldloc.s    V_4
      IL_0fd8:  brfalse    IL_17a5

      IL_0fdd:  ldarg.2
      IL_0fde:  isinst     uint8[]
      IL_0fe3:  stloc.s    V_5
      IL_0fe5:  ldloc.s    V_5
      IL_0fe7:  brfalse.s  IL_0ff6

      IL_0fe9:  ldloc.s    V_5
      IL_0feb:  ldloc.s    V_4
      IL_0fed:  stloc.s    V_7
      IL_0fef:  stloc.s    V_6
      IL_0ff1:  br         IL_004b

      IL_0ff6:  ldarg.s    arr1
      IL_0ff8:  isinst     int32[]
      IL_0ffd:  stloc.s    V_8
      IL_0fff:  ldloc.s    V_8
      IL_1001:  brfalse    IL_13e2

      IL_1006:  ldarg.2
      IL_1007:  isinst     int32[]
      IL_100c:  stloc.s    V_9
      IL_100e:  ldloc.s    V_9
      IL_1010:  brfalse.s  IL_101f

      IL_1012:  ldloc.s    V_9
      IL_1014:  ldloc.s    V_8
      IL_1016:  stloc.s    V_11
      IL_1018:  stloc.s    V_10
      IL_101a:  br         IL_0079

      IL_101f:  ldarg.s    arr1
      IL_1021:  isinst     int64[]
      IL_1026:  stloc.s    V_12
      IL_1028:  ldloc.s    V_12
      IL_102a:  brfalse    IL_1215

      IL_102f:  ldarg.2
      IL_1030:  isinst     int64[]
      IL_1035:  stloc.s    V_13
      IL_1037:  ldloc.s    V_13
      IL_1039:  brfalse.s  IL_1048

      IL_103b:  ldloc.s    V_13
      IL_103d:  ldloc.s    V_12
      IL_103f:  stloc.s    V_15
      IL_1041:  stloc.s    V_14
      IL_1043:  br         IL_00a7

      IL_1048:  ldarg.s    arr1
      IL_104a:  isinst     char[]
      IL_104f:  stloc.s    V_16
      IL_1051:  ldloc.s    V_16
      IL_1053:  brfalse    IL_1143

      IL_1058:  ldarg.2
      IL_1059:  isinst     char[]
      IL_105e:  stloc.s    V_17
      IL_1060:  ldloc.s    V_17
      IL_1062:  brfalse.s  IL_1071

      IL_1064:  ldloc.s    V_17
      IL_1066:  ldloc.s    V_16
      IL_1068:  stloc.s    V_19
      IL_106a:  stloc.s    V_18
      IL_106c:  br         IL_00d5

      IL_1071:  ldarg.s    arr1
      IL_1073:  isinst     float32[]
      IL_1078:  stloc.s    V_20
      IL_107a:  ldloc.s    V_20
      IL_107c:  brfalse.s  IL_10ed

      IL_107e:  ldarg.2
      IL_107f:  isinst     float32[]
      IL_1084:  stloc.s    V_21
      IL_1086:  ldloc.s    V_21
      IL_1088:  brfalse.s  IL_1097

      IL_108a:  ldloc.s    V_21
      IL_108c:  ldloc.s    V_20
      IL_108e:  stloc.s    V_23
      IL_1090:  stloc.s    V_22
      IL_1092:  br         IL_0103

      IL_1097:  ldarg.s    arr1
      IL_1099:  isinst     float64[]
      IL_109e:  stloc.s    V_24
      IL_10a0:  ldloc.s    V_24
      IL_10a2:  brfalse.s  IL_10d5

      IL_10a4:  ldarg.2
      IL_10a5:  isinst     float64[]
      IL_10aa:  stloc.s    V_25
      IL_10ac:  ldloc.s    V_25
      IL_10ae:  brfalse.s  IL_10bd

      IL_10b0:  ldloc.s    V_25
      IL_10b2:  ldloc.s    V_24
      IL_10b4:  stloc.s    V_27
      IL_10b6:  stloc.s    V_26
      IL_10b8:  br         IL_012f

      IL_10bd:  ldarg.2
      IL_10be:  isinst     [mscorlib]System.Array
      IL_10c3:  stloc.s    V_28
      IL_10c5:  ldloc.s    V_28
      IL_10c7:  brfalse    IL_0156

      IL_10cc:  ldloc.s    V_28
      IL_10ce:  stloc.s    V_29
      IL_10d0:  br         IL_014a

      IL_10d5:  ldarg.2
      IL_10d6:  isinst     [mscorlib]System.Array
      IL_10db:  stloc.s    V_28
      IL_10dd:  ldloc.s    V_28
      IL_10df:  brfalse    IL_0156

      IL_10e4:  ldloc.s    V_28
      IL_10e6:  stloc.s    V_29
      IL_10e8:  br         IL_014a

      IL_10ed:  ldarg.s    arr1
      IL_10ef:  isinst     float64[]
      IL_10f4:  stloc.s    V_24
      IL_10f6:  ldloc.s    V_24
      IL_10f8:  brfalse.s  IL_112b

      IL_10fa:  ldarg.2
      IL_10fb:  isinst     float64[]
      IL_1100:  stloc.s    V_25
      IL_1102:  ldloc.s    V_25
      IL_1104:  brfalse.s  IL_1113

      IL_1106:  ldloc.s    V_25
      IL_1108:  ldloc.s    V_24
      IL_110a:  stloc.s    V_27
      IL_110c:  stloc.s    V_26
      IL_110e:  br         IL_012f

      IL_1113:  ldarg.2
      IL_1114:  isinst     [mscorlib]System.Array
      IL_1119:  stloc.s    V_28
      IL_111b:  ldloc.s    V_28
      IL_111d:  brfalse    IL_0156

      IL_1122:  ldloc.s    V_28
      IL_1124:  stloc.s    V_29
      IL_1126:  br         IL_014a

      IL_112b:  ldarg.2
      IL_112c:  isinst     [mscorlib]System.Array
      IL_1131:  stloc.s    V_28
      IL_1133:  ldloc.s    V_28
      IL_1135:  brfalse    IL_0156

      IL_113a:  ldloc.s    V_28
      IL_113c:  stloc.s    V_29
      IL_113e:  br         IL_014a

      IL_1143:  ldarg.s    arr1
      IL_1145:  isinst     float32[]
      IL_114a:  stloc.s    V_20
      IL_114c:  ldloc.s    V_20
      IL_114e:  brfalse.s  IL_11bf

      IL_1150:  ldarg.2
      IL_1151:  isinst     float32[]
      IL_1156:  stloc.s    V_21
      IL_1158:  ldloc.s    V_21
      IL_115a:  brfalse.s  IL_1169

      IL_115c:  ldloc.s    V_21
      IL_115e:  ldloc.s    V_20
      IL_1160:  stloc.s    V_23
      IL_1162:  stloc.s    V_22
      IL_1164:  br         IL_0103

      IL_1169:  ldarg.s    arr1
      IL_116b:  isinst     float64[]
      IL_1170:  stloc.s    V_24
      IL_1172:  ldloc.s    V_24
      IL_1174:  brfalse.s  IL_11a7

      IL_1176:  ldarg.2
      IL_1177:  isinst     float64[]
      IL_117c:  stloc.s    V_25
      IL_117e:  ldloc.s    V_25
      IL_1180:  brfalse.s  IL_118f

      IL_1182:  ldloc.s    V_25
      IL_1184:  ldloc.s    V_24
      IL_1186:  stloc.s    V_27
      IL_1188:  stloc.s    V_26
      IL_118a:  br         IL_012f

      IL_118f:  ldarg.2
      IL_1190:  isinst     [mscorlib]System.Array
      IL_1195:  stloc.s    V_28
      IL_1197:  ldloc.s    V_28
      IL_1199:  brfalse    IL_0156

      IL_119e:  ldloc.s    V_28
      IL_11a0:  stloc.s    V_29
      IL_11a2:  br         IL_014a

      IL_11a7:  ldarg.2
      IL_11a8:  isinst     [mscorlib]System.Array
      IL_11ad:  stloc.s    V_28
      IL_11af:  ldloc.s    V_28
      IL_11b1:  brfalse    IL_0156

      IL_11b6:  ldloc.s    V_28
      IL_11b8:  stloc.s    V_29
      IL_11ba:  br         IL_014a

      IL_11bf:  ldarg.s    arr1
      IL_11c1:  isinst     float64[]
      IL_11c6:  stloc.s    V_24
      IL_11c8:  ldloc.s    V_24
      IL_11ca:  brfalse.s  IL_11fd

      IL_11cc:  ldarg.2
      IL_11cd:  isinst     float64[]
      IL_11d2:  stloc.s    V_25
      IL_11d4:  ldloc.s    V_25
      IL_11d6:  brfalse.s  IL_11e5

      IL_11d8:  ldloc.s    V_25
      IL_11da:  ldloc.s    V_24
      IL_11dc:  stloc.s    V_27
      IL_11de:  stloc.s    V_26
      IL_11e0:  br         IL_012f

      IL_11e5:  ldarg.2
      IL_11e6:  isinst     [mscorlib]System.Array
      IL_11eb:  stloc.s    V_28
      IL_11ed:  ldloc.s    V_28
      IL_11ef:  brfalse    IL_0156

      IL_11f4:  ldloc.s    V_28
      IL_11f6:  stloc.s    V_29
      IL_11f8:  br         IL_014a

      IL_11fd:  ldarg.2
      IL_11fe:  isinst     [mscorlib]System.Array
      IL_1203:  stloc.s    V_28
      IL_1205:  ldloc.s    V_28
      IL_1207:  brfalse    IL_0156

      IL_120c:  ldloc.s    V_28
      IL_120e:  stloc.s    V_29
      IL_1210:  br         IL_014a

      IL_1215:  ldarg.s    arr1
      IL_1217:  isinst     char[]
      IL_121c:  stloc.s    V_16
      IL_121e:  ldloc.s    V_16
      IL_1220:  brfalse    IL_1310

      IL_1225:  ldarg.2
      IL_1226:  isinst     char[]
      IL_122b:  stloc.s    V_17
      IL_122d:  ldloc.s    V_17
      IL_122f:  brfalse.s  IL_123e

      IL_1231:  ldloc.s    V_17
      IL_1233:  ldloc.s    V_16
      IL_1235:  stloc.s    V_19
      IL_1237:  stloc.s    V_18
      IL_1239:  br         IL_00d5

      IL_123e:  ldarg.s    arr1
      IL_1240:  isinst     float32[]
      IL_1245:  stloc.s    V_20
      IL_1247:  ldloc.s    V_20
      IL_1249:  brfalse.s  IL_12ba

      IL_124b:  ldarg.2
      IL_124c:  isinst     float32[]
      IL_1251:  stloc.s    V_21
      IL_1253:  ldloc.s    V_21
      IL_1255:  brfalse.s  IL_1264

      IL_1257:  ldloc.s    V_21
      IL_1259:  ldloc.s    V_20
      IL_125b:  stloc.s    V_23
      IL_125d:  stloc.s    V_22
      IL_125f:  br         IL_0103

      IL_1264:  ldarg.s    arr1
      IL_1266:  isinst     float64[]
      IL_126b:  stloc.s    V_24
      IL_126d:  ldloc.s    V_24
      IL_126f:  brfalse.s  IL_12a2

      IL_1271:  ldarg.2
      IL_1272:  isinst     float64[]
      IL_1277:  stloc.s    V_25
      IL_1279:  ldloc.s    V_25
      IL_127b:  brfalse.s  IL_128a

      IL_127d:  ldloc.s    V_25
      IL_127f:  ldloc.s    V_24
      IL_1281:  stloc.s    V_27
      IL_1283:  stloc.s    V_26
      IL_1285:  br         IL_012f

      IL_128a:  ldarg.2
      IL_128b:  isinst     [mscorlib]System.Array
      IL_1290:  stloc.s    V_28
      IL_1292:  ldloc.s    V_28
      IL_1294:  brfalse    IL_0156

      IL_1299:  ldloc.s    V_28
      IL_129b:  stloc.s    V_29
      IL_129d:  br         IL_014a

      IL_12a2:  ldarg.2
      IL_12a3:  isinst     [mscorlib]System.Array
      IL_12a8:  stloc.s    V_28
      IL_12aa:  ldloc.s    V_28
      IL_12ac:  brfalse    IL_0156

      IL_12b1:  ldloc.s    V_28
      IL_12b3:  stloc.s    V_29
      IL_12b5:  br         IL_014a

      IL_12ba:  ldarg.s    arr1
      IL_12bc:  isinst     float64[]
      IL_12c1:  stloc.s    V_24
      IL_12c3:  ldloc.s    V_24
      IL_12c5:  brfalse.s  IL_12f8

      IL_12c7:  ldarg.2
      IL_12c8:  isinst     float64[]
      IL_12cd:  stloc.s    V_25
      IL_12cf:  ldloc.s    V_25
      IL_12d1:  brfalse.s  IL_12e0

      IL_12d3:  ldloc.s    V_25
      IL_12d5:  ldloc.s    V_24
      IL_12d7:  stloc.s    V_27
      IL_12d9:  stloc.s    V_26
      IL_12db:  br         IL_012f

      IL_12e0:  ldarg.2
      IL_12e1:  isinst     [mscorlib]System.Array
      IL_12e6:  stloc.s    V_28
      IL_12e8:  ldloc.s    V_28
      IL_12ea:  brfalse    IL_0156

      IL_12ef:  ldloc.s    V_28
      IL_12f1:  stloc.s    V_29
      IL_12f3:  br         IL_014a

      IL_12f8:  ldarg.2
      IL_12f9:  isinst     [mscorlib]System.Array
      IL_12fe:  stloc.s    V_28
      IL_1300:  ldloc.s    V_28
      IL_1302:  brfalse    IL_0156

      IL_1307:  ldloc.s    V_28
      IL_1309:  stloc.s    V_29
      IL_130b:  br         IL_014a

      IL_1310:  ldarg.s    arr1
      IL_1312:  isinst     float32[]
      IL_1317:  stloc.s    V_20
      IL_1319:  ldloc.s    V_20
      IL_131b:  brfalse.s  IL_138c

      IL_131d:  ldarg.2
      IL_131e:  isinst     float32[]
      IL_1323:  stloc.s    V_21
      IL_1325:  ldloc.s    V_21
      IL_1327:  brfalse.s  IL_1336

      IL_1329:  ldloc.s    V_21
      IL_132b:  ldloc.s    V_20
      IL_132d:  stloc.s    V_23
      IL_132f:  stloc.s    V_22
      IL_1331:  br         IL_0103

      IL_1336:  ldarg.s    arr1
      IL_1338:  isinst     float64[]
      IL_133d:  stloc.s    V_24
      IL_133f:  ldloc.s    V_24
      IL_1341:  brfalse.s  IL_1374

      IL_1343:  ldarg.2
      IL_1344:  isinst     float64[]
      IL_1349:  stloc.s    V_25
      IL_134b:  ldloc.s    V_25
      IL_134d:  brfalse.s  IL_135c

      IL_134f:  ldloc.s    V_25
      IL_1351:  ldloc.s    V_24
      IL_1353:  stloc.s    V_27
      IL_1355:  stloc.s    V_26
      IL_1357:  br         IL_012f

      IL_135c:  ldarg.2
      IL_135d:  isinst     [mscorlib]System.Array
      IL_1362:  stloc.s    V_28
      IL_1364:  ldloc.s    V_28
      IL_1366:  brfalse    IL_0156

      IL_136b:  ldloc.s    V_28
      IL_136d:  stloc.s    V_29
      IL_136f:  br         IL_014a

      IL_1374:  ldarg.2
      IL_1375:  isinst     [mscorlib]System.Array
      IL_137a:  stloc.s    V_28
      IL_137c:  ldloc.s    V_28
      IL_137e:  brfalse    IL_0156

      IL_1383:  ldloc.s    V_28
      IL_1385:  stloc.s    V_29
      IL_1387:  br         IL_014a

      IL_138c:  ldarg.s    arr1
      IL_138e:  isinst     float64[]
      IL_1393:  stloc.s    V_24
      IL_1395:  ldloc.s    V_24
      IL_1397:  brfalse.s  IL_13ca

      IL_1399:  ldarg.2
      IL_139a:  isinst     float64[]
      IL_139f:  stloc.s    V_25
      IL_13a1:  ldloc.s    V_25
      IL_13a3:  brfalse.s  IL_13b2

      IL_13a5:  ldloc.s    V_25
      IL_13a7:  ldloc.s    V_24
      IL_13a9:  stloc.s    V_27
      IL_13ab:  stloc.s    V_26
      IL_13ad:  br         IL_012f

      IL_13b2:  ldarg.2
      IL_13b3:  isinst     [mscorlib]System.Array
      IL_13b8:  stloc.s    V_28
      IL_13ba:  ldloc.s    V_28
      IL_13bc:  brfalse    IL_0156

      IL_13c1:  ldloc.s    V_28
      IL_13c3:  stloc.s    V_29
      IL_13c5:  br         IL_014a

      IL_13ca:  ldarg.2
      IL_13cb:  isinst     [mscorlib]System.Array
      IL_13d0:  stloc.s    V_28
      IL_13d2:  ldloc.s    V_28
      IL_13d4:  brfalse    IL_0156

      IL_13d9:  ldloc.s    V_28
      IL_13db:  stloc.s    V_29
      IL_13dd:  br         IL_014a

      IL_13e2:  ldarg.s    arr1
      IL_13e4:  isinst     int64[]
      IL_13e9:  stloc.s    V_12
      IL_13eb:  ldloc.s    V_12
      IL_13ed:  brfalse    IL_15d8

      IL_13f2:  ldarg.2
      IL_13f3:  isinst     int64[]
      IL_13f8:  stloc.s    V_13
      IL_13fa:  ldloc.s    V_13
      IL_13fc:  brfalse.s  IL_140b

      IL_13fe:  ldloc.s    V_13
      IL_1400:  ldloc.s    V_12
      IL_1402:  stloc.s    V_15
      IL_1404:  stloc.s    V_14
      IL_1406:  br         IL_00a7

      IL_140b:  ldarg.s    arr1
      IL_140d:  isinst     char[]
      IL_1412:  stloc.s    V_16
      IL_1414:  ldloc.s    V_16
      IL_1416:  brfalse    IL_1506

      IL_141b:  ldarg.2
      IL_141c:  isinst     char[]
      IL_1421:  stloc.s    V_17
      IL_1423:  ldloc.s    V_17
      IL_1425:  brfalse.s  IL_1434

      IL_1427:  ldloc.s    V_17
      IL_1429:  ldloc.s    V_16
      IL_142b:  stloc.s    V_19
      IL_142d:  stloc.s    V_18
      IL_142f:  br         IL_00d5

      IL_1434:  ldarg.s    arr1
      IL_1436:  isinst     float32[]
      IL_143b:  stloc.s    V_20
      IL_143d:  ldloc.s    V_20
      IL_143f:  brfalse.s  IL_14b0

      IL_1441:  ldarg.2
      IL_1442:  isinst     float32[]
      IL_1447:  stloc.s    V_21
      IL_1449:  ldloc.s    V_21
      IL_144b:  brfalse.s  IL_145a

      IL_144d:  ldloc.s    V_21
      IL_144f:  ldloc.s    V_20
      IL_1451:  stloc.s    V_23
      IL_1453:  stloc.s    V_22
      IL_1455:  br         IL_0103

      IL_145a:  ldarg.s    arr1
      IL_145c:  isinst     float64[]
      IL_1461:  stloc.s    V_24
      IL_1463:  ldloc.s    V_24
      IL_1465:  brfalse.s  IL_1498

      IL_1467:  ldarg.2
      IL_1468:  isinst     float64[]
      IL_146d:  stloc.s    V_25
      IL_146f:  ldloc.s    V_25
      IL_1471:  brfalse.s  IL_1480

      IL_1473:  ldloc.s    V_25
      IL_1475:  ldloc.s    V_24
      IL_1477:  stloc.s    V_27
      IL_1479:  stloc.s    V_26
      IL_147b:  br         IL_012f

      IL_1480:  ldarg.2
      IL_1481:  isinst     [mscorlib]System.Array
      IL_1486:  stloc.s    V_28
      IL_1488:  ldloc.s    V_28
      IL_148a:  brfalse    IL_0156

      IL_148f:  ldloc.s    V_28
      IL_1491:  stloc.s    V_29
      IL_1493:  br         IL_014a

      IL_1498:  ldarg.2
      IL_1499:  isinst     [mscorlib]System.Array
      IL_149e:  stloc.s    V_28
      IL_14a0:  ldloc.s    V_28
      IL_14a2:  brfalse    IL_0156

      IL_14a7:  ldloc.s    V_28
      IL_14a9:  stloc.s    V_29
      IL_14ab:  br         IL_014a

      IL_14b0:  ldarg.s    arr1
      IL_14b2:  isinst     float64[]
      IL_14b7:  stloc.s    V_24
      IL_14b9:  ldloc.s    V_24
      IL_14bb:  brfalse.s  IL_14ee

      IL_14bd:  ldarg.2
      IL_14be:  isinst     float64[]
      IL_14c3:  stloc.s    V_25
      IL_14c5:  ldloc.s    V_25
      IL_14c7:  brfalse.s  IL_14d6

      IL_14c9:  ldloc.s    V_25
      IL_14cb:  ldloc.s    V_24
      IL_14cd:  stloc.s    V_27
      IL_14cf:  stloc.s    V_26
      IL_14d1:  br         IL_012f

      IL_14d6:  ldarg.2
      IL_14d7:  isinst     [mscorlib]System.Array
      IL_14dc:  stloc.s    V_28
      IL_14de:  ldloc.s    V_28
      IL_14e0:  brfalse    IL_0156

      IL_14e5:  ldloc.s    V_28
      IL_14e7:  stloc.s    V_29
      IL_14e9:  br         IL_014a

      IL_14ee:  ldarg.2
      IL_14ef:  isinst     [mscorlib]System.Array
      IL_14f4:  stloc.s    V_28
      IL_14f6:  ldloc.s    V_28
      IL_14f8:  brfalse    IL_0156

      IL_14fd:  ldloc.s    V_28
      IL_14ff:  stloc.s    V_29
      IL_1501:  br         IL_014a

      IL_1506:  ldarg.s    arr1
      IL_1508:  isinst     float32[]
      IL_150d:  stloc.s    V_20
      IL_150f:  ldloc.s    V_20
      IL_1511:  brfalse.s  IL_1582

      IL_1513:  ldarg.2
      IL_1514:  isinst     float32[]
      IL_1519:  stloc.s    V_21
      IL_151b:  ldloc.s    V_21
      IL_151d:  brfalse.s  IL_152c

      IL_151f:  ldloc.s    V_21
      IL_1521:  ldloc.s    V_20
      IL_1523:  stloc.s    V_23
      IL_1525:  stloc.s    V_22
      IL_1527:  br         IL_0103

      IL_152c:  ldarg.s    arr1
      IL_152e:  isinst     float64[]
      IL_1533:  stloc.s    V_24
      IL_1535:  ldloc.s    V_24
      IL_1537:  brfalse.s  IL_156a

      IL_1539:  ldarg.2
      IL_153a:  isinst     float64[]
      IL_153f:  stloc.s    V_25
      IL_1541:  ldloc.s    V_25
      IL_1543:  brfalse.s  IL_1552

      IL_1545:  ldloc.s    V_25
      IL_1547:  ldloc.s    V_24
      IL_1549:  stloc.s    V_27
      IL_154b:  stloc.s    V_26
      IL_154d:  br         IL_012f

      IL_1552:  ldarg.2
      IL_1553:  isinst     [mscorlib]System.Array
      IL_1558:  stloc.s    V_28
      IL_155a:  ldloc.s    V_28
      IL_155c:  brfalse    IL_0156

      IL_1561:  ldloc.s    V_28
      IL_1563:  stloc.s    V_29
      IL_1565:  br         IL_014a

      IL_156a:  ldarg.2
      IL_156b:  isinst     [mscorlib]System.Array
      IL_1570:  stloc.s    V_28
      IL_1572:  ldloc.s    V_28
      IL_1574:  brfalse    IL_0156

      IL_1579:  ldloc.s    V_28
      IL_157b:  stloc.s    V_29
      IL_157d:  br         IL_014a

      IL_1582:  ldarg.s    arr1
      IL_1584:  isinst     float64[]
      IL_1589:  stloc.s    V_24
      IL_158b:  ldloc.s    V_24
      IL_158d:  brfalse.s  IL_15c0

      IL_158f:  ldarg.2
      IL_1590:  isinst     float64[]
      IL_1595:  stloc.s    V_25
      IL_1597:  ldloc.s    V_25
      IL_1599:  brfalse.s  IL_15a8

      IL_159b:  ldloc.s    V_25
      IL_159d:  ldloc.s    V_24
      IL_159f:  stloc.s    V_27
      IL_15a1:  stloc.s    V_26
      IL_15a3:  br         IL_012f

      IL_15a8:  ldarg.2
      IL_15a9:  isinst     [mscorlib]System.Array
      IL_15ae:  stloc.s    V_28
      IL_15b0:  ldloc.s    V_28
      IL_15b2:  brfalse    IL_0156

      IL_15b7:  ldloc.s    V_28
      IL_15b9:  stloc.s    V_29
      IL_15bb:  br         IL_014a

      IL_15c0:  ldarg.2
      IL_15c1:  isinst     [mscorlib]System.Array
      IL_15c6:  stloc.s    V_28
      IL_15c8:  ldloc.s    V_28
      IL_15ca:  brfalse    IL_0156

      IL_15cf:  ldloc.s    V_28
      IL_15d1:  stloc.s    V_29
      IL_15d3:  br         IL_014a

      IL_15d8:  ldarg.s    arr1
      IL_15da:  isinst     char[]
      IL_15df:  stloc.s    V_16
      IL_15e1:  ldloc.s    V_16
      IL_15e3:  brfalse    IL_16d3

      IL_15e8:  ldarg.2
      IL_15e9:  isinst     char[]
      IL_15ee:  stloc.s    V_17
      IL_15f0:  ldloc.s    V_17
      IL_15f2:  brfalse.s  IL_1601

      IL_15f4:  ldloc.s    V_17
      IL_15f6:  ldloc.s    V_16
      IL_15f8:  stloc.s    V_19
      IL_15fa:  stloc.s    V_18
      IL_15fc:  br         IL_00d5

      IL_1601:  ldarg.s    arr1
      IL_1603:  isinst     float32[]
      IL_1608:  stloc.s    V_20
      IL_160a:  ldloc.s    V_20
      IL_160c:  brfalse.s  IL_167d

      IL_160e:  ldarg.2
      IL_160f:  isinst     float32[]
      IL_1614:  stloc.s    V_21
      IL_1616:  ldloc.s    V_21
      IL_1618:  brfalse.s  IL_1627

      IL_161a:  ldloc.s    V_21
      IL_161c:  ldloc.s    V_20
      IL_161e:  stloc.s    V_23
      IL_1620:  stloc.s    V_22
      IL_1622:  br         IL_0103

      IL_1627:  ldarg.s    arr1
      IL_1629:  isinst     float64[]
      IL_162e:  stloc.s    V_24
      IL_1630:  ldloc.s    V_24
      IL_1632:  brfalse.s  IL_1665

      IL_1634:  ldarg.2
      IL_1635:  isinst     float64[]
      IL_163a:  stloc.s    V_25
      IL_163c:  ldloc.s    V_25
      IL_163e:  brfalse.s  IL_164d

      IL_1640:  ldloc.s    V_25
      IL_1642:  ldloc.s    V_24
      IL_1644:  stloc.s    V_27
      IL_1646:  stloc.s    V_26
      IL_1648:  br         IL_012f

      IL_164d:  ldarg.2
      IL_164e:  isinst     [mscorlib]System.Array
      IL_1653:  stloc.s    V_28
      IL_1655:  ldloc.s    V_28
      IL_1657:  brfalse    IL_0156

      IL_165c:  ldloc.s    V_28
      IL_165e:  stloc.s    V_29
      IL_1660:  br         IL_014a

      IL_1665:  ldarg.2
      IL_1666:  isinst     [mscorlib]System.Array
      IL_166b:  stloc.s    V_28
      IL_166d:  ldloc.s    V_28
      IL_166f:  brfalse    IL_0156

      IL_1674:  ldloc.s    V_28
      IL_1676:  stloc.s    V_29
      IL_1678:  br         IL_014a

      IL_167d:  ldarg.s    arr1
      IL_167f:  isinst     float64[]
      IL_1684:  stloc.s    V_24
      IL_1686:  ldloc.s    V_24
      IL_1688:  brfalse.s  IL_16bb

      IL_168a:  ldarg.2
      IL_168b:  isinst     float64[]
      IL_1690:  stloc.s    V_25
      IL_1692:  ldloc.s    V_25
      IL_1694:  brfalse.s  IL_16a3

      IL_1696:  ldloc.s    V_25
      IL_1698:  ldloc.s    V_24
      IL_169a:  stloc.s    V_27
      IL_169c:  stloc.s    V_26
      IL_169e:  br         IL_012f

      IL_16a3:  ldarg.2
      IL_16a4:  isinst     [mscorlib]System.Array
      IL_16a9:  stloc.s    V_28
      IL_16ab:  ldloc.s    V_28
      IL_16ad:  brfalse    IL_0156

      IL_16b2:  ldloc.s    V_28
      IL_16b4:  stloc.s    V_29
      IL_16b6:  br         IL_014a

      IL_16bb:  ldarg.2
      IL_16bc:  isinst     [mscorlib]System.Array
      IL_16c1:  stloc.s    V_28
      IL_16c3:  ldloc.s    V_28
      IL_16c5:  brfalse    IL_0156

      IL_16ca:  ldloc.s    V_28
      IL_16cc:  stloc.s    V_29
      IL_16ce:  br         IL_014a

      IL_16d3:  ldarg.s    arr1
      IL_16d5:  isinst     float32[]
      IL_16da:  stloc.s    V_20
      IL_16dc:  ldloc.s    V_20
      IL_16de:  brfalse.s  IL_174f

      IL_16e0:  ldarg.2
      IL_16e1:  isinst     float32[]
      IL_16e6:  stloc.s    V_21
      IL_16e8:  ldloc.s    V_21
      IL_16ea:  brfalse.s  IL_16f9

      IL_16ec:  ldloc.s    V_21
      IL_16ee:  ldloc.s    V_20
      IL_16f0:  stloc.s    V_23
      IL_16f2:  stloc.s    V_22
      IL_16f4:  br         IL_0103

      IL_16f9:  ldarg.s    arr1
      IL_16fb:  isinst     float64[]
      IL_1700:  stloc.s    V_24
      IL_1702:  ldloc.s    V_24
      IL_1704:  brfalse.s  IL_1737

      IL_1706:  ldarg.2
      IL_1707:  isinst     float64[]
      IL_170c:  stloc.s    V_25
      IL_170e:  ldloc.s    V_25
      IL_1710:  brfalse.s  IL_171f

      IL_1712:  ldloc.s    V_25
      IL_1714:  ldloc.s    V_24
      IL_1716:  stloc.s    V_27
      IL_1718:  stloc.s    V_26
      IL_171a:  br         IL_012f

      IL_171f:  ldarg.2
      IL_1720:  isinst     [mscorlib]System.Array
      IL_1725:  stloc.s    V_28
      IL_1727:  ldloc.s    V_28
      IL_1729:  brfalse    IL_0156

      IL_172e:  ldloc.s    V_28
      IL_1730:  stloc.s    V_29
      IL_1732:  br         IL_014a

      IL_1737:  ldarg.2
      IL_1738:  isinst     [mscorlib]System.Array
      IL_173d:  stloc.s    V_28
      IL_173f:  ldloc.s    V_28
      IL_1741:  brfalse    IL_0156

      IL_1746:  ldloc.s    V_28
      IL_1748:  stloc.s    V_29
      IL_174a:  br         IL_014a

      IL_174f:  ldarg.s    arr1
      IL_1751:  isinst     float64[]
      IL_1756:  stloc.s    V_24
      IL_1758:  ldloc.s    V_24
      IL_175a:  brfalse.s  IL_178d

      IL_175c:  ldarg.2
      IL_175d:  isinst     float64[]
      IL_1762:  stloc.s    V_25
      IL_1764:  ldloc.s    V_25
      IL_1766:  brfalse.s  IL_1775

      IL_1768:  ldloc.s    V_25
      IL_176a:  ldloc.s    V_24
      IL_176c:  stloc.s    V_27
      IL_176e:  stloc.s    V_26
      IL_1770:  br         IL_012f

      IL_1775:  ldarg.2
      IL_1776:  isinst     [mscorlib]System.Array
      IL_177b:  stloc.s    V_28
      IL_177d:  ldloc.s    V_28
      IL_177f:  brfalse    IL_0156

      IL_1784:  ldloc.s    V_28
      IL_1786:  stloc.s    V_29
      IL_1788:  br         IL_014a

      IL_178d:  ldarg.2
      IL_178e:  isinst     [mscorlib]System.Array
      IL_1793:  stloc.s    V_28
      IL_1795:  ldloc.s    V_28
      IL_1797:  brfalse    IL_0156

      IL_179c:  ldloc.s    V_28
      IL_179e:  stloc.s    V_29
      IL_17a0:  br         IL_014a

      IL_17a5:  ldarg.s    arr1
      IL_17a7:  isinst     int32[]
      IL_17ac:  stloc.s    V_8
      IL_17ae:  ldloc.s    V_8
      IL_17b0:  brfalse    IL_1b91

      IL_17b5:  ldarg.2
      IL_17b6:  isinst     int32[]
      IL_17bb:  stloc.s    V_9
      IL_17bd:  ldloc.s    V_9
      IL_17bf:  brfalse.s  IL_17ce

      IL_17c1:  ldloc.s    V_9
      IL_17c3:  ldloc.s    V_8
      IL_17c5:  stloc.s    V_11
      IL_17c7:  stloc.s    V_10
      IL_17c9:  br         IL_0079

      IL_17ce:  ldarg.s    arr1
      IL_17d0:  isinst     int64[]
      IL_17d5:  stloc.s    V_12
      IL_17d7:  ldloc.s    V_12
      IL_17d9:  brfalse    IL_19c4

      IL_17de:  ldarg.2
      IL_17df:  isinst     int64[]
      IL_17e4:  stloc.s    V_13
      IL_17e6:  ldloc.s    V_13
      IL_17e8:  brfalse.s  IL_17f7

      IL_17ea:  ldloc.s    V_13
      IL_17ec:  ldloc.s    V_12
      IL_17ee:  stloc.s    V_15
      IL_17f0:  stloc.s    V_14
      IL_17f2:  br         IL_00a7

      IL_17f7:  ldarg.s    arr1
      IL_17f9:  isinst     char[]
      IL_17fe:  stloc.s    V_16
      IL_1800:  ldloc.s    V_16
      IL_1802:  brfalse    IL_18f2

      IL_1807:  ldarg.2
      IL_1808:  isinst     char[]
      IL_180d:  stloc.s    V_17
      IL_180f:  ldloc.s    V_17
      IL_1811:  brfalse.s  IL_1820

      IL_1813:  ldloc.s    V_17
      IL_1815:  ldloc.s    V_16
      IL_1817:  stloc.s    V_19
      IL_1819:  stloc.s    V_18
      IL_181b:  br         IL_00d5

      IL_1820:  ldarg.s    arr1
      IL_1822:  isinst     float32[]
      IL_1827:  stloc.s    V_20
      IL_1829:  ldloc.s    V_20
      IL_182b:  brfalse.s  IL_189c

      IL_182d:  ldarg.2
      IL_182e:  isinst     float32[]
      IL_1833:  stloc.s    V_21
      IL_1835:  ldloc.s    V_21
      IL_1837:  brfalse.s  IL_1846

      IL_1839:  ldloc.s    V_21
      IL_183b:  ldloc.s    V_20
      IL_183d:  stloc.s    V_23
      IL_183f:  stloc.s    V_22
      IL_1841:  br         IL_0103

      IL_1846:  ldarg.s    arr1
      IL_1848:  isinst     float64[]
      IL_184d:  stloc.s    V_24
      IL_184f:  ldloc.s    V_24
      IL_1851:  brfalse.s  IL_1884

      IL_1853:  ldarg.2
      IL_1854:  isinst     float64[]
      IL_1859:  stloc.s    V_25
      IL_185b:  ldloc.s    V_25
      IL_185d:  brfalse.s  IL_186c

      IL_185f:  ldloc.s    V_25
      IL_1861:  ldloc.s    V_24
      IL_1863:  stloc.s    V_27
      IL_1865:  stloc.s    V_26
      IL_1867:  br         IL_012f

      IL_186c:  ldarg.2
      IL_186d:  isinst     [mscorlib]System.Array
      IL_1872:  stloc.s    V_28
      IL_1874:  ldloc.s    V_28
      IL_1876:  brfalse    IL_0156

      IL_187b:  ldloc.s    V_28
      IL_187d:  stloc.s    V_29
      IL_187f:  br         IL_014a

      IL_1884:  ldarg.2
      IL_1885:  isinst     [mscorlib]System.Array
      IL_188a:  stloc.s    V_28
      IL_188c:  ldloc.s    V_28
      IL_188e:  brfalse    IL_0156

      IL_1893:  ldloc.s    V_28
      IL_1895:  stloc.s    V_29
      IL_1897:  br         IL_014a

      IL_189c:  ldarg.s    arr1
      IL_189e:  isinst     float64[]
      IL_18a3:  stloc.s    V_24
      IL_18a5:  ldloc.s    V_24
      IL_18a7:  brfalse.s  IL_18da

      IL_18a9:  ldarg.2
      IL_18aa:  isinst     float64[]
      IL_18af:  stloc.s    V_25
      IL_18b1:  ldloc.s    V_25
      IL_18b3:  brfalse.s  IL_18c2

      IL_18b5:  ldloc.s    V_25
      IL_18b7:  ldloc.s    V_24
      IL_18b9:  stloc.s    V_27
      IL_18bb:  stloc.s    V_26
      IL_18bd:  br         IL_012f

      IL_18c2:  ldarg.2
      IL_18c3:  isinst     [mscorlib]System.Array
      IL_18c8:  stloc.s    V_28
      IL_18ca:  ldloc.s    V_28
      IL_18cc:  brfalse    IL_0156

      IL_18d1:  ldloc.s    V_28
      IL_18d3:  stloc.s    V_29
      IL_18d5:  br         IL_014a

      IL_18da:  ldarg.2
      IL_18db:  isinst     [mscorlib]System.Array
      IL_18e0:  stloc.s    V_28
      IL_18e2:  ldloc.s    V_28
      IL_18e4:  brfalse    IL_0156

      IL_18e9:  ldloc.s    V_28
      IL_18eb:  stloc.s    V_29
      IL_18ed:  br         IL_014a

      IL_18f2:  ldarg.s    arr1
      IL_18f4:  isinst     float32[]
      IL_18f9:  stloc.s    V_20
      IL_18fb:  ldloc.s    V_20
      IL_18fd:  brfalse.s  IL_196e

      IL_18ff:  ldarg.2
      IL_1900:  isinst     float32[]
      IL_1905:  stloc.s    V_21
      IL_1907:  ldloc.s    V_21
      IL_1909:  brfalse.s  IL_1918

      IL_190b:  ldloc.s    V_21
      IL_190d:  ldloc.s    V_20
      IL_190f:  stloc.s    V_23
      IL_1911:  stloc.s    V_22
      IL_1913:  br         IL_0103

      IL_1918:  ldarg.s    arr1
      IL_191a:  isinst     float64[]
      IL_191f:  stloc.s    V_24
      IL_1921:  ldloc.s    V_24
      IL_1923:  brfalse.s  IL_1956

      IL_1925:  ldarg.2
      IL_1926:  isinst     float64[]
      IL_192b:  stloc.s    V_25
      IL_192d:  ldloc.s    V_25
      IL_192f:  brfalse.s  IL_193e

      IL_1931:  ldloc.s    V_25
      IL_1933:  ldloc.s    V_24
      IL_1935:  stloc.s    V_27
      IL_1937:  stloc.s    V_26
      IL_1939:  br         IL_012f

      IL_193e:  ldarg.2
      IL_193f:  isinst     [mscorlib]System.Array
      IL_1944:  stloc.s    V_28
      IL_1946:  ldloc.s    V_28
      IL_1948:  brfalse    IL_0156

      IL_194d:  ldloc.s    V_28
      IL_194f:  stloc.s    V_29
      IL_1951:  br         IL_014a

      IL_1956:  ldarg.2
      IL_1957:  isinst     [mscorlib]System.Array
      IL_195c:  stloc.s    V_28
      IL_195e:  ldloc.s    V_28
      IL_1960:  brfalse    IL_0156

      IL_1965:  ldloc.s    V_28
      IL_1967:  stloc.s    V_29
      IL_1969:  br         IL_014a

      IL_196e:  ldarg.s    arr1
      IL_1970:  isinst     float64[]
      IL_1975:  stloc.s    V_24
      IL_1977:  ldloc.s    V_24
      IL_1979:  brfalse.s  IL_19ac

      IL_197b:  ldarg.2
      IL_197c:  isinst     float64[]
      IL_1981:  stloc.s    V_25
      IL_1983:  ldloc.s    V_25
      IL_1985:  brfalse.s  IL_1994

      IL_1987:  ldloc.s    V_25
      IL_1989:  ldloc.s    V_24
      IL_198b:  stloc.s    V_27
      IL_198d:  stloc.s    V_26
      IL_198f:  br         IL_012f

      IL_1994:  ldarg.2
      IL_1995:  isinst     [mscorlib]System.Array
      IL_199a:  stloc.s    V_28
      IL_199c:  ldloc.s    V_28
      IL_199e:  brfalse    IL_0156

      IL_19a3:  ldloc.s    V_28
      IL_19a5:  stloc.s    V_29
      IL_19a7:  br         IL_014a

      IL_19ac:  ldarg.2
      IL_19ad:  isinst     [mscorlib]System.Array
      IL_19b2:  stloc.s    V_28
      IL_19b4:  ldloc.s    V_28
      IL_19b6:  brfalse    IL_0156

      IL_19bb:  ldloc.s    V_28
      IL_19bd:  stloc.s    V_29
      IL_19bf:  br         IL_014a

      IL_19c4:  ldarg.s    arr1
      IL_19c6:  isinst     char[]
      IL_19cb:  stloc.s    V_16
      IL_19cd:  ldloc.s    V_16
      IL_19cf:  brfalse    IL_1abf

      IL_19d4:  ldarg.2
      IL_19d5:  isinst     char[]
      IL_19da:  stloc.s    V_17
      IL_19dc:  ldloc.s    V_17
      IL_19de:  brfalse.s  IL_19ed

      IL_19e0:  ldloc.s    V_17
      IL_19e2:  ldloc.s    V_16
      IL_19e4:  stloc.s    V_19
      IL_19e6:  stloc.s    V_18
      IL_19e8:  br         IL_00d5

      IL_19ed:  ldarg.s    arr1
      IL_19ef:  isinst     float32[]
      IL_19f4:  stloc.s    V_20
      IL_19f6:  ldloc.s    V_20
      IL_19f8:  brfalse.s  IL_1a69

      IL_19fa:  ldarg.2
      IL_19fb:  isinst     float32[]
      IL_1a00:  stloc.s    V_21
      IL_1a02:  ldloc.s    V_21
      IL_1a04:  brfalse.s  IL_1a13

      IL_1a06:  ldloc.s    V_21
      IL_1a08:  ldloc.s    V_20
      IL_1a0a:  stloc.s    V_23
      IL_1a0c:  stloc.s    V_22
      IL_1a0e:  br         IL_0103

      IL_1a13:  ldarg.s    arr1
      IL_1a15:  isinst     float64[]
      IL_1a1a:  stloc.s    V_24
      IL_1a1c:  ldloc.s    V_24
      IL_1a1e:  brfalse.s  IL_1a51

      IL_1a20:  ldarg.2
      IL_1a21:  isinst     float64[]
      IL_1a26:  stloc.s    V_25
      IL_1a28:  ldloc.s    V_25
      IL_1a2a:  brfalse.s  IL_1a39

      IL_1a2c:  ldloc.s    V_25
      IL_1a2e:  ldloc.s    V_24
      IL_1a30:  stloc.s    V_27
      IL_1a32:  stloc.s    V_26
      IL_1a34:  br         IL_012f

      IL_1a39:  ldarg.2
      IL_1a3a:  isinst     [mscorlib]System.Array
      IL_1a3f:  stloc.s    V_28
      IL_1a41:  ldloc.s    V_28
      IL_1a43:  brfalse    IL_0156

      IL_1a48:  ldloc.s    V_28
      IL_1a4a:  stloc.s    V_29
      IL_1a4c:  br         IL_014a

      IL_1a51:  ldarg.2
      IL_1a52:  isinst     [mscorlib]System.Array
      IL_1a57:  stloc.s    V_28
      IL_1a59:  ldloc.s    V_28
      IL_1a5b:  brfalse    IL_0156

      IL_1a60:  ldloc.s    V_28
      IL_1a62:  stloc.s    V_29
      IL_1a64:  br         IL_014a

      IL_1a69:  ldarg.s    arr1
      IL_1a6b:  isinst     float64[]
      IL_1a70:  stloc.s    V_24
      IL_1a72:  ldloc.s    V_24
      IL_1a74:  brfalse.s  IL_1aa7

      IL_1a76:  ldarg.2
      IL_1a77:  isinst     float64[]
      IL_1a7c:  stloc.s    V_25
      IL_1a7e:  ldloc.s    V_25
      IL_1a80:  brfalse.s  IL_1a8f

      IL_1a82:  ldloc.s    V_25
      IL_1a84:  ldloc.s    V_24
      IL_1a86:  stloc.s    V_27
      IL_1a88:  stloc.s    V_26
      IL_1a8a:  br         IL_012f

      IL_1a8f:  ldarg.2
      IL_1a90:  isinst     [mscorlib]System.Array
      IL_1a95:  stloc.s    V_28
      IL_1a97:  ldloc.s    V_28
      IL_1a99:  brfalse    IL_0156

      IL_1a9e:  ldloc.s    V_28
      IL_1aa0:  stloc.s    V_29
      IL_1aa2:  br         IL_014a

      IL_1aa7:  ldarg.2
      IL_1aa8:  isinst     [mscorlib]System.Array
      IL_1aad:  stloc.s    V_28
      IL_1aaf:  ldloc.s    V_28
      IL_1ab1:  brfalse    IL_0156

      IL_1ab6:  ldloc.s    V_28
      IL_1ab8:  stloc.s    V_29
      IL_1aba:  br         IL_014a

      IL_1abf:  ldarg.s    arr1
      IL_1ac1:  isinst     float32[]
      IL_1ac6:  stloc.s    V_20
      IL_1ac8:  ldloc.s    V_20
      IL_1aca:  brfalse.s  IL_1b3b

      IL_1acc:  ldarg.2
      IL_1acd:  isinst     float32[]
      IL_1ad2:  stloc.s    V_21
      IL_1ad4:  ldloc.s    V_21
      IL_1ad6:  brfalse.s  IL_1ae5

      IL_1ad8:  ldloc.s    V_21
      IL_1ada:  ldloc.s    V_20
      IL_1adc:  stloc.s    V_23
      IL_1ade:  stloc.s    V_22
      IL_1ae0:  br         IL_0103

      IL_1ae5:  ldarg.s    arr1
      IL_1ae7:  isinst     float64[]
      IL_1aec:  stloc.s    V_24
      IL_1aee:  ldloc.s    V_24
      IL_1af0:  brfalse.s  IL_1b23

      IL_1af2:  ldarg.2
      IL_1af3:  isinst     float64[]
      IL_1af8:  stloc.s    V_25
      IL_1afa:  ldloc.s    V_25
      IL_1afc:  brfalse.s  IL_1b0b

      IL_1afe:  ldloc.s    V_25
      IL_1b00:  ldloc.s    V_24
      IL_1b02:  stloc.s    V_27
      IL_1b04:  stloc.s    V_26
      IL_1b06:  br         IL_012f

      IL_1b0b:  ldarg.2
      IL_1b0c:  isinst     [mscorlib]System.Array
      IL_1b11:  stloc.s    V_28
      IL_1b13:  ldloc.s    V_28
      IL_1b15:  brfalse    IL_0156

      IL_1b1a:  ldloc.s    V_28
      IL_1b1c:  stloc.s    V_29
      IL_1b1e:  br         IL_014a

      IL_1b23:  ldarg.2
      IL_1b24:  isinst     [mscorlib]System.Array
      IL_1b29:  stloc.s    V_28
      IL_1b2b:  ldloc.s    V_28
      IL_1b2d:  brfalse    IL_0156

      IL_1b32:  ldloc.s    V_28
      IL_1b34:  stloc.s    V_29
      IL_1b36:  br         IL_014a

      IL_1b3b:  ldarg.s    arr1
      IL_1b3d:  isinst     float64[]
      IL_1b42:  stloc.s    V_24
      IL_1b44:  ldloc.s    V_24
      IL_1b46:  brfalse.s  IL_1b79

      IL_1b48:  ldarg.2
      IL_1b49:  isinst     float64[]
      IL_1b4e:  stloc.s    V_25
      IL_1b50:  ldloc.s    V_25
      IL_1b52:  brfalse.s  IL_1b61

      IL_1b54:  ldloc.s    V_25
      IL_1b56:  ldloc.s    V_24
      IL_1b58:  stloc.s    V_27
      IL_1b5a:  stloc.s    V_26
      IL_1b5c:  br         IL_012f

      IL_1b61:  ldarg.2
      IL_1b62:  isinst     [mscorlib]System.Array
      IL_1b67:  stloc.s    V_28
      IL_1b69:  ldloc.s    V_28
      IL_1b6b:  brfalse    IL_0156

      IL_1b70:  ldloc.s    V_28
      IL_1b72:  stloc.s    V_29
      IL_1b74:  br         IL_014a

      IL_1b79:  ldarg.2
      IL_1b7a:  isinst     [mscorlib]System.Array
      IL_1b7f:  stloc.s    V_28
      IL_1b81:  ldloc.s    V_28
      IL_1b83:  brfalse    IL_0156

      IL_1b88:  ldloc.s    V_28
      IL_1b8a:  stloc.s    V_29
      IL_1b8c:  br         IL_014a

      IL_1b91:  ldarg.s    arr1
      IL_1b93:  isinst     int64[]
      IL_1b98:  stloc.s    V_12
      IL_1b9a:  ldloc.s    V_12
      IL_1b9c:  brfalse    IL_1d87

      IL_1ba1:  ldarg.2
      IL_1ba2:  isinst     int64[]
      IL_1ba7:  stloc.s    V_13
      IL_1ba9:  ldloc.s    V_13
      IL_1bab:  brfalse.s  IL_1bba

      IL_1bad:  ldloc.s    V_13
      IL_1baf:  ldloc.s    V_12
      IL_1bb1:  stloc.s    V_15
      IL_1bb3:  stloc.s    V_14
      IL_1bb5:  br         IL_00a7

      IL_1bba:  ldarg.s    arr1
      IL_1bbc:  isinst     char[]
      IL_1bc1:  stloc.s    V_16
      IL_1bc3:  ldloc.s    V_16
      IL_1bc5:  brfalse    IL_1cb5

      IL_1bca:  ldarg.2
      IL_1bcb:  isinst     char[]
      IL_1bd0:  stloc.s    V_17
      IL_1bd2:  ldloc.s    V_17
      IL_1bd4:  brfalse.s  IL_1be3

      IL_1bd6:  ldloc.s    V_17
      IL_1bd8:  ldloc.s    V_16
      IL_1bda:  stloc.s    V_19
      IL_1bdc:  stloc.s    V_18
      IL_1bde:  br         IL_00d5

      IL_1be3:  ldarg.s    arr1
      IL_1be5:  isinst     float32[]
      IL_1bea:  stloc.s    V_20
      IL_1bec:  ldloc.s    V_20
      IL_1bee:  brfalse.s  IL_1c5f

      IL_1bf0:  ldarg.2
      IL_1bf1:  isinst     float32[]
      IL_1bf6:  stloc.s    V_21
      IL_1bf8:  ldloc.s    V_21
      IL_1bfa:  brfalse.s  IL_1c09

      IL_1bfc:  ldloc.s    V_21
      IL_1bfe:  ldloc.s    V_20
      IL_1c00:  stloc.s    V_23
      IL_1c02:  stloc.s    V_22
      IL_1c04:  br         IL_0103

      IL_1c09:  ldarg.s    arr1
      IL_1c0b:  isinst     float64[]
      IL_1c10:  stloc.s    V_24
      IL_1c12:  ldloc.s    V_24
      IL_1c14:  brfalse.s  IL_1c47

      IL_1c16:  ldarg.2
      IL_1c17:  isinst     float64[]
      IL_1c1c:  stloc.s    V_25
      IL_1c1e:  ldloc.s    V_25
      IL_1c20:  brfalse.s  IL_1c2f

      IL_1c22:  ldloc.s    V_25
      IL_1c24:  ldloc.s    V_24
      IL_1c26:  stloc.s    V_27
      IL_1c28:  stloc.s    V_26
      IL_1c2a:  br         IL_012f

      IL_1c2f:  ldarg.2
      IL_1c30:  isinst     [mscorlib]System.Array
      IL_1c35:  stloc.s    V_28
      IL_1c37:  ldloc.s    V_28
      IL_1c39:  brfalse    IL_0156

      IL_1c3e:  ldloc.s    V_28
      IL_1c40:  stloc.s    V_29
      IL_1c42:  br         IL_014a

      IL_1c47:  ldarg.2
      IL_1c48:  isinst     [mscorlib]System.Array
      IL_1c4d:  stloc.s    V_28
      IL_1c4f:  ldloc.s    V_28
      IL_1c51:  brfalse    IL_0156

      IL_1c56:  ldloc.s    V_28
      IL_1c58:  stloc.s    V_29
      IL_1c5a:  br         IL_014a

      IL_1c5f:  ldarg.s    arr1
      IL_1c61:  isinst     float64[]
      IL_1c66:  stloc.s    V_24
      IL_1c68:  ldloc.s    V_24
      IL_1c6a:  brfalse.s  IL_1c9d

      IL_1c6c:  ldarg.2
      IL_1c6d:  isinst     float64[]
      IL_1c72:  stloc.s    V_25
      IL_1c74:  ldloc.s    V_25
      IL_1c76:  brfalse.s  IL_1c85

      IL_1c78:  ldloc.s    V_25
      IL_1c7a:  ldloc.s    V_24
      IL_1c7c:  stloc.s    V_27
      IL_1c7e:  stloc.s    V_26
      IL_1c80:  br         IL_012f

      IL_1c85:  ldarg.2
      IL_1c86:  isinst     [mscorlib]System.Array
      IL_1c8b:  stloc.s    V_28
      IL_1c8d:  ldloc.s    V_28
      IL_1c8f:  brfalse    IL_0156

      IL_1c94:  ldloc.s    V_28
      IL_1c96:  stloc.s    V_29
      IL_1c98:  br         IL_014a

      IL_1c9d:  ldarg.2
      IL_1c9e:  isinst     [mscorlib]System.Array
      IL_1ca3:  stloc.s    V_28
      IL_1ca5:  ldloc.s    V_28
      IL_1ca7:  brfalse    IL_0156

      IL_1cac:  ldloc.s    V_28
      IL_1cae:  stloc.s    V_29
      IL_1cb0:  br         IL_014a

      IL_1cb5:  ldarg.s    arr1
      IL_1cb7:  isinst     float32[]
      IL_1cbc:  stloc.s    V_20
      IL_1cbe:  ldloc.s    V_20
      IL_1cc0:  brfalse.s  IL_1d31

      IL_1cc2:  ldarg.2
      IL_1cc3:  isinst     float32[]
      IL_1cc8:  stloc.s    V_21
      IL_1cca:  ldloc.s    V_21
      IL_1ccc:  brfalse.s  IL_1cdb

      IL_1cce:  ldloc.s    V_21
      IL_1cd0:  ldloc.s    V_20
      IL_1cd2:  stloc.s    V_23
      IL_1cd4:  stloc.s    V_22
      IL_1cd6:  br         IL_0103

      IL_1cdb:  ldarg.s    arr1
      IL_1cdd:  isinst     float64[]
      IL_1ce2:  stloc.s    V_24
      IL_1ce4:  ldloc.s    V_24
      IL_1ce6:  brfalse.s  IL_1d19

      IL_1ce8:  ldarg.2
      IL_1ce9:  isinst     float64[]
      IL_1cee:  stloc.s    V_25
      IL_1cf0:  ldloc.s    V_25
      IL_1cf2:  brfalse.s  IL_1d01

      IL_1cf4:  ldloc.s    V_25
      IL_1cf6:  ldloc.s    V_24
      IL_1cf8:  stloc.s    V_27
      IL_1cfa:  stloc.s    V_26
      IL_1cfc:  br         IL_012f

      IL_1d01:  ldarg.2
      IL_1d02:  isinst     [mscorlib]System.Array
      IL_1d07:  stloc.s    V_28
      IL_1d09:  ldloc.s    V_28
      IL_1d0b:  brfalse    IL_0156

      IL_1d10:  ldloc.s    V_28
      IL_1d12:  stloc.s    V_29
      IL_1d14:  br         IL_014a

      IL_1d19:  ldarg.2
      IL_1d1a:  isinst     [mscorlib]System.Array
      IL_1d1f:  stloc.s    V_28
      IL_1d21:  ldloc.s    V_28
      IL_1d23:  brfalse    IL_0156

      IL_1d28:  ldloc.s    V_28
      IL_1d2a:  stloc.s    V_29
      IL_1d2c:  br         IL_014a

      IL_1d31:  ldarg.s    arr1
      IL_1d33:  isinst     float64[]
      IL_1d38:  stloc.s    V_24
      IL_1d3a:  ldloc.s    V_24
      IL_1d3c:  brfalse.s  IL_1d6f

      IL_1d3e:  ldarg.2
      IL_1d3f:  isinst     float64[]
      IL_1d44:  stloc.s    V_25
      IL_1d46:  ldloc.s    V_25
      IL_1d48:  brfalse.s  IL_1d57

      IL_1d4a:  ldloc.s    V_25
      IL_1d4c:  ldloc.s    V_24
      IL_1d4e:  stloc.s    V_27
      IL_1d50:  stloc.s    V_26
      IL_1d52:  br         IL_012f

      IL_1d57:  ldarg.2
      IL_1d58:  isinst     [mscorlib]System.Array
      IL_1d5d:  stloc.s    V_28
      IL_1d5f:  ldloc.s    V_28
      IL_1d61:  brfalse    IL_0156

      IL_1d66:  ldloc.s    V_28
      IL_1d68:  stloc.s    V_29
      IL_1d6a:  br         IL_014a

      IL_1d6f:  ldarg.2
      IL_1d70:  isinst     [mscorlib]System.Array
      IL_1d75:  stloc.s    V_28
      IL_1d77:  ldloc.s    V_28
      IL_1d79:  brfalse    IL_0156

      IL_1d7e:  ldloc.s    V_28
      IL_1d80:  stloc.s    V_29
      IL_1d82:  br         IL_014a

      IL_1d87:  ldarg.s    arr1
      IL_1d89:  isinst     char[]
      IL_1d8e:  stloc.s    V_16
      IL_1d90:  ldloc.s    V_16
      IL_1d92:  brfalse    IL_1e82

      IL_1d97:  ldarg.2
      IL_1d98:  isinst     char[]
      IL_1d9d:  stloc.s    V_17
      IL_1d9f:  ldloc.s    V_17
      IL_1da1:  brfalse.s  IL_1db0

      IL_1da3:  ldloc.s    V_17
      IL_1da5:  ldloc.s    V_16
      IL_1da7:  stloc.s    V_19
      IL_1da9:  stloc.s    V_18
      IL_1dab:  br         IL_00d5

      IL_1db0:  ldarg.s    arr1
      IL_1db2:  isinst     float32[]
      IL_1db7:  stloc.s    V_20
      IL_1db9:  ldloc.s    V_20
      IL_1dbb:  brfalse.s  IL_1e2c

      IL_1dbd:  ldarg.2
      IL_1dbe:  isinst     float32[]
      IL_1dc3:  stloc.s    V_21
      IL_1dc5:  ldloc.s    V_21
      IL_1dc7:  brfalse.s  IL_1dd6

      IL_1dc9:  ldloc.s    V_21
      IL_1dcb:  ldloc.s    V_20
      IL_1dcd:  stloc.s    V_23
      IL_1dcf:  stloc.s    V_22
      IL_1dd1:  br         IL_0103

      IL_1dd6:  ldarg.s    arr1
      IL_1dd8:  isinst     float64[]
      IL_1ddd:  stloc.s    V_24
      IL_1ddf:  ldloc.s    V_24
      IL_1de1:  brfalse.s  IL_1e14

      IL_1de3:  ldarg.2
      IL_1de4:  isinst     float64[]
      IL_1de9:  stloc.s    V_25
      IL_1deb:  ldloc.s    V_25
      IL_1ded:  brfalse.s  IL_1dfc

      IL_1def:  ldloc.s    V_25
      IL_1df1:  ldloc.s    V_24
      IL_1df3:  stloc.s    V_27
      IL_1df5:  stloc.s    V_26
      IL_1df7:  br         IL_012f

      IL_1dfc:  ldarg.2
      IL_1dfd:  isinst     [mscorlib]System.Array
      IL_1e02:  stloc.s    V_28
      IL_1e04:  ldloc.s    V_28
      IL_1e06:  brfalse    IL_0156

      IL_1e0b:  ldloc.s    V_28
      IL_1e0d:  stloc.s    V_29
      IL_1e0f:  br         IL_014a

      IL_1e14:  ldarg.2
      IL_1e15:  isinst     [mscorlib]System.Array
      IL_1e1a:  stloc.s    V_28
      IL_1e1c:  ldloc.s    V_28
      IL_1e1e:  brfalse    IL_0156

      IL_1e23:  ldloc.s    V_28
      IL_1e25:  stloc.s    V_29
      IL_1e27:  br         IL_014a

      IL_1e2c:  ldarg.s    arr1
      IL_1e2e:  isinst     float64[]
      IL_1e33:  stloc.s    V_24
      IL_1e35:  ldloc.s    V_24
      IL_1e37:  brfalse.s  IL_1e6a

      IL_1e39:  ldarg.2
      IL_1e3a:  isinst     float64[]
      IL_1e3f:  stloc.s    V_25
      IL_1e41:  ldloc.s    V_25
      IL_1e43:  brfalse.s  IL_1e52

      IL_1e45:  ldloc.s    V_25
      IL_1e47:  ldloc.s    V_24
      IL_1e49:  stloc.s    V_27
      IL_1e4b:  stloc.s    V_26
      IL_1e4d:  br         IL_012f

      IL_1e52:  ldarg.2
      IL_1e53:  isinst     [mscorlib]System.Array
      IL_1e58:  stloc.s    V_28
      IL_1e5a:  ldloc.s    V_28
      IL_1e5c:  brfalse    IL_0156

      IL_1e61:  ldloc.s    V_28
      IL_1e63:  stloc.s    V_29
      IL_1e65:  br         IL_014a

      IL_1e6a:  ldarg.2
      IL_1e6b:  isinst     [mscorlib]System.Array
      IL_1e70:  stloc.s    V_28
      IL_1e72:  ldloc.s    V_28
      IL_1e74:  brfalse    IL_0156

      IL_1e79:  ldloc.s    V_28
      IL_1e7b:  stloc.s    V_29
      IL_1e7d:  br         IL_014a

      IL_1e82:  ldarg.s    arr1
      IL_1e84:  isinst     float32[]
      IL_1e89:  stloc.s    V_20
      IL_1e8b:  ldloc.s    V_20
      IL_1e8d:  brfalse.s  IL_1efe

      IL_1e8f:  ldarg.2
      IL_1e90:  isinst     float32[]
      IL_1e95:  stloc.s    V_21
      IL_1e97:  ldloc.s    V_21
      IL_1e99:  brfalse.s  IL_1ea8

      IL_1e9b:  ldloc.s    V_21
      IL_1e9d:  ldloc.s    V_20
      IL_1e9f:  stloc.s    V_23
      IL_1ea1:  stloc.s    V_22
      IL_1ea3:  br         IL_0103

      IL_1ea8:  ldarg.s    arr1
      IL_1eaa:  isinst     float64[]
      IL_1eaf:  stloc.s    V_24
      IL_1eb1:  ldloc.s    V_24
      IL_1eb3:  brfalse.s  IL_1ee6

      IL_1eb5:  ldarg.2
      IL_1eb6:  isinst     float64[]
      IL_1ebb:  stloc.s    V_25
      IL_1ebd:  ldloc.s    V_25
      IL_1ebf:  brfalse.s  IL_1ece

      IL_1ec1:  ldloc.s    V_25
      IL_1ec3:  ldloc.s    V_24
      IL_1ec5:  stloc.s    V_27
      IL_1ec7:  stloc.s    V_26
      IL_1ec9:  br         IL_012f

      IL_1ece:  ldarg.2
      IL_1ecf:  isinst     [mscorlib]System.Array
      IL_1ed4:  stloc.s    V_28
      IL_1ed6:  ldloc.s    V_28
      IL_1ed8:  brfalse    IL_0156

      IL_1edd:  ldloc.s    V_28
      IL_1edf:  stloc.s    V_29
      IL_1ee1:  br         IL_014a

      IL_1ee6:  ldarg.2
      IL_1ee7:  isinst     [mscorlib]System.Array
      IL_1eec:  stloc.s    V_28
      IL_1eee:  ldloc.s    V_28
      IL_1ef0:  brfalse    IL_0156

      IL_1ef5:  ldloc.s    V_28
      IL_1ef7:  stloc.s    V_29
      IL_1ef9:  br         IL_014a

      IL_1efe:  ldarg.s    arr1
      IL_1f00:  isinst     float64[]
      IL_1f05:  stloc.s    V_24
      IL_1f07:  ldloc.s    V_24
      IL_1f09:  brfalse.s  IL_1f3c

      IL_1f0b:  ldarg.2
      IL_1f0c:  isinst     float64[]
      IL_1f11:  stloc.s    V_25
      IL_1f13:  ldloc.s    V_25
      IL_1f15:  brfalse.s  IL_1f24

      IL_1f17:  ldloc.s    V_25
      IL_1f19:  ldloc.s    V_24
      IL_1f1b:  stloc.s    V_27
      IL_1f1d:  stloc.s    V_26
      IL_1f1f:  br         IL_012f

      IL_1f24:  ldarg.2
      IL_1f25:  isinst     [mscorlib]System.Array
      IL_1f2a:  stloc.s    V_28
      IL_1f2c:  ldloc.s    V_28
      IL_1f2e:  brfalse    IL_0156

      IL_1f33:  ldloc.s    V_28
      IL_1f35:  stloc.s    V_29
      IL_1f37:  br         IL_014a

      IL_1f3c:  ldarg.2
      IL_1f3d:  isinst     [mscorlib]System.Array
      IL_1f42:  stloc.s    V_28
      IL_1f44:  ldloc.s    V_28
      IL_1f46:  brfalse    IL_0156

      IL_1f4b:  ldloc.s    V_28
      IL_1f4d:  stloc.s    V_29
      IL_1f4f:  br         IL_014a
    } // end of method HashCompare::GenericEqualityObj$cont@1498

    .method assembly static bool  GenericEqualityObjArray(bool er,
                                                          class [mscorlib]System.Collections.IEqualityComparer iec,
                                                          object[] x,
                                                          object[] y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 04 00 00 00 01 00 00 00 01 00 00 00 01 00
                                                                                                         00 00 01 00 00 00 00 00 )
      // Code size       73 (0x49)
      .maxstack  5
      .locals init (int32 V_0,
               int32 V_1,
               bool V_2,
               int32 V_3,
               bool V_4,
               bool V_5)
      IL_0000:  nop
      IL_0001:  ldarg.2
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldarg.3
      IL_0006:  ldlen
      IL_0007:  conv.i4
      IL_0008:  stloc.1
      IL_0009:  ldloc.0
      IL_000a:  ldloc.1
      IL_000b:  ceq
      IL_000d:  stloc.2
      IL_000e:  ldloc.2
      IL_000f:  brtrue.s   IL_0013

      IL_0011:  ldloc.2
      IL_0012:  ret

      IL_0013:  ldc.i4.0
      IL_0014:  stloc.3
      IL_0015:  ldc.i4.1
      IL_0016:  stloc.s    V_4
      IL_0018:  ldloc.3
      IL_0019:  ldloc.0
      IL_001a:  bge.s      IL_0046

      IL_001c:  ldarg.0
      IL_001d:  ldarg.1
      IL_001e:  ldarg.2
      IL_001f:  ldloc.3
      IL_0020:  ldelem     [mscorlib]System.Object
      IL_0025:  ldarg.3
      IL_0026:  ldloc.3
      IL_0027:  ldelem     [mscorlib]System.Object
      IL_002c:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityObj(bool,
                                                                                                         class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                         object,
                                                                                                         object)
      IL_0031:  stloc.s    V_5
      IL_0033:  ldloc.s    V_5
      IL_0035:  brtrue.s   IL_003f

      IL_0037:  ldc.i4.0
      IL_0038:  stloc.s    V_4
      IL_003a:  ldloc.0
      IL_003b:  stloc.3
      IL_003c:  nop
      IL_003d:  br.s       IL_0018

      IL_003f:  ldloc.3
      IL_0040:  ldc.i4.1
      IL_0041:  add
      IL_0042:  stloc.3
      IL_0043:  nop
      IL_0044:  br.s       IL_0018

      IL_0046:  ldloc.s    V_4
      IL_0048:  ret
    } // end of method HashCompare::GenericEqualityObjArray

    .method assembly static bool  GenericEqualityArbArray(bool er,
                                                          class [mscorlib]System.Collections.IEqualityComparer iec,
                                                          class [mscorlib]System.Array x,
                                                          class [mscorlib]System.Array y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 04 00 00 00 01 00 00 00 01 00 00 00 01 00
                                                                                                         00 00 01 00 00 00 00 00 )
      // Code size       407 (0x197)
      .maxstack  11
      .locals init (int64 V_0,
               int64 V_1,
               int64 V_2,
               int64 V_3,
               bool V_4,
               class [mscorlib]System.Collections.IEqualityComparer V_5,
               class [mscorlib]System.Array V_6,
               class [mscorlib]System.Array V_7,
               int64 V_8,
               int64 V_9,
               int64 V_10,
               int64 V_11,
               int64 V_12,
               int64 V_13,
               int64 V_14,
               int64 V_15,
               int64 V_16,
               int64 V_17,
               int32 V_18,
               int32 V_19,
               int64[] V_20,
               class [mscorlib]System.Array V_21,
               class [mscorlib]System.Array V_22,
               int32 V_23,
               int64[] V_24)
      IL_0000:  nop
      IL_0001:  ldarg.2
      IL_0002:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_0007:  ldc.i4.1
      IL_0008:  bne.un.s   IL_0016

      IL_000a:  ldarg.3
      IL_000b:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_0010:  ldc.i4.1
      IL_0011:  ceq
      IL_0013:  nop
      IL_0014:  br.s       IL_0018

      IL_0016:  ldc.i4.0
      IL_0017:  nop
      IL_0018:  brfalse.s  IL_0071

      IL_001a:  ldarg.2
      IL_001b:  callvirt   instance int64 [mscorlib]System.Array::get_LongLength()
      IL_0020:  stloc.0
      IL_0021:  ldarg.3
      IL_0022:  callvirt   instance int64 [mscorlib]System.Array::get_LongLength()
      IL_0027:  stloc.1
      IL_0028:  ldloc.0
      IL_0029:  ldloc.1
      IL_002a:  bne.un.s   IL_006f

      IL_002c:  ldarg.2
      IL_002d:  ldc.i4.0
      IL_002e:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_0033:  conv.i8
      IL_0034:  stloc.2
      IL_0035:  ldarg.3
      IL_0036:  ldc.i4.0
      IL_0037:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_003c:  conv.i8
      IL_003d:  stloc.3
      IL_003e:  ldloc.2
      IL_003f:  ldloc.3
      IL_0040:  bne.un.s   IL_006d

      IL_0042:  ldarg.0
      IL_0043:  stloc.s    V_4
      IL_0045:  ldarg.1
      IL_0046:  stloc.s    V_5
      IL_0048:  ldarg.2
      IL_0049:  stloc.s    V_6
      IL_004b:  ldarg.3
      IL_004c:  stloc.s    V_7
      IL_004e:  ldloc.0
      IL_004f:  stloc.s    V_8
      IL_0051:  ldloc.2
      IL_0052:  stloc.s    V_9
      IL_0054:  ldloc.3
      IL_0055:  stloc.s    V_10
      IL_0057:  ldarg.0
      IL_0058:  ldarg.1
      IL_0059:  ldarg.2
      IL_005a:  ldarg.3
      IL_005b:  ldloc.0
      IL_005c:  ldloc.2
      IL_005d:  ldloc.3
      IL_005e:  ldc.i8     0x0
      IL_0067:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::'check@1588-2'(bool,
                                                                                                     class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                     class [mscorlib]System.Array,
                                                                                                     class [mscorlib]System.Array,
                                                                                                     int64,
                                                                                                     int64,
                                                                                                     int64,
                                                                                                     int64)
      IL_006c:  ret

      IL_006d:  ldc.i4.0
      IL_006e:  ret

      IL_006f:  ldc.i4.0
      IL_0070:  ret

      IL_0071:  ldarg.2
      IL_0072:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_0077:  ldc.i4.2
      IL_0078:  bne.un.s   IL_0086

      IL_007a:  ldarg.3
      IL_007b:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_0080:  ldc.i4.2
      IL_0081:  ceq
      IL_0083:  nop
      IL_0084:  br.s       IL_0088

      IL_0086:  ldc.i4.0
      IL_0087:  nop
      IL_0088:  brfalse    IL_013b

      IL_008d:  ldarg.2
      IL_008e:  ldc.i4.0
      IL_008f:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_0094:  stloc.0
      IL_0095:  ldarg.3
      IL_0096:  ldc.i4.0
      IL_0097:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_009c:  stloc.1
      IL_009d:  ldloc.0
      IL_009e:  ldloc.1
      IL_009f:  bne.un     IL_0139

      IL_00a4:  ldarg.2
      IL_00a5:  ldc.i4.1
      IL_00a6:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_00ab:  stloc.2
      IL_00ac:  ldarg.3
      IL_00ad:  ldc.i4.1
      IL_00ae:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_00b3:  stloc.3
      IL_00b4:  ldloc.2
      IL_00b5:  ldloc.3
      IL_00b6:  bne.un     IL_0137

      IL_00bb:  ldarg.2
      IL_00bc:  ldc.i4.0
      IL_00bd:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_00c2:  conv.i8
      IL_00c3:  stloc.s    V_8
      IL_00c5:  ldarg.2
      IL_00c6:  ldc.i4.1
      IL_00c7:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_00cc:  conv.i8
      IL_00cd:  stloc.s    V_9
      IL_00cf:  ldarg.3
      IL_00d0:  ldc.i4.0
      IL_00d1:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_00d6:  conv.i8
      IL_00d7:  stloc.s    V_10
      IL_00d9:  ldarg.3
      IL_00da:  ldc.i4.1
      IL_00db:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_00e0:  conv.i8
      IL_00e1:  stloc.s    V_11
      IL_00e3:  ldloc.s    V_8
      IL_00e5:  ldloc.s    V_10
      IL_00e7:  bne.un.s   IL_00f2

      IL_00e9:  ldloc.s    V_9
      IL_00eb:  ldloc.s    V_11
      IL_00ed:  ceq
      IL_00ef:  nop
      IL_00f0:  br.s       IL_00f4

      IL_00f2:  ldc.i4.0
      IL_00f3:  nop
      IL_00f4:  brfalse.s  IL_0135

      IL_00f6:  ldarg.0
      IL_00f7:  stloc.s    V_4
      IL_00f9:  ldarg.1
      IL_00fa:  stloc.s    V_5
      IL_00fc:  ldarg.2
      IL_00fd:  stloc.s    V_6
      IL_00ff:  ldarg.3
      IL_0100:  stloc.s    V_7
      IL_0102:  ldloc.0
      IL_0103:  stloc.s    V_12
      IL_0105:  ldloc.2
      IL_0106:  stloc.s    V_13
      IL_0108:  ldloc.s    V_8
      IL_010a:  stloc.s    V_14
      IL_010c:  ldloc.s    V_9
      IL_010e:  stloc.s    V_15
      IL_0110:  ldloc.s    V_10
      IL_0112:  stloc.s    V_16
      IL_0114:  ldloc.s    V_11
      IL_0116:  stloc.s    V_17
      IL_0118:  ldarg.0
      IL_0119:  ldarg.1
      IL_011a:  ldarg.2
      IL_011b:  ldarg.3
      IL_011c:  ldloc.0
      IL_011d:  ldloc.2
      IL_011e:  ldloc.s    V_8
      IL_0120:  ldloc.s    V_9
      IL_0122:  ldloc.s    V_10
      IL_0124:  ldloc.s    V_11
      IL_0126:  ldc.i8     0x0
      IL_012f:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::'check0@1605-1'(bool,
                                                                                                      class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                      class [mscorlib]System.Array,
                                                                                                      class [mscorlib]System.Array,
                                                                                                      int64,
                                                                                                      int64,
                                                                                                      int64,
                                                                                                      int64,
                                                                                                      int64,
                                                                                                      int64,
                                                                                                      int64)
      IL_0134:  ret

      IL_0135:  ldc.i4.0
      IL_0136:  ret

      IL_0137:  ldc.i4.0
      IL_0138:  ret

      IL_0139:  ldc.i4.0
      IL_013a:  ret

      IL_013b:  ldarg.2
      IL_013c:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_0141:  ldarg.3
      IL_0142:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_0147:  bne.un.s   IL_0195

      IL_0149:  ldarg.2
      IL_014a:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_014f:  stloc.s    V_18
      IL_0151:  ldarg.2
      IL_0152:  stloc.s    V_6
      IL_0154:  ldarg.3
      IL_0155:  stloc.s    V_7
      IL_0157:  ldloc.s    V_18
      IL_0159:  stloc.s    V_19
      IL_015b:  ldarg.2
      IL_015c:  ldarg.3
      IL_015d:  ldloc.s    V_18
      IL_015f:  ldc.i4.0
      IL_0160:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::'precheck@1613-1'(class [mscorlib]System.Array,
                                                                                                        class [mscorlib]System.Array,
                                                                                                        int32,
                                                                                                        int32)
      IL_0165:  brfalse.s  IL_0193

      IL_0167:  ldloc.s    V_18
      IL_0169:  newarr     [mscorlib]System.Int64
      IL_016e:  stloc.s    V_20
      IL_0170:  ldarg.0
      IL_0171:  stloc.s    V_4
      IL_0173:  ldarg.1
      IL_0174:  stloc.s    V_5
      IL_0176:  ldarg.2
      IL_0177:  stloc.s    V_21
      IL_0179:  ldarg.3
      IL_017a:  stloc.s    V_22
      IL_017c:  ldloc.s    V_18
      IL_017e:  stloc.s    V_23
      IL_0180:  ldloc.s    V_20
      IL_0182:  stloc.s    V_24
      IL_0184:  ldarg.0
      IL_0185:  ldarg.1
      IL_0186:  ldarg.2
      IL_0187:  ldarg.3
      IL_0188:  ldloc.s    V_18
      IL_018a:  ldloc.s    V_20
      IL_018c:  ldc.i4.0
      IL_018d:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::'check@1628-3'(bool,
                                                                                                     class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                     class [mscorlib]System.Array,
                                                                                                     class [mscorlib]System.Array,
                                                                                                     int32,
                                                                                                     int64[],
                                                                                                     int32)
      IL_0192:  ret

      IL_0193:  ldc.i4.0
      IL_0194:  ret

      IL_0195:  ldc.i4.0
      IL_0196:  ret
    } // end of method HashCompare::GenericEqualityArbArray

    .method assembly static bool  GenericEqualityObj(bool er,
                                                     class [mscorlib]System.Collections.IEqualityComparer iec,
                                                     object xobj,
                                                     object yobj) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 03 00 00 00 01 00 00 00 01 00 00 00 02 00
                                                                                                         00 00 00 00 )
      // Code size       494 (0x1ee)
      .maxstack  6
      .locals init (string V_0,
               string V_1,
               string V_2,
               string V_3,
               class [mscorlib]System.Array V_4,
               class [mscorlib]System.Array V_5,
               class [mscorlib]System.Collections.IStructuralEquatable V_6,
               class [mscorlib]System.Collections.IStructuralEquatable V_7,
               float32 V_8,
               float32 V_9,
               float64 V_10,
               float64 V_11)
      IL_0000:  ldarg.2
      IL_0001:  brfalse    IL_01e7

      IL_0006:  ldarg.3
      IL_0007:  brfalse    IL_01e5

      IL_000c:  ldarg.2
      IL_000d:  isinst     [mscorlib]System.String
      IL_0012:  stloc.0
      IL_0013:  ldloc.0
      IL_0014:  brfalse    IL_0138

      IL_0019:  ldarg.3
      IL_001a:  isinst     [mscorlib]System.String
      IL_001f:  stloc.1
      IL_0020:  ldloc.1
      IL_0021:  brfalse.s  IL_0030

      IL_0023:  ldloc.1
      IL_0024:  stloc.2
      IL_0025:  ldloc.0
      IL_0026:  stloc.3
      IL_0027:  nop
      IL_0028:  ldloc.3
      IL_0029:  ldloc.2
      IL_002a:  call       bool [mscorlib]System.String::Equals(string,
                                                                string)
      IL_002f:  ret

      IL_0030:  ldarg.2
      IL_0031:  isinst     [mscorlib]System.Array
      IL_0036:  stloc.s    V_4
      IL_0038:  ldloc.s    V_4
      IL_003a:  brfalse.s  IL_004f

      IL_003c:  ldloc.s    V_4
      IL_003e:  stloc.s    V_5
      IL_0040:  ldarg.0
      IL_0041:  ldarg.1
      IL_0042:  ldarg.3
      IL_0043:  ldarg.2
      IL_0044:  ldloc.s    V_5
      IL_0046:  ldnull
      IL_0047:  tail.
      IL_0049:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityObj$cont@1498(bool,
                                                                                                                   class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                                   object,
                                                                                                                   object,
                                                                                                                   class [mscorlib]System.Array,
                                                                                                                   class Microsoft.FSharp.Core.Unit)
      IL_004e:  ret

      IL_004f:  ldarg.2
      IL_0050:  isinst     [mscorlib]System.Collections.IStructuralEquatable
      IL_0055:  stloc.s    V_6
      IL_0057:  ldloc.s    V_6
      IL_0059:  brfalse.s  IL_006b

      IL_005b:  ldloc.s    V_6
      IL_005d:  stloc.s    V_7
      IL_005f:  ldloc.s    V_7
      IL_0061:  ldarg.3
      IL_0062:  ldarg.1
      IL_0063:  tail.
      IL_0065:  callvirt   instance bool [mscorlib]System.Collections.IStructuralEquatable::Equals(object,
                                                                                                   class [mscorlib]System.Collections.IEqualityComparer)
      IL_006a:  ret

      IL_006b:  ldarg.2
      IL_006c:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float64>(object)
      IL_0071:  brtrue.s   IL_00cb

      IL_0073:  ldarg.2
      IL_0074:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float32>(object)
      IL_0079:  brtrue.s   IL_0085

      IL_007b:  ldarg.2
      IL_007c:  ldarg.3
      IL_007d:  tail.
      IL_007f:  callvirt   instance bool [mscorlib]System.Object::Equals(object)
      IL_0084:  ret

      IL_0085:  ldarg.3
      IL_0086:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float32>(object)
      IL_008b:  brtrue.s   IL_008f

      IL_008d:  br.s       IL_007b

      IL_008f:  ldarg.3
      IL_0090:  unbox.any  [mscorlib]System.Single
      IL_0095:  stloc.s    V_8
      IL_0097:  ldarg.2
      IL_0098:  unbox.any  [mscorlib]System.Single
      IL_009d:  stloc.s    V_9
      IL_009f:  ldarg.0
      IL_00a0:  brfalse.s  IL_00ae

      IL_00a2:  ldloc.s    V_9
      IL_00a4:  ldloc.s    V_9
      IL_00a6:  ceq
      IL_00a8:  ldc.i4.0
      IL_00a9:  ceq
      IL_00ab:  nop
      IL_00ac:  br.s       IL_00b0

      IL_00ae:  ldc.i4.0
      IL_00af:  nop
      IL_00b0:  brfalse.s  IL_00be

      IL_00b2:  ldloc.s    V_8
      IL_00b4:  ldloc.s    V_8
      IL_00b6:  ceq
      IL_00b8:  ldc.i4.0
      IL_00b9:  ceq
      IL_00bb:  nop
      IL_00bc:  br.s       IL_00c0

      IL_00be:  ldc.i4.0
      IL_00bf:  nop
      IL_00c0:  brfalse.s  IL_00c4

      IL_00c2:  ldc.i4.1
      IL_00c3:  ret

      IL_00c4:  ldloc.s    V_9
      IL_00c6:  ldloc.s    V_8
      IL_00c8:  ceq
      IL_00ca:  ret

      IL_00cb:  ldarg.3
      IL_00cc:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float64>(object)
      IL_00d1:  brtrue.s   IL_00fc

      IL_00d3:  ldarg.2
      IL_00d4:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float32>(object)
      IL_00d9:  brtrue.s   IL_00dd

      IL_00db:  br.s       IL_007b

      IL_00dd:  ldarg.3
      IL_00de:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float32>(object)
      IL_00e3:  brtrue.s   IL_00ea

      IL_00e5:  br         IL_007b

      IL_00ea:  ldarg.3
      IL_00eb:  unbox.any  [mscorlib]System.Single
      IL_00f0:  ldarg.2
      IL_00f1:  unbox.any  [mscorlib]System.Single
      IL_00f6:  stloc.s    V_9
      IL_00f8:  stloc.s    V_8
      IL_00fa:  br.s       IL_009f

      IL_00fc:  ldarg.3
      IL_00fd:  unbox.any  [mscorlib]System.Double
      IL_0102:  stloc.s    V_10
      IL_0104:  ldarg.2
      IL_0105:  unbox.any  [mscorlib]System.Double
      IL_010a:  stloc.s    V_11
      IL_010c:  ldarg.0
      IL_010d:  brfalse.s  IL_011b

      IL_010f:  ldloc.s    V_11
      IL_0111:  ldloc.s    V_11
      IL_0113:  ceq
      IL_0115:  ldc.i4.0
      IL_0116:  ceq
      IL_0118:  nop
      IL_0119:  br.s       IL_011d

      IL_011b:  ldc.i4.0
      IL_011c:  nop
      IL_011d:  brfalse.s  IL_012b

      IL_011f:  ldloc.s    V_10
      IL_0121:  ldloc.s    V_10
      IL_0123:  ceq
      IL_0125:  ldc.i4.0
      IL_0126:  ceq
      IL_0128:  nop
      IL_0129:  br.s       IL_012d

      IL_012b:  ldc.i4.0
      IL_012c:  nop
      IL_012d:  brfalse.s  IL_0131

      IL_012f:  ldc.i4.1
      IL_0130:  ret

      IL_0131:  ldloc.s    V_11
      IL_0133:  ldloc.s    V_10
      IL_0135:  ceq
      IL_0137:  ret

      IL_0138:  ldarg.2
      IL_0139:  isinst     [mscorlib]System.Array
      IL_013e:  stloc.s    V_4
      IL_0140:  ldloc.s    V_4
      IL_0142:  brfalse.s  IL_014d

      IL_0144:  ldloc.s    V_4
      IL_0146:  stloc.s    V_5
      IL_0148:  br         IL_0040

      IL_014d:  ldarg.2
      IL_014e:  isinst     [mscorlib]System.Collections.IStructuralEquatable
      IL_0153:  stloc.s    V_6
      IL_0155:  ldloc.s    V_6
      IL_0157:  brfalse.s  IL_0162

      IL_0159:  ldloc.s    V_6
      IL_015b:  stloc.s    V_7
      IL_015d:  br         IL_005f

      IL_0162:  ldarg.2
      IL_0163:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float64>(object)
      IL_0168:  brtrue.s   IL_0199

      IL_016a:  ldarg.2
      IL_016b:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float32>(object)
      IL_0170:  brtrue.s   IL_0177

      IL_0172:  br         IL_007b

      IL_0177:  ldarg.3
      IL_0178:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float32>(object)
      IL_017d:  brtrue.s   IL_0184

      IL_017f:  br         IL_007b

      IL_0184:  ldarg.3
      IL_0185:  unbox.any  [mscorlib]System.Single
      IL_018a:  ldarg.2
      IL_018b:  unbox.any  [mscorlib]System.Single
      IL_0190:  stloc.s    V_9
      IL_0192:  stloc.s    V_8
      IL_0194:  br         IL_009f

      IL_0199:  ldarg.3
      IL_019a:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float64>(object)
      IL_019f:  brtrue.s   IL_01d0

      IL_01a1:  ldarg.2
      IL_01a2:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float32>(object)
      IL_01a7:  brtrue.s   IL_01ae

      IL_01a9:  br         IL_007b

      IL_01ae:  ldarg.3
      IL_01af:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<float32>(object)
      IL_01b4:  brtrue.s   IL_01bb

      IL_01b6:  br         IL_007b

      IL_01bb:  ldarg.3
      IL_01bc:  unbox.any  [mscorlib]System.Single
      IL_01c1:  ldarg.2
      IL_01c2:  unbox.any  [mscorlib]System.Single
      IL_01c7:  stloc.s    V_9
      IL_01c9:  stloc.s    V_8
      IL_01cb:  br         IL_009f

      IL_01d0:  ldarg.3
      IL_01d1:  unbox.any  [mscorlib]System.Double
      IL_01d6:  ldarg.2
      IL_01d7:  unbox.any  [mscorlib]System.Double
      IL_01dc:  stloc.s    V_11
      IL_01de:  stloc.s    V_10
      IL_01e0:  br         IL_010c

      IL_01e5:  ldc.i4.0
      IL_01e6:  ret

      IL_01e7:  ldarg.3
      IL_01e8:  brfalse.s  IL_01ec

      IL_01ea:  ldc.i4.0
      IL_01eb:  ret

      IL_01ec:  ldc.i4.1
      IL_01ed:  ret
    } // end of method HashCompare::GenericEqualityObj

    .method assembly static bool  'check@1588-2'(bool er,
                                                 class [mscorlib]System.Collections.IEqualityComparer iec,
                                                 class [mscorlib]System.Array x,
                                                 class [mscorlib]System.Array y,
                                                 int64 lenx,
                                                 int64 basex,
                                                 int64 basey,
                                                 int64 i) cil managed
    {
      // Code size       82 (0x52)
      .maxstack  9
      IL_0000:  nop
      IL_0001:  ldarg.s    i
      IL_0003:  ldarg.s    lenx
      IL_0005:  blt.s      IL_0009

      IL_0007:  ldc.i4.1
      IL_0008:  ret

      IL_0009:  ldarg.0
      IL_000a:  ldarg.1
      IL_000b:  ldarg.2
      IL_000c:  ldarg.s    basex
      IL_000e:  ldarg.s    i
      IL_0010:  add
      IL_0011:  callvirt   instance object [mscorlib]System.Array::GetValue(int64)
      IL_0016:  ldarg.3
      IL_0017:  ldarg.s    basey
      IL_0019:  ldarg.s    i
      IL_001b:  add
      IL_001c:  callvirt   instance object [mscorlib]System.Array::GetValue(int64)
      IL_0021:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityObj(bool,
                                                                                                         class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                         object,
                                                                                                         object)
      IL_0026:  brfalse.s  IL_0050

      IL_0028:  ldarg.0
      IL_0029:  ldarg.1
      IL_002a:  ldarg.2
      IL_002b:  ldarg.3
      IL_002c:  ldarg.s    lenx
      IL_002e:  ldarg.s    basex
      IL_0030:  ldarg.s    basey
      IL_0032:  ldarg.s    i
      IL_0034:  ldc.i8     0x1
      IL_003d:  add
      IL_003e:  starg.s    i
      IL_0040:  starg.s    basey
      IL_0042:  starg.s    basex
      IL_0044:  starg.s    lenx
      IL_0046:  starg.s    y
      IL_0048:  starg.s    x
      IL_004a:  starg.s    iec
      IL_004c:  starg.s    er
      IL_004e:  br.s       IL_0001

      IL_0050:  ldc.i4.0
      IL_0051:  ret
    } // end of method HashCompare::'check@1588-2'

    .method assembly static bool  'check1@1606-1'(bool er,
                                                  class [mscorlib]System.Collections.IEqualityComparer iec,
                                                  class [mscorlib]System.Array x,
                                                  class [mscorlib]System.Array y,
                                                  int64 lenx1,
                                                  int64 basex0,
                                                  int64 basex1,
                                                  int64 basey0,
                                                  int64 basey1,
                                                  int64 i,
                                                  int64 j) cil managed
    {
      // Code size       104 (0x68)
      .maxstack  12
      IL_0000:  nop
      IL_0001:  ldarg.s    j
      IL_0003:  ldarg.s    lenx1
      IL_0005:  blt.s      IL_0009

      IL_0007:  ldc.i4.1
      IL_0008:  ret

      IL_0009:  ldarg.0
      IL_000a:  ldarg.1
      IL_000b:  ldarg.2
      IL_000c:  ldarg.s    basex0
      IL_000e:  ldarg.s    i
      IL_0010:  add
      IL_0011:  ldarg.s    basex1
      IL_0013:  ldarg.s    j
      IL_0015:  add
      IL_0016:  callvirt   instance object [mscorlib]System.Array::GetValue(int64,
                                                                            int64)
      IL_001b:  ldarg.3
      IL_001c:  ldarg.s    basey0
      IL_001e:  ldarg.s    i
      IL_0020:  add
      IL_0021:  ldarg.s    basey1
      IL_0023:  ldarg.s    j
      IL_0025:  add
      IL_0026:  callvirt   instance object [mscorlib]System.Array::GetValue(int64,
                                                                            int64)
      IL_002b:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityObj(bool,
                                                                                                         class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                         object,
                                                                                                         object)
      IL_0030:  brfalse.s  IL_0066

      IL_0032:  ldarg.0
      IL_0033:  ldarg.1
      IL_0034:  ldarg.2
      IL_0035:  ldarg.3
      IL_0036:  ldarg.s    lenx1
      IL_0038:  ldarg.s    basex0
      IL_003a:  ldarg.s    basex1
      IL_003c:  ldarg.s    basey0
      IL_003e:  ldarg.s    basey1
      IL_0040:  ldarg.s    i
      IL_0042:  ldarg.s    j
      IL_0044:  ldc.i8     0x1
      IL_004d:  add
      IL_004e:  starg.s    j
      IL_0050:  starg.s    i
      IL_0052:  starg.s    basey1
      IL_0054:  starg.s    basey0
      IL_0056:  starg.s    basex1
      IL_0058:  starg.s    basex0
      IL_005a:  starg.s    lenx1
      IL_005c:  starg.s    y
      IL_005e:  starg.s    x
      IL_0060:  starg.s    iec
      IL_0062:  starg.s    er
      IL_0064:  br.s       IL_0001

      IL_0066:  ldc.i4.0
      IL_0067:  ret
    } // end of method HashCompare::'check1@1606-1'

    .method assembly static bool  'check0@1605-1'(bool er,
                                                  class [mscorlib]System.Collections.IEqualityComparer iec,
                                                  class [mscorlib]System.Array x,
                                                  class [mscorlib]System.Array y,
                                                  int64 lenx0,
                                                  int64 lenx1,
                                                  int64 basex0,
                                                  int64 basex1,
                                                  int64 basey0,
                                                  int64 basey1,
                                                  int64 i) cil managed
    {
      // Code size       130 (0x82)
      .maxstack  12
      .locals init (bool V_0,
               class [mscorlib]System.Collections.IEqualityComparer V_1,
               class [mscorlib]System.Array V_2,
               class [mscorlib]System.Array V_3,
               int64 V_4,
               int64 V_5,
               int64 V_6,
               int64 V_7,
               int64 V_8,
               int64 V_9)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  stloc.0
      IL_0003:  ldarg.1
      IL_0004:  stloc.1
      IL_0005:  ldarg.2
      IL_0006:  stloc.2
      IL_0007:  ldarg.3
      IL_0008:  stloc.3
      IL_0009:  ldarg.s    lenx1
      IL_000b:  stloc.s    V_4
      IL_000d:  ldarg.s    basex0
      IL_000f:  stloc.s    V_5
      IL_0011:  ldarg.s    basex1
      IL_0013:  stloc.s    V_6
      IL_0015:  ldarg.s    basey0
      IL_0017:  stloc.s    V_7
      IL_0019:  ldarg.s    basey1
      IL_001b:  stloc.s    V_8
      IL_001d:  ldarg.s    i
      IL_001f:  stloc.s    V_9
      IL_0021:  ldarg.s    i
      IL_0023:  ldarg.s    lenx0
      IL_0025:  blt.s      IL_0029

      IL_0027:  ldc.i4.1
      IL_0028:  ret

      IL_0029:  ldarg.0
      IL_002a:  ldarg.1
      IL_002b:  ldarg.2
      IL_002c:  ldarg.3
      IL_002d:  ldarg.s    lenx1
      IL_002f:  ldarg.s    basex0
      IL_0031:  ldarg.s    basex1
      IL_0033:  ldarg.s    basey0
      IL_0035:  ldarg.s    basey1
      IL_0037:  ldarg.s    i
      IL_0039:  ldc.i8     0x0
      IL_0042:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::'check1@1606-1'(bool,
                                                                                                      class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                      class [mscorlib]System.Array,
                                                                                                      class [mscorlib]System.Array,
                                                                                                      int64,
                                                                                                      int64,
                                                                                                      int64,
                                                                                                      int64,
                                                                                                      int64,
                                                                                                      int64,
                                                                                                      int64)
      IL_0047:  brfalse.s  IL_0080

      IL_0049:  ldarg.0
      IL_004a:  ldarg.1
      IL_004b:  ldarg.2
      IL_004c:  ldarg.3
      IL_004d:  ldarg.s    lenx0
      IL_004f:  ldarg.s    lenx1
      IL_0051:  ldarg.s    basex0
      IL_0053:  ldarg.s    basex1
      IL_0055:  ldarg.s    basey0
      IL_0057:  ldarg.s    basey1
      IL_0059:  ldarg.s    i
      IL_005b:  ldc.i8     0x1
      IL_0064:  add
      IL_0065:  starg.s    i
      IL_0067:  starg.s    basey1
      IL_0069:  starg.s    basey0
      IL_006b:  starg.s    basex1
      IL_006d:  starg.s    basex0
      IL_006f:  starg.s    lenx1
      IL_0071:  starg.s    lenx0
      IL_0073:  starg.s    y
      IL_0075:  starg.s    x
      IL_0077:  starg.s    iec
      IL_0079:  starg.s    er
      IL_007b:  br         IL_0001

      IL_0080:  ldc.i4.0
      IL_0081:  ret
    } // end of method HashCompare::'check0@1605-1'

    .method assembly static bool  'precheck@1613-1'(class [mscorlib]System.Array x,
                                                    class [mscorlib]System.Array y,
                                                    int32 ndims,
                                                    int32 k) cil managed
    {
      // Code size       64 (0x40)
      .maxstack  5
      IL_0000:  nop
      IL_0001:  ldarg.3
      IL_0002:  ldarg.2
      IL_0003:  blt.s      IL_0007

      IL_0005:  ldc.i4.1
      IL_0006:  ret

      IL_0007:  ldarg.0
      IL_0008:  ldarg.3
      IL_0009:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_000e:  ldarg.1
      IL_000f:  ldarg.3
      IL_0010:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_0015:  bne.un.s   IL_002a

      IL_0017:  ldarg.0
      IL_0018:  ldarg.3
      IL_0019:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_001e:  ldarg.1
      IL_001f:  ldarg.3
      IL_0020:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_0025:  ceq
      IL_0027:  nop
      IL_0028:  br.s       IL_002c

      IL_002a:  ldc.i4.0
      IL_002b:  nop
      IL_002c:  brfalse.s  IL_003e

      IL_002e:  ldarg.0
      IL_002f:  ldarg.1
      IL_0030:  ldarg.2
      IL_0031:  ldarg.3
      IL_0032:  ldc.i4.1
      IL_0033:  add
      IL_0034:  starg.s    k
      IL_0036:  starg.s    ndims
      IL_0038:  starg.s    y
      IL_003a:  starg.s    x
      IL_003c:  br.s       IL_0001

      IL_003e:  ldc.i4.0
      IL_003f:  ret
    } // end of method HashCompare::'precheck@1613-1'

    .method assembly static bool  'checkN@1621-1'(bool er,
                                                  class [mscorlib]System.Collections.IEqualityComparer iec,
                                                  class [mscorlib]System.Array x,
                                                  class [mscorlib]System.Array y,
                                                  int32 ndims,
                                                  int64[] idxs,
                                                  int32 k,
                                                  int64 baseIdx,
                                                  int64 i,
                                                  int64 lim) cil managed
    {
      // Code size       130 (0x82)
      .maxstack  10
      IL_0000:  ldarg.s    i
      IL_0002:  ldarg.s    lim
      IL_0004:  blt.s      IL_0008

      IL_0006:  ldc.i4.1
      IL_0007:  ret

      IL_0008:  nop
      IL_0009:  ldarg.s    idxs
      IL_000b:  ldarg.s    k
      IL_000d:  ldarg.s    baseIdx
      IL_000f:  ldarg.s    i
      IL_0011:  add
      IL_0012:  stelem     [mscorlib]System.Int64
      IL_0017:  ldarg.s    k
      IL_0019:  ldarg.s    ndims
      IL_001b:  ldc.i4.1
      IL_001c:  sub
      IL_001d:  bne.un.s   IL_0039

      IL_001f:  ldarg.0
      IL_0020:  ldarg.1
      IL_0021:  ldarg.2
      IL_0022:  ldarg.s    idxs
      IL_0024:  callvirt   instance object [mscorlib]System.Array::GetValue(int64[])
      IL_0029:  ldarg.3
      IL_002a:  ldarg.s    idxs
      IL_002c:  callvirt   instance object [mscorlib]System.Array::GetValue(int64[])
      IL_0031:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityObj(bool,
                                                                                                         class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                         object,
                                                                                                         object)
      IL_0036:  nop
      IL_0037:  br.s       IL_004b

      IL_0039:  ldarg.0
      IL_003a:  ldarg.1
      IL_003b:  ldarg.2
      IL_003c:  ldarg.3
      IL_003d:  ldarg.s    ndims
      IL_003f:  ldarg.s    idxs
      IL_0041:  ldarg.s    k
      IL_0043:  ldc.i4.1
      IL_0044:  add
      IL_0045:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::'check@1628-3'(bool,
                                                                                                     class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                     class [mscorlib]System.Array,
                                                                                                     class [mscorlib]System.Array,
                                                                                                     int32,
                                                                                                     int64[],
                                                                                                     int32)
      IL_004a:  nop
      IL_004b:  brfalse.s  IL_0080

      IL_004d:  ldarg.0
      IL_004e:  ldarg.1
      IL_004f:  ldarg.2
      IL_0050:  ldarg.3
      IL_0051:  ldarg.s    ndims
      IL_0053:  ldarg.s    idxs
      IL_0055:  ldarg.s    k
      IL_0057:  ldarg.s    baseIdx
      IL_0059:  ldarg.s    i
      IL_005b:  ldc.i8     0x1
      IL_0064:  add
      IL_0065:  ldarg.s    lim
      IL_0067:  starg.s    lim
      IL_0069:  starg.s    i
      IL_006b:  starg.s    baseIdx
      IL_006d:  starg.s    k
      IL_006f:  starg.s    idxs
      IL_0071:  starg.s    ndims
      IL_0073:  starg.s    y
      IL_0075:  starg.s    x
      IL_0077:  starg.s    iec
      IL_0079:  starg.s    er
      IL_007b:  br         IL_0000

      IL_0080:  ldc.i4.0
      IL_0081:  ret
    } // end of method HashCompare::'checkN@1621-1'

    .method assembly static bool  'check@1628-3'(bool er,
                                                 class [mscorlib]System.Collections.IEqualityComparer iec,
                                                 class [mscorlib]System.Array x,
                                                 class [mscorlib]System.Array y,
                                                 int32 ndims,
                                                 int64[] idxs,
                                                 int32 k) cil managed
    {
      // Code size       53 (0x35)
      .maxstack  11
      .locals init (int32 V_0)
      IL_0000:  ldarg.s    k
      IL_0002:  ldarg.s    ndims
      IL_0004:  blt.s      IL_0008

      IL_0006:  ldc.i4.1
      IL_0007:  ret

      IL_0008:  nop
      IL_0009:  ldarg.2
      IL_000a:  ldarg.s    k
      IL_000c:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_0011:  stloc.0
      IL_0012:  ldarg.0
      IL_0013:  ldarg.1
      IL_0014:  ldarg.2
      IL_0015:  ldarg.3
      IL_0016:  ldarg.s    ndims
      IL_0018:  ldarg.s    idxs
      IL_001a:  ldarg.s    k
      IL_001c:  ldloc.0
      IL_001d:  conv.i8
      IL_001e:  ldc.i8     0x0
      IL_0027:  ldarg.2
      IL_0028:  ldarg.s    k
      IL_002a:  callvirt   instance int64 [mscorlib]System.Array::GetLongLength(int32)
      IL_002f:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::'checkN@1621-1'(bool,
                                                                                                      class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                      class [mscorlib]System.Array,
                                                                                                      class [mscorlib]System.Array,
                                                                                                      int32,
                                                                                                      int64[],
                                                                                                      int32,
                                                                                                      int64,
                                                                                                      int64,
                                                                                                      int64)
      IL_0034:  ret
    } // end of method HashCompare::'check@1628-3'

    .method assembly specialname static class [mscorlib]System.Collections.IEqualityComparer
            get_fsEqualityComparerNoHashingPER() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  ldsfld     class [mscorlib]System.Collections.IEqualityComparer '<StartupCode$FSharp-Core>'.'$Prim-types'::fsEqualityComparerNoHashingPER@1654
      IL_0005:  ret
    } // end of method HashCompare::get_fsEqualityComparerNoHashingPER

    .method assembly specialname static class [mscorlib]System.Collections.IEqualityComparer
            get_fsEqualityComparerNoHashingER() cil managed
    {
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  ldsfld     class [mscorlib]System.Collections.IEqualityComparer '<StartupCode$FSharp-Core>'.'$Prim-types'::fsEqualityComparerNoHashingER@1661
      IL_0005:  ret
    } // end of method HashCompare::get_fsEqualityComparerNoHashingER

    .method public static bool  GenericEqualityERIntrinsic<T>(!!T x,
                                                              !!T y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
      .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                                  int32) = ( 01 00 66 54 68 69 73 20 66 75 6E 63 74 69 6F 6E   // ..fThis function
                                                                                             20 69 73 20 61 20 70 72 69 6D 69 74 69 76 65 20   //  is a primitive
                                                                                             6C 69 62 72 61 72 79 20 72 6F 75 74 69 6E 65 20   // library routine
                                                                                             75 73 65 64 20 62 79 20 6F 70 74 69 6D 69 7A 65   // used by optimize
                                                                                             64 20 46 23 20 63 6F 64 65 20 61 6E 64 20 73 68   // d F# code and sh
                                                                                             6F 75 6C 64 20 6E 6F 74 20 62 65 20 75 73 65 64   // ould not be used
                                                                                             20 64 69 72 65 63 74 6C 79 B4 04 00 00 01 00 54   //  directly......T
                                                                                             02 08 49 73 48 69 64 64 65 6E 01 )                // ..IsHidden.
      // Code size       27 (0x1b)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldc.i4.1
      IL_0002:  call       class [mscorlib]System.Collections.IEqualityComparer Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_fsEqualityComparerNoHashingER()
      IL_0007:  ldarg.0
      IL_0008:  box        !!T
      IL_000d:  ldarg.1
      IL_000e:  box        !!T
      IL_0013:  tail.
      IL_0015:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityObj(bool,
                                                                                                         class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                         object,
                                                                                                         object)
      IL_001a:  ret
    } // end of method HashCompare::GenericEqualityERIntrinsic

    .method public static bool  GenericEqualityWithComparerIntrinsic<T>(class [mscorlib]System.Collections.IEqualityComparer comp,
                                                                        !!T x,
                                                                        !!T y) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                                  int32) = ( 01 00 66 54 68 69 73 20 66 75 6E 63 74 69 6F 6E   // ..fThis function
                                                                                             20 69 73 20 61 20 70 72 69 6D 69 74 69 76 65 20   //  is a primitive
                                                                                             6C 69 62 72 61 72 79 20 72 6F 75 74 69 6E 65 20   // library routine
                                                                                             75 73 65 64 20 62 79 20 6F 70 74 69 6D 69 7A 65   // used by optimize
                                                                                             64 20 46 23 20 63 6F 64 65 20 61 6E 64 20 73 68   // d F# code and sh
                                                                                             6F 75 6C 64 20 6E 6F 74 20 62 65 20 75 73 65 64   // ould not be used
                                                                                             20 64 69 72 65 63 74 6C 79 B4 04 00 00 01 00 54   //  directly......T
                                                                                             02 08 49 73 48 69 64 64 65 6E 01 )                // ..IsHidden.
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 03 00 00 00 01 00 00 00 01 00 00 00 01 00
                                                                                                         00 00 00 00 )
      // Code size       22 (0x16)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldarg.1
      IL_0003:  box        !!T
      IL_0008:  ldarg.2
      IL_0009:  box        !!T
      IL_000e:  tail.
      IL_0010:  callvirt   instance bool [mscorlib]System.Collections.IEqualityComparer::Equals(object,
                                                                                                object)
      IL_0015:  ret
    } // end of method HashCompare::GenericEqualityWithComparerIntrinsic

    .method assembly static int32  GenericHashObjArray(class [mscorlib]System.Collections.IEqualityComparer iec,
                                                       object[] x) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
      // Code size       68 (0x44)
      .maxstack  4
      .locals init (int32 V_0,
               int32 V_1,
               int32 V_2,
               int32 V_3,
               int32 V_4)
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldloc.0
      IL_0006:  ldc.i4.1
      IL_0007:  sub
      IL_0008:  stloc.1
      IL_0009:  ldloc.1
      IL_000a:  ldc.i4.s   18
      IL_000c:  ble.s      IL_0014

      IL_000e:  ldc.i4.s   18
      IL_0010:  stloc.1
      IL_0011:  nop
      IL_0012:  br.s       IL_0015

      IL_0014:  nop
      IL_0015:  ldc.i4.0
      IL_0016:  stloc.2
      IL_0017:  ldloc.1
      IL_0018:  ldc.i4.0
      IL_0019:  blt.s      IL_0042

      IL_001b:  ldloc.1
      IL_001c:  stloc.3
      IL_001d:  ldloc.2
      IL_001e:  stloc.s    V_4
      IL_0020:  ldloc.s    V_4
      IL_0022:  ldc.i4.1
      IL_0023:  shl
      IL_0024:  ldarg.0
      IL_0025:  ldarg.1
      IL_0026:  ldloc.1
      IL_0027:  callvirt   instance object [mscorlib]System.Array::GetValue(int32)
      IL_002c:  callvirt   instance int32 [mscorlib]System.Collections.IEqualityComparer::GetHashCode(object)
      IL_0031:  add
      IL_0032:  ldc.i4     0x277
      IL_0037:  ldloc.3
      IL_0038:  mul
      IL_0039:  add
      IL_003a:  stloc.2
      IL_003b:  ldloc.1
      IL_003c:  ldc.i4.1
      IL_003d:  sub
      IL_003e:  stloc.1
      IL_003f:  nop
      IL_0040:  br.s       IL_0017

      IL_0042:  ldloc.2
      IL_0043:  ret
    } // end of method HashCompare::GenericHashObjArray

    .method assembly static int32  GenericHashByteArray(uint8[] x) cil managed
    {
      // Code size       62 (0x3e)
      .maxstack  3
      .locals init (int32 V_0,
               int32 V_1,
               int32 V_2,
               int32 V_3,
               int32 V_4)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldloc.0
      IL_0006:  ldc.i4.1
      IL_0007:  sub
      IL_0008:  stloc.1
      IL_0009:  ldloc.1
      IL_000a:  ldc.i4.s   18
      IL_000c:  ble.s      IL_0014

      IL_000e:  ldc.i4.s   18
      IL_0010:  stloc.1
      IL_0011:  nop
      IL_0012:  br.s       IL_0015

      IL_0014:  nop
      IL_0015:  ldc.i4.0
      IL_0016:  stloc.2
      IL_0017:  ldloc.1
      IL_0018:  ldc.i4.0
      IL_0019:  blt.s      IL_003c

      IL_001b:  ldloc.1
      IL_001c:  stloc.3
      IL_001d:  ldloc.2
      IL_001e:  stloc.s    V_4
      IL_0020:  ldloc.s    V_4
      IL_0022:  ldc.i4.1
      IL_0023:  shl
      IL_0024:  ldarg.0
      IL_0025:  ldloc.1
      IL_0026:  ldelem     [mscorlib]System.Byte
      IL_002b:  add
      IL_002c:  ldc.i4     0x277
      IL_0031:  ldloc.3
      IL_0032:  mul
      IL_0033:  add
      IL_0034:  stloc.2
      IL_0035:  ldloc.1
      IL_0036:  ldc.i4.1
      IL_0037:  sub
      IL_0038:  stloc.1
      IL_0039:  nop
      IL_003a:  br.s       IL_0017

      IL_003c:  ldloc.2
      IL_003d:  ret
    } // end of method HashCompare::GenericHashByteArray

    .method assembly static int32  GenericHashInt32Array(int32[] x) cil managed
    {
      // Code size       62 (0x3e)
      .maxstack  3
      .locals init (int32 V_0,
               int32 V_1,
               int32 V_2,
               int32 V_3,
               int32 V_4)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldloc.0
      IL_0006:  ldc.i4.1
      IL_0007:  sub
      IL_0008:  stloc.1
      IL_0009:  ldloc.1
      IL_000a:  ldc.i4.s   18
      IL_000c:  ble.s      IL_0014

      IL_000e:  ldc.i4.s   18
      IL_0010:  stloc.1
      IL_0011:  nop
      IL_0012:  br.s       IL_0015

      IL_0014:  nop
      IL_0015:  ldc.i4.0
      IL_0016:  stloc.2
      IL_0017:  ldloc.1
      IL_0018:  ldc.i4.0
      IL_0019:  blt.s      IL_003c

      IL_001b:  ldloc.1
      IL_001c:  stloc.3
      IL_001d:  ldloc.2
      IL_001e:  stloc.s    V_4
      IL_0020:  ldloc.s    V_4
      IL_0022:  ldc.i4.1
      IL_0023:  shl
      IL_0024:  ldarg.0
      IL_0025:  ldloc.1
      IL_0026:  ldelem     [mscorlib]System.Int32
      IL_002b:  add
      IL_002c:  ldc.i4     0x277
      IL_0031:  ldloc.3
      IL_0032:  mul
      IL_0033:  add
      IL_0034:  stloc.2
      IL_0035:  ldloc.1
      IL_0036:  ldc.i4.1
      IL_0037:  sub
      IL_0038:  stloc.1
      IL_0039:  nop
      IL_003a:  br.s       IL_0017

      IL_003c:  ldloc.2
      IL_003d:  ret
    } // end of method HashCompare::GenericHashInt32Array

    .method assembly static int32  GenericHashInt64Array(int64[] x) cil managed
    {
      // Code size       63 (0x3f)
      .maxstack  3
      .locals init (int32 V_0,
               int32 V_1,
               int32 V_2,
               int32 V_3,
               int32 V_4)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldlen
      IL_0003:  conv.i4
      IL_0004:  stloc.0
      IL_0005:  ldloc.0
      IL_0006:  ldc.i4.1
      IL_0007:  sub
      IL_0008:  stloc.1
      IL_0009:  ldloc.1
      IL_000a:  ldc.i4.s   18
      IL_000c:  ble.s      IL_0014

      IL_000e:  ldc.i4.s   18
      IL_0010:  stloc.1
      IL_0011:  nop
      IL_0012:  br.s       IL_0015

      IL_0014:  nop
      IL_0015:  ldc.i4.0
      IL_0016:  stloc.2
      IL_0017:  ldloc.1
      IL_0018:  ldc.i4.0
      IL_0019:  blt.s      IL_003d

      IL_001b:  ldloc.1
      IL_001c:  stloc.3
      IL_001d:  ldloc.2
      IL_001e:  stloc.s    V_4
      IL_0020:  ldloc.s    V_4
      IL_0022:  ldc.i4.1
      IL_0023:  shl
      IL_0024:  ldarg.0
      IL_0025:  ldloc.1
      IL_0026:  ldelem     [mscorlib]System.Int64
      IL_002b:  conv.i4
      IL_002c:  add
      IL_002d:  ldc.i4     0x277
      IL_0032:  ldloc.3
      IL_0033:  mul
      IL_0034:  add
      IL_0035:  stloc.2
      IL_0036:  ldloc.1
      IL_0037:  ldc.i4.1
      IL_0038:  sub
      IL_0039:  stloc.1
      IL_003a:  nop
      IL_003b:  br.s       IL_0017

      IL_003d:  ldloc.2
      IL_003e:  ret
    } // end of method HashCompare::GenericHashInt64Array

    .method assembly static int32  GenericHashArbArray(class [mscorlib]System.Collections.IEqualityComparer iec,
                                                       class [mscorlib]System.Array x) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
      // Code size       130 (0x82)
      .maxstack  4
      .locals init (int32 V_0,
               int32 V_1,
               int32 V_2,
               int32 V_3,
               int32 V_4,
               int32 V_5)
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  callvirt   instance int32 [mscorlib]System.Array::get_Rank()
      IL_0007:  ldc.i4.1
      IL_0008:  sub
      IL_0009:  switch     (
                            IL_002c)
      IL_0012:  ldarg.1
      IL_0013:  ldc.i4.0
      IL_0014:  callvirt   instance int32 [mscorlib]System.Array::GetLength(int32)
      IL_0019:  stloc.0
      IL_001a:  ldloc.0
      IL_001b:  ldc.i4.1
      IL_001c:  shl
      IL_001d:  ldarg.1
      IL_001e:  ldc.i4.1
      IL_001f:  callvirt   instance int32 [mscorlib]System.Array::GetLength(int32)
      IL_0024:  add
      IL_0025:  ldc.i4     0x18a6
      IL_002a:  add
      IL_002b:  ret

      IL_002c:  ldarg.1
      IL_002d:  ldc.i4.0
      IL_002e:  callvirt   instance int32 [mscorlib]System.Array::GetLowerBound(int32)
      IL_0033:  stloc.0
      IL_0034:  ldarg.1
      IL_0035:  callvirt   instance int32 [mscorlib]System.Array::get_Length()
      IL_003a:  stloc.1
      IL_003b:  ldloc.0
      IL_003c:  ldloc.1
      IL_003d:  add
      IL_003e:  ldc.i4.1
      IL_003f:  sub
      IL_0040:  stloc.2
      IL_0041:  ldloc.2
      IL_0042:  ldloc.0
      IL_0043:  ldc.i4.s   18
      IL_0045:  add
      IL_0046:  ble.s      IL_0050

      IL_0048:  ldloc.0
      IL_0049:  ldc.i4.s   18
      IL_004b:  add
      IL_004c:  stloc.2
      IL_004d:  nop
      IL_004e:  br.s       IL_0051

      IL_0050:  nop
      IL_0051:  ldc.i4.0
      IL_0052:  stloc.3
      IL_0053:  ldloc.2
      IL_0054:  ldloc.0
      IL_0055:  blt.s      IL_0080

      IL_0057:  ldloc.2
      IL_0058:  stloc.s    V_4
      IL_005a:  ldloc.3
      IL_005b:  stloc.s    V_5
      IL_005d:  ldloc.s    V_5
      IL_005f:  ldc.i4.1
      IL_0060:  shl
      IL_0061:  ldarg.0
      IL_0062:  ldarg.1
      IL_0063:  ldloc.2
      IL_0064:  callvirt   instance object [mscorlib]System.Array::GetValue(int32)
      IL_0069:  callvirt   instance int32 [mscorlib]System.Collections.IEqualityComparer::GetHashCode(object)
      IL_006e:  add
      IL_006f:  ldc.i4     0x277
      IL_0074:  ldloc.s    V_4
      IL_0076:  mul
      IL_0077:  add
      IL_0078:  stloc.3
      IL_0079:  ldloc.2
      IL_007a:  ldc.i4.1
      IL_007b:  sub
      IL_007c:  stloc.2
      IL_007d:  nop
      IL_007e:  br.s       IL_0053

      IL_0080:  ldloc.3
      IL_0081:  ret
    } // end of method HashCompare::GenericHashArbArray

    .method assembly static int32  GenericHashParamObj(class [mscorlib]System.Collections.IEqualityComparer iec,
                                                       object x) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
      // Code size       157 (0x9d)
      .maxstack  2
      .locals init (class [mscorlib]System.Array V_0,
               class [mscorlib]System.Array V_1,
               object[] V_2,
               object[] V_3,
               uint8[] V_4,
               uint8[] V_5,
               int32[] V_6,
               int32[] V_7,
               int64[] V_8,
               int64[] V_9,
               class [mscorlib]System.Collections.IStructuralEquatable V_10,
               class [mscorlib]System.Collections.IStructuralEquatable V_11)
      IL_0000:  ldarg.1
      IL_0001:  brfalse    IL_009b

      IL_0006:  ldarg.1
      IL_0007:  isinst     [mscorlib]System.Array
      IL_000c:  stloc.0
      IL_000d:  ldloc.0
      IL_000e:  brfalse.s  IL_0077

      IL_0010:  ldloc.0
      IL_0011:  stloc.1
      IL_0012:  ldloc.1
      IL_0013:  isinst     object[]
      IL_0018:  stloc.2
      IL_0019:  ldloc.2
      IL_001a:  brfalse.s  IL_0027

      IL_001c:  ldloc.2
      IL_001d:  stloc.3
      IL_001e:  nop
      IL_001f:  ldarg.0
      IL_0020:  ldloc.3
      IL_0021:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashObjArray(class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                           object[])
      IL_0026:  ret

      IL_0027:  ldloc.1
      IL_0028:  isinst     uint8[]
      IL_002d:  stloc.s    V_4
      IL_002f:  ldloc.s    V_4
      IL_0031:  brfalse.s  IL_003f

      IL_0033:  ldloc.s    V_4
      IL_0035:  stloc.s    V_5
      IL_0037:  ldloc.s    V_5
      IL_0039:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashByteArray(uint8[])
      IL_003e:  ret

      IL_003f:  ldloc.1
      IL_0040:  isinst     int32[]
      IL_0045:  stloc.s    V_6
      IL_0047:  ldloc.s    V_6
      IL_0049:  brfalse.s  IL_0057

      IL_004b:  ldloc.s    V_6
      IL_004d:  stloc.s    V_7
      IL_004f:  ldloc.s    V_7
      IL_0051:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashInt32Array(int32[])
      IL_0056:  ret

      IL_0057:  ldloc.1
      IL_0058:  isinst     int64[]
      IL_005d:  stloc.s    V_8
      IL_005f:  ldloc.s    V_8
      IL_0061:  brfalse.s  IL_006f

      IL_0063:  ldloc.s    V_8
      IL_0065:  stloc.s    V_9
      IL_0067:  ldloc.s    V_9
      IL_0069:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashInt64Array(int64[])
      IL_006e:  ret

      IL_006f:  ldarg.0
      IL_0070:  ldloc.1
      IL_0071:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashArbArray(class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                           class [mscorlib]System.Array)
      IL_0076:  ret

      IL_0077:  ldarg.1
      IL_0078:  isinst     [mscorlib]System.Collections.IStructuralEquatable
      IL_007d:  stloc.s    V_10
      IL_007f:  ldloc.s    V_10
      IL_0081:  brfalse.s  IL_0092

      IL_0083:  ldloc.s    V_10
      IL_0085:  stloc.s    V_11
      IL_0087:  ldloc.s    V_11
      IL_0089:  ldarg.0
      IL_008a:  tail.
      IL_008c:  callvirt   instance int32 [mscorlib]System.Collections.IStructuralEquatable::GetHashCode(class [mscorlib]System.Collections.IEqualityComparer)
      IL_0091:  ret

      IL_0092:  ldarg.1
      IL_0093:  tail.
      IL_0095:  callvirt   instance int32 [mscorlib]System.Object::GetHashCode()
      IL_009a:  ret

      IL_009b:  ldc.i4.0
      IL_009c:  ret
    } // end of method HashCompare::GenericHashParamObj

    .property class [mscorlib]System.Exception
            NaNException()
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 09 00 00 00 00 00 )
      .get class [mscorlib]System.Exception Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_NaNException()
    } // end of property HashCompare::NaNException
    .property class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer
            fsComparerER()
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 09 00 00 00 00 00 )
      .get class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/GenericComparer Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_fsComparerER()
    } // end of property HashCompare::fsComparerER
    .property class [mscorlib]System.Collections.IEqualityComparer
            fsEqualityComparerNoHashingPER()
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 09 00 00 00 00 00 )
      .get class [mscorlib]System.Collections.IEqualityComparer Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_fsEqualityComparerNoHashingPER()
    } // end of property HashCompare::fsEqualityComparerNoHashingPER
    .property class [mscorlib]System.Collections.IEqualityComparer
            fsEqualityComparerNoHashingER()
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 09 00 00 00 00 00 )
      .get class [mscorlib]System.Collections.IEqualityComparer Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_fsEqualityComparerNoHashingER()
    } // end of property HashCompare::fsEqualityComparerNoHashingER
    .property class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherER
            fsEqualityComparerUnlimitedHashingER()
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 09 00 00 00 00 00 )
      .get class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherER Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_fsEqualityComparerUnlimitedHashingER()
    } // end of property HashCompare::fsEqualityComparerUnlimitedHashingER
    .property class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherPER
            fsEqualityComparerUnlimitedHashingPER()
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 09 00 00 00 00 00 )
      .get class Microsoft.FSharp.Core.LanguagePrimitives/HashCompare/UnlimitedHasherPER Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::get_fsEqualityComparerUnlimitedHashingPER()
    } // end of property HashCompare::fsEqualityComparerUnlimitedHashingPER
  } // end of class HashCompare

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname Int16Comparer@2302
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<int16>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method Int16Comparer@2302::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(int16 x,
                                                                             int16 y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<int16>::Compare(!0,
                                                                                                              !0)
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  bge.s      IL_0007

      IL_0005:  ldc.i4.m1
      IL_0006:  ret

      IL_0007:  ldarg.1
      IL_0008:  ldarg.2
      IL_0009:  cgt
      IL_000b:  ret
    } // end of method Int16Comparer@2302::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class Int16Comparer@2302

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname Int16IEquality@2224
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int16>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method Int16IEquality@2224::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(int16 obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int16>::GetHashCode(!0)
      // Code size       9 (0x9)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  conv.u2
      IL_0003:  ldarg.1
      IL_0004:  ldc.i4.s   16
      IL_0006:  shl
      IL_0007:  or
      IL_0008:  ret
    } // end of method Int16IEquality@2224::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(int16 x,
                                                                                   int16 y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int16>::Equals(!0,
                                                                                                                    !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method Int16IEquality@2224::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class Int16IEquality@2224

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname Int32Comparer@2303
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<int32>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method Int32Comparer@2303::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(int32 x,
                                                                             int32 y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<int32>::Compare(!0,
                                                                                                              !0)
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  bge.s      IL_0007

      IL_0005:  ldc.i4.m1
      IL_0006:  ret

      IL_0007:  ldarg.1
      IL_0008:  ldarg.2
      IL_0009:  cgt
      IL_000b:  ret
    } // end of method Int32Comparer@2303::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class Int32Comparer@2303

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname Int32IEquality@2225
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int32>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method Int32IEquality@2225::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(int32 obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int32>::GetHashCode(!0)
      // Code size       3 (0x3)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ret
    } // end of method Int32IEquality@2225::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(int32 x,
                                                                                   int32 y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int32>::Equals(!0,
                                                                                                                    !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method Int32IEquality@2225::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class Int32IEquality@2225

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname Int64Comparer@2304
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<int64>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method Int64Comparer@2304::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(int64 x,
                                                                             int64 y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<int64>::Compare(!0,
                                                                                                              !0)
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  bge.s      IL_0007

      IL_0005:  ldc.i4.m1
      IL_0006:  ret

      IL_0007:  ldarg.1
      IL_0008:  ldarg.2
      IL_0009:  cgt
      IL_000b:  ret
    } // end of method Int64Comparer@2304::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class Int64Comparer@2304

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname Int64IEquality@2226
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int64>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method Int64IEquality@2226::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(int64 obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int64>::GetHashCode(!0)
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  conv.i4
      IL_0003:  ldarg.1
      IL_0004:  ldc.i4.s   32
      IL_0006:  shr
      IL_0007:  conv.i4
      IL_0008:  xor
      IL_0009:  ret
    } // end of method Int64IEquality@2226::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(int64 x,
                                                                                   int64 y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int64>::Equals(!0,
                                                                                                                    !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method Int64IEquality@2226::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class Int64IEquality@2226

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname IntPtrComparer@2305
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<native int>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method IntPtrComparer@2305::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(native int x,
                                                                             native int y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<native int>::Compare(!0,
                                                                                                                   !0)
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  bge.s      IL_0007

      IL_0005:  ldc.i4.m1
      IL_0006:  ret

      IL_0007:  ldarg.1
      IL_0008:  ldarg.2
      IL_0009:  cgt
      IL_000b:  ret
    } // end of method IntPtrComparer@2305::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class IntPtrComparer@2305

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname IntPtrIEquality@2227
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<native int>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method IntPtrIEquality@2227::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(native int obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<native int>::GetHashCode(!0)
      // Code size       5 (0x5)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  conv.u8
      IL_0003:  conv.i4
      IL_0004:  ret
    } // end of method IntPtrIEquality@2227::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(native int x,
                                                                                   native int y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<native int>::Equals(!0,
                                                                                                                         !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method IntPtrIEquality@2227::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class IntPtrIEquality@2227

  .class abstract auto ansi sealed nested public IntrinsicFunctions
         extends [mscorlib]System.Object
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 07 00 00 00 00 00 )
    .class auto ansi serializable nested assembly beforefieldinit TypeInfo`1<T>
           extends [mscorlib]System.Object
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
      .field static assembly int32 info
      .method public specialname rtspecialname
              instance void  .ctor() cil managed
      {
        // Code size       10 (0xa)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
        IL_0006:  ldarg.0
        IL_0007:  pop
        IL_0008:  nop
        IL_0009:  ret
      } // end of method TypeInfo`1::.ctor

      .method private specialname rtspecialname static
              void  .cctor() cil managed
      {
        // Code size       22 (0x16)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldtoken    !T
        IL_0006:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
        IL_000b:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::getTypeInfo(class [mscorlib]System.Type)
        IL_0010:  stsfld     int32 class Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions/TypeInfo`1<!T>::info
        IL_0015:  ret
      } // end of method TypeInfo`1::.cctor

    } // end of class TypeInfo`1

    .method public static bool  TypeTestGeneric<T>(object source) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                                  int32) = ( 01 00 4C 54 68 69 73 20 66 75 6E 63 74 69 6F 6E   // ..LThis function
                                                                                             20 69 73 20 66 6F 72 20 75 73 65 20 62 79 20 63   //  is for use by c
                                                                                             6F 6D 70 69 6C 65 64 20 46 23 20 63 6F 64 65 20   // ompiled F# code
                                                                                             61 6E 64 20 73 68 6F 75 6C 64 20 6E 6F 74 20 62   // and should not b
                                                                                             65 20 75 73 65 64 20 64 69 72 65 63 74 6C 79 B4   // e used directly.
                                                                                             04 00 00 01 00 54 02 08 49 73 48 69 64 64 65 6E   // .....T..IsHidden
                                                                                             01 )
      // Code size       31 (0x1f)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  isinst     !!T
      IL_0007:  ldnull
      IL_0008:  cgt.un
      IL_000a:  brfalse.s  IL_000e

      IL_000c:  ldc.i4.1
      IL_000d:  ret

      IL_000e:  ldarg.0
      IL_000f:  ldnull
      IL_0010:  cgt.un
      IL_0012:  brfalse.s  IL_0016

      IL_0014:  ldc.i4.0
      IL_0015:  ret

      IL_0016:  ldsfld     int32 class Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions/TypeInfo`1<!!T>::info
      IL_001b:  ldc.i4.2
      IL_001c:  ceq
      IL_001e:  ret
    } // end of method IntrinsicFunctions::TypeTestGeneric

    .method assembly static int32  getTypeInfo(class [mscorlib]System.Type ty) cil managed
    {
      // Code size       154 (0x9a)
      .maxstack  3
      .locals init (object[] V_0,
               object[] V_1,
               object V_2,
               class Microsoft.FSharp.Core.CompilationRepresentationAttribute V_3)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance bool [mscorlib]System.Type::get_IsValueType()
      IL_0007:  brfalse.s  IL_000b

      IL_0009:  ldc.i4.4
      IL_000a:  ret

      IL_000b:  ldarg.0
      IL_000c:  ldtoken    Microsoft.FSharp.Core.CompilationMappingAttribute
      IL_0011:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
      IL_0016:  ldc.i4.0
      IL_0017:  callvirt   instance object[] [mscorlib]System.Reflection.MemberInfo::GetCustomAttributes(class [mscorlib]System.Type,
                                                                                                         bool)
      IL_001c:  stloc.0
      IL_001d:  ldloc.0
      IL_001e:  ldlen
      IL_001f:  conv.i4
      IL_0020:  brtrue.s   IL_0024

      IL_0022:  ldc.i4.1
      IL_0023:  ret

      IL_0024:  ldarg.0
      IL_0025:  ldtoken    Microsoft.FSharp.Core.Unit
      IL_002a:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
      IL_002f:  callvirt   instance bool [mscorlib]System.Type::Equals(class [mscorlib]System.Type)
      IL_0034:  brfalse.s  IL_0038

      IL_0036:  ldc.i4.2
      IL_0037:  ret

      IL_0038:  ldtoken    [mscorlib]System.Delegate
      IL_003d:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
      IL_0042:  ldarg.0
      IL_0043:  callvirt   instance bool [mscorlib]System.Type::IsAssignableFrom(class [mscorlib]System.Type)
      IL_0048:  brfalse.s  IL_004c

      IL_004a:  ldc.i4.1
      IL_004b:  ret

      IL_004c:  ldarg.0
      IL_004d:  ldtoken    Microsoft.FSharp.Core.AllowNullLiteralAttribute
      IL_0052:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
      IL_0057:  ldc.i4.0
      IL_0058:  callvirt   instance object[] [mscorlib]System.Reflection.MemberInfo::GetCustomAttributes(class [mscorlib]System.Type,
                                                                                                         bool)
      IL_005d:  ldlen
      IL_005e:  conv.i4
      IL_005f:  ldc.i4.0
      IL_0060:  ble.s      IL_0064

      IL_0062:  ldc.i4.1
      IL_0063:  ret

      IL_0064:  ldarg.0
      IL_0065:  ldtoken    Microsoft.FSharp.Core.CompilationRepresentationAttribute
      IL_006a:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
      IL_006f:  ldc.i4.0
      IL_0070:  callvirt   instance object[] [mscorlib]System.Reflection.MemberInfo::GetCustomAttributes(class [mscorlib]System.Type,
                                                                                                         bool)
      IL_0075:  stloc.1
      IL_0076:  ldloc.1
      IL_0077:  ldlen
      IL_0078:  conv.i4
      IL_0079:  brtrue.s   IL_007d

      IL_007b:  ldc.i4.3
      IL_007c:  ret

      IL_007d:  ldloc.1
      IL_007e:  ldc.i4.0
      IL_007f:  ldelem     [mscorlib]System.Object
      IL_0084:  stloc.2
      IL_0085:  ldloc.2
      IL_0086:  unbox.any  Microsoft.FSharp.Core.CompilationRepresentationAttribute
      IL_008b:  stloc.3
      IL_008c:  ldloc.3
      IL_008d:  ldfld      valuetype Microsoft.FSharp.Core.CompilationRepresentationFlags Microsoft.FSharp.Core.CompilationRepresentationAttribute::'flags'
      IL_0092:  ldc.i4.8
      IL_0093:  and
      IL_0094:  brtrue.s   IL_0098

      IL_0096:  ldc.i4.3
      IL_0097:  ret

      IL_0098:  ldc.i4.2
      IL_0099:  ret
    } // end of method IntrinsicFunctions::getTypeInfo

    .method public static void  FailStaticInit() cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                                  int32) = ( 01 00 4C 54 68 69 73 20 66 75 6E 63 74 69 6F 6E   // ..LThis function
                                                                                             20 69 73 20 66 6F 72 20 75 73 65 20 62 79 20 63   //  is for use by c
                                                                                             6F 6D 70 69 6C 65 64 20 46 23 20 63 6F 64 65 20   // ompiled F# code
                                                                                             61 6E 64 20 73 68 6F 75 6C 64 20 6E 6F 74 20 62   // and should not b
                                                                                             65 20 75 73 65 64 20 64 69 72 65 63 74 6C 79 B4   // e used directly.
                                                                                             04 00 00 01 00 54 02 08 49 73 48 69 64 64 65 6E   // .....T..IsHidden
                                                                                             01 )
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldstr      "checkStaticInit"
      IL_0006:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor(string)
      IL_000b:  throw
    } // end of method IntrinsicFunctions::FailStaticInit

  } // end of class IntrinsicFunctions

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname SByteComparer@2301
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<int8>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method SByteComparer@2301::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(int8 x,
                                                                             int8 y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<int8>::Compare(!0,
                                                                                                             !0)
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  bge.s      IL_0007

      IL_0005:  ldc.i4.m1
      IL_0006:  ret

      IL_0007:  ldarg.1
      IL_0008:  ldarg.2
      IL_0009:  cgt
      IL_000b:  ret
    } // end of method SByteComparer@2301::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class SByteComparer@2301

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname SByteIEquality@2223
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int8>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method SByteIEquality@2223::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(int8 obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int8>::GetHashCode(!0)
      // Code size       7 (0x7)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldc.i4.8
      IL_0003:  shl
      IL_0004:  ldarg.1
      IL_0005:  xor
      IL_0006:  ret
    } // end of method SByteIEquality@2223::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(int8 x,
                                                                                   int8 y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<int8>::Equals(!0,
                                                                                                                   !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method SByteIEquality@2223::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class SByteIEquality@2223

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname StringComparer@2300
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<string>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method StringComparer@2300::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(string x,
                                                                             string y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<string>::Compare(!0,
                                                                                                               !0)
      // Code size       9 (0x9)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  call       int32 [mscorlib]System.String::CompareOrdinal(string,
                                                                         string)
      IL_0008:  ret
    } // end of method StringComparer@2300::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class StringComparer@2300

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname StringIEquality@2222
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<string>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method StringIEquality@2222::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(string obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<string>::GetHashCode(!0)
      // Code size       13 (0xd)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  brfalse.s  IL_000b

      IL_0004:  ldarg.1
      IL_0005:  call       instance int32 [mscorlib]System.String::GetHashCode()
      IL_000a:  ret

      IL_000b:  ldc.i4.0
      IL_000c:  ret
    } // end of method StringIEquality@2222::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(string x,
                                                                                   string y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<string>::Equals(!0,
                                                                                                                     !0)
      // Code size       9 (0x9)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  call       bool [mscorlib]System.String::Equals(string,
                                                                string)
      IL_0008:  ret
    } // end of method StringIEquality@2222::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class StringIEquality@2222

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname UInt16Comparer@2307
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<uint16>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method UInt16Comparer@2307::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(uint16 x,
                                                                             uint16 y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<uint16>::Compare(!0,
                                                                                                               !0)
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  bge.un.s   IL_0007

      IL_0005:  ldc.i4.m1
      IL_0006:  ret

      IL_0007:  ldarg.1
      IL_0008:  ldarg.2
      IL_0009:  cgt.un
      IL_000b:  ret
    } // end of method UInt16Comparer@2307::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class UInt16Comparer@2307

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname UInt16IEquality@2229
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint16>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method UInt16IEquality@2229::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(uint16 obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint16>::GetHashCode(!0)
      // Code size       3 (0x3)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ret
    } // end of method UInt16IEquality@2229::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(uint16 x,
                                                                                   uint16 y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint16>::Equals(!0,
                                                                                                                     !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method UInt16IEquality@2229::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class UInt16IEquality@2229

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname UInt32Comparer@2308
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<uint32>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method UInt32Comparer@2308::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(uint32 x,
                                                                             uint32 y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<uint32>::Compare(!0,
                                                                                                               !0)
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  bge.un.s   IL_0007

      IL_0005:  ldc.i4.m1
      IL_0006:  ret

      IL_0007:  ldarg.1
      IL_0008:  ldarg.2
      IL_0009:  cgt.un
      IL_000b:  ret
    } // end of method UInt32Comparer@2308::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class UInt32Comparer@2308

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname UInt32IEquality@2230
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint32>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method UInt32IEquality@2230::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(uint32 obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint32>::GetHashCode(!0)
      // Code size       3 (0x3)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ret
    } // end of method UInt32IEquality@2230::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(uint32 x,
                                                                                   uint32 y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint32>::Equals(!0,
                                                                                                                     !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method UInt32IEquality@2230::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class UInt32IEquality@2230

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname UInt64Comparer@2309
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<uint64>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method UInt64Comparer@2309::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(uint64 x,
                                                                             uint64 y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<uint64>::Compare(!0,
                                                                                                               !0)
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  bge.un.s   IL_0007

      IL_0005:  ldc.i4.m1
      IL_0006:  ret

      IL_0007:  ldarg.1
      IL_0008:  ldarg.2
      IL_0009:  cgt.un
      IL_000b:  ret
    } // end of method UInt64Comparer@2309::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class UInt64Comparer@2309

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname UInt64IEquality@2231
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint64>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method UInt64IEquality@2231::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(uint64 obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint64>::GetHashCode(!0)
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  conv.i4
      IL_0003:  ldarg.1
      IL_0004:  ldc.i4.s   32
      IL_0006:  shr.un
      IL_0007:  conv.i4
      IL_0008:  xor
      IL_0009:  ret
    } // end of method UInt64IEquality@2231::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(uint64 x,
                                                                                   uint64 y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<uint64>::Equals(!0,
                                                                                                                     !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method UInt64IEquality@2231::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class UInt64IEquality@2231

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname UIntPtrComparer@2310
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IComparer`1<native uint>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method UIntPtrComparer@2310::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IComparer`1-Compare'(native uint x,
                                                                             native uint y) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IComparer`1<native uint>::Compare(!0,
                                                                                                                    !0)
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  bge.un.s   IL_0007

      IL_0005:  ldc.i4.m1
      IL_0006:  ret

      IL_0007:  ldarg.1
      IL_0008:  ldarg.2
      IL_0009:  cgt.un
      IL_000b:  ret
    } // end of method UIntPtrComparer@2310::'System-Collections-Generic-IComparer`1-Compare'

  } // end of class UIntPtrComparer@2310

  .class auto autochar serializable sealed nested assembly beforefieldinit specialname UIntPtrIEquality@2232
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEqualityComparer`1<native uint>
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .method public specialname rtspecialname
            instance void  .ctor() cil managed
    {
      // Code size       10 (0xa)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0007:  ldarg.0
      IL_0008:  pop
      IL_0009:  ret
    } // end of method UIntPtrIEquality@2232::.ctor

    .method private hidebysig newslot virtual final
            instance int32  'System-Collections-Generic-IEqualityComparer`1-GetHashCode'(native uint obj) cil managed
    {
      .override  method instance int32 class [mscorlib]System.Collections.Generic.IEqualityComparer`1<native uint>::GetHashCode(!0)
      // Code size       11 (0xb)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  conv.u8
      IL_0003:  conv.i4
      IL_0004:  ldc.i4     0x7fffffff
      IL_0009:  and
      IL_000a:  ret
    } // end of method UIntPtrIEquality@2232::'System-Collections-Generic-IEqualityComparer`1-GetHashCode'

    .method private hidebysig newslot virtual final
            instance bool  'System-Collections-Generic-IEqualityComparer`1-Equals'(native uint x,
                                                                                   native uint y) cil managed
    {
      .override  method instance bool class [mscorlib]System.Collections.Generic.IEqualityComparer`1<native uint>::Equals(!0,
                                                                                                                          !0)
      // Code size       6 (0x6)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldarg.2
      IL_0003:  ceq
      IL_0005:  ret
    } // end of method UIntPtrIEquality@2232::'System-Collections-Generic-IEqualityComparer`1-Equals'

  } // end of class UIntPtrIEquality@2232

  .method assembly static string  anyToStringShowingNull<T>(!!T x) cil managed
  {
    // Code size       55 (0x37)
    .maxstack  3
    .locals init (object V_0,
             class [mscorlib]System.IFormattable V_1,
             class [mscorlib]System.IFormattable V_2,
             object V_3)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  box        !!T
    IL_0007:  stloc.0
    IL_0008:  ldloc.0
    IL_0009:  brfalse.s  IL_0031

    IL_000b:  ldloc.0
    IL_000c:  isinst     [mscorlib]System.IFormattable
    IL_0011:  stloc.1
    IL_0012:  ldloc.1
    IL_0013:  brfalse.s  IL_0026

    IL_0015:  ldloc.1
    IL_0016:  stloc.2
    IL_0017:  ldloc.2
    IL_0018:  ldnull
    IL_0019:  call       class [mscorlib]System.Globalization.CultureInfo [mscorlib]System.Globalization.CultureInfo::get_InvariantCulture()
    IL_001e:  tail.
    IL_0020:  callvirt   instance string [mscorlib]System.IFormattable::ToString(string,
                                                                                 class [mscorlib]System.IFormatProvider)
    IL_0025:  ret

    IL_0026:  ldloc.0
    IL_0027:  stloc.3
    IL_0028:  ldloc.3
    IL_0029:  tail.
    IL_002b:  callvirt   instance string [mscorlib]System.Object::ToString()
    IL_0030:  ret

    IL_0031:  ldstr      "null"
    IL_0036:  ret
  } // end of method LanguagePrimitives::anyToStringShowingNull

  .method public specialname static class [mscorlib]System.Collections.IComparer
          get_GenericComparer() cil managed
  {
    // Code size       6 (0x6)
    .maxstack  8
    IL_0000:  ldsfld     class [mscorlib]System.Collections.IComparer '<StartupCode$FSharp-Core>'.'$Prim-types'::GenericComparer@2195
    IL_0005:  ret
  } // end of method LanguagePrimitives::get_GenericComparer

  .method public specialname static class [mscorlib]System.Collections.IEqualityComparer
          get_GenericEqualityComparer() cil managed
  {
    // Code size       6 (0x6)
    .maxstack  8
    IL_0000:  ldsfld     class [mscorlib]System.Collections.IEqualityComparer '<StartupCode$FSharp-Core>'.'$Prim-types'::GenericEqualityComparer@2196
    IL_0005:  ret
  } // end of method LanguagePrimitives::get_GenericEqualityComparer

  .property class [mscorlib]System.Collections.IComparer
          GenericComparer()
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 09 00 00 00 00 00 )
    .get class [mscorlib]System.Collections.IComparer Microsoft.FSharp.Core.LanguagePrimitives::get_GenericComparer()
  } // end of property LanguagePrimitives::GenericComparer
  .property class [mscorlib]System.Collections.IEqualityComparer
          GenericEqualityComparer()
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 09 00 00 00 00 00 )
    .get class [mscorlib]System.Collections.IEqualityComparer Microsoft.FSharp.Core.LanguagePrimitives::get_GenericEqualityComparer()
  } // end of property LanguagePrimitives::GenericEqualityComparer
} // end of class Microsoft.FSharp.Core.LanguagePrimitives

.class public abstract auto ansi serializable beforefieldinit Microsoft.FSharp.Core.FSharpFunc`2<T,TResult>
       extends [mscorlib]System.Object
{
  .custom instance void Microsoft.FSharp.Core.AbstractClassAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .method public hidebysig abstract virtual
          instance !TResult  Invoke(!T func) cil managed
  {
  } // end of method FSharpFunc`2::Invoke

  .method public specialname rtspecialname
          instance void  .ctor() cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  pop
    IL_0008:  nop
    IL_0009:  ret
  } // end of method FSharpFunc`2::.ctor

} // end of class Microsoft.FSharp.Core.FSharpFunc`2

.class private abstract auto ansi sealed Microsoft.FSharp.Collections.Internal
       extends [mscorlib]System.Object
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 07 00 00 00 00 00 )
  .class abstract auto ansi sealed nested assembly IEnumerator
         extends [mscorlib]System.Object
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 07 00 00 00 00 00 )
    .class auto autochar serializable sealed nested assembly beforefieldinit specialname filter@182<T>
           extends [mscorlib]System.Object
           implements class [mscorlib]System.Collections.Generic.IEnumerator`1<!T>,
                      [mscorlib]System.IDisposable,
                      [mscorlib]System.Collections.IEnumerator
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
      .field public class Microsoft.FSharp.Core.FSharpFunc`2<!T,bool> f
      .field public class [mscorlib]System.Collections.Generic.IEnumerator`1<!T> e
      .field public class Microsoft.FSharp.Core.FSharpRef`1<bool> started
      .method public specialname rtspecialname
              instance void  .ctor(class Microsoft.FSharp.Core.FSharpFunc`2<!T,bool> f,
                                   class [mscorlib]System.Collections.Generic.IEnumerator`1<!T> e,
                                   class Microsoft.FSharp.Core.FSharpRef`1<bool> started) cil managed
      {
        // Code size       31 (0x1f)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  ldarg.1
        IL_0002:  stfld      class Microsoft.FSharp.Core.FSharpFunc`2<!0,bool> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::f
        IL_0007:  ldarg.0
        IL_0008:  ldarg.2
        IL_0009:  stfld      class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::e
        IL_000e:  ldarg.0
        IL_000f:  ldarg.3
        IL_0010:  stfld      class Microsoft.FSharp.Core.FSharpRef`1<bool> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::started
        IL_0015:  nop
        IL_0016:  ldarg.0
        IL_0017:  callvirt   instance void [mscorlib]System.Object::.ctor()
        IL_001c:  ldarg.0
        IL_001d:  pop
        IL_001e:  ret
      } // end of method filter@182::.ctor

      .method private hidebysig newslot virtual final
              instance !T  'System-Collections-Generic-IEnumerator`1-get_Current'() cil managed
      {
        .override  method instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<!T>::get_Current()
        // Code size       40 (0x28)
        .maxstack  1
        .locals init (bool V_0)
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      class Microsoft.FSharp.Core.FSharpRef`1<bool> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::started
        IL_0007:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<bool>::contents@
        IL_000c:  stloc.0
        IL_000d:  ldloc.0
        IL_000e:  brtrue.s   IL_0019

        IL_0010:  call       !!0 Microsoft.FSharp.Collections.IEnumerator::notStarted<class Microsoft.FSharp.Core.Unit>()
        IL_0015:  pop
        IL_0016:  nop
        IL_0017:  br.s       IL_001a

        IL_0019:  nop
        IL_001a:  ldarg.0
        IL_001b:  ldfld      class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::e
        IL_0020:  tail.
        IL_0022:  callvirt   instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<!T>::get_Current()
        IL_0027:  ret
      } // end of method filter@182::'System-Collections-Generic-IEnumerator`1-get_Current'

      .method private hidebysig newslot virtual final
              instance object  'System-Collections-IEnumerator-get_Current'() cil managed
      {
        .override [mscorlib]System.Collections.IEnumerator::get_Current
        // Code size       43 (0x2b)
        .maxstack  1
        .locals init (bool V_0)
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      class Microsoft.FSharp.Core.FSharpRef`1<bool> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::started
        IL_0007:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<bool>::contents@
        IL_000c:  stloc.0
        IL_000d:  ldloc.0
        IL_000e:  brtrue.s   IL_0019

        IL_0010:  call       !!0 Microsoft.FSharp.Collections.IEnumerator::notStarted<class Microsoft.FSharp.Core.Unit>()
        IL_0015:  pop
        IL_0016:  nop
        IL_0017:  br.s       IL_001a

        IL_0019:  nop
        IL_001a:  ldarg.0
        IL_001b:  ldfld      class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::e
        IL_0020:  callvirt   instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<!T>::get_Current()
        IL_0025:  box        !T
        IL_002a:  ret
      } // end of method filter@182::'System-Collections-IEnumerator-get_Current'

      .method private hidebysig newslot virtual final
              instance bool  'System-Collections-IEnumerator-MoveNext'() cil managed
      {
        .override [mscorlib]System.Collections.IEnumerator::MoveNext
        // Code size       47 (0x2f)
        .maxstack  4
        .locals init (class Microsoft.FSharp.Core.FSharpFunc`2<!T,bool> V_0,
                 class [mscorlib]System.Collections.Generic.IEnumerator`1<!T> V_1,
                 class Microsoft.FSharp.Core.FSharpRef`1<bool> V_2)
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      class Microsoft.FSharp.Core.FSharpFunc`2<!0,bool> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::f
        IL_0007:  stloc.0
        IL_0008:  ldarg.0
        IL_0009:  ldfld      class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::e
        IL_000e:  stloc.1
        IL_000f:  ldarg.0
        IL_0010:  ldfld      class Microsoft.FSharp.Core.FSharpRef`1<bool> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::started
        IL_0015:  stloc.2
        IL_0016:  ldarg.0
        IL_0017:  ldfld      class Microsoft.FSharp.Core.FSharpFunc`2<!0,bool> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::f
        IL_001c:  ldarg.0
        IL_001d:  ldfld      class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::e
        IL_0022:  ldarg.0
        IL_0023:  ldfld      class Microsoft.FSharp.Core.FSharpRef`1<bool> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::started
        IL_0028:  ldnull
        IL_0029:  call       bool Microsoft.FSharp.Collections.Internal/IEnumerator::next@187<!T>(class Microsoft.FSharp.Core.FSharpFunc`2<!!0,bool>,
                                                                                                  class [mscorlib]System.Collections.Generic.IEnumerator`1<!!0>,
                                                                                                  class Microsoft.FSharp.Core.FSharpRef`1<bool>,
                                                                                                  class Microsoft.FSharp.Core.Unit)
        IL_002e:  ret
      } // end of method filter@182::'System-Collections-IEnumerator-MoveNext'

      .method private hidebysig newslot virtual final
              instance void  'System-Collections-IEnumerator-Reset'() cil managed
      {
        .override [mscorlib]System.Collections.IEnumerator::Reset
        // Code size       12 (0xc)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldstr      "resetNotSupported"
        IL_0006:  newobj     instance void [mscorlib]System.NotSupportedException::.ctor(string)
        IL_000b:  throw
      } // end of method filter@182::'System-Collections-IEnumerator-Reset'

      .method private hidebysig newslot virtual final
              instance void  'System-IDisposable-Dispose'() cil managed
      {
        .override [mscorlib]System.IDisposable::Dispose
        // Code size       15 (0xf)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!T>::e
        IL_0007:  tail.
        IL_0009:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
        IL_000e:  ret
      } // end of method filter@182::'System-IDisposable-Dispose'

    } // end of class filter@182

    .method assembly static bool  next@187<T>(class Microsoft.FSharp.Core.FSharpFunc`2<!!T,bool> f,
                                              class [mscorlib]System.Collections.Generic.IEnumerator`1<!!T> e,
                                              class Microsoft.FSharp.Core.FSharpRef`1<bool> started,
                                              class Microsoft.FSharp.Core.Unit unitVar0) cil managed
    {
      // Code size       60 (0x3c)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.2
      IL_0002:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<bool>::contents@
      IL_0007:  brtrue.s   IL_0013

      IL_0009:  ldarg.2
      IL_000a:  ldc.i4.1
      IL_000b:  call       instance void class Microsoft.FSharp.Core.FSharpRef`1<bool>::set_contents(!0)
      IL_0010:  nop
      IL_0011:  br.s       IL_0014

      IL_0013:  nop
      IL_0014:  ldarg.1
      IL_0015:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
      IL_001a:  brfalse.s  IL_003a

      IL_001c:  ldarg.0
      IL_001d:  ldarg.1
      IL_001e:  callvirt   instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<!!T>::get_Current()
      IL_0023:  callvirt   instance !1 class Microsoft.FSharp.Core.FSharpFunc`2<!!T,bool>::Invoke(!0)
      IL_0028:  brfalse.s  IL_002c

      IL_002a:  ldc.i4.1
      IL_002b:  ret

      IL_002c:  ldarg.0
      IL_002d:  ldarg.1
      IL_002e:  ldarg.2
      IL_002f:  ldnull
      IL_0030:  starg.s    unitVar0
      IL_0032:  starg.s    started
      IL_0034:  starg.s    e
      IL_0036:  starg.s    f
      IL_0038:  br.s       IL_0000

      IL_003a:  ldc.i4.0
      IL_003b:  ret
    } // end of method IEnumerator::next@187

    .method assembly static class [mscorlib]System.Collections.Generic.IEnumerator`1<!!T>
            'filter'<T>(class Microsoft.FSharp.Core.FSharpFunc`2<!!T,bool> f,
                        class [mscorlib]System.Collections.Generic.IEnumerator`1<!!T> e) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
      // Code size       17 (0x11)
      .maxstack  3
      .locals init (class Microsoft.FSharp.Core.FSharpRef`1<bool> V_0)
      IL_0000:  nop
      IL_0001:  ldc.i4.0
      IL_0002:  newobj     instance void class Microsoft.FSharp.Core.FSharpRef`1<bool>::.ctor(!0)
      IL_0007:  stloc.0
      IL_0008:  ldarg.0
      IL_0009:  ldarg.1
      IL_000a:  ldloc.0
      IL_000b:  newobj     instance void class Microsoft.FSharp.Collections.Internal/IEnumerator/filter@182<!!T>::.ctor(class Microsoft.FSharp.Core.FSharpFunc`2<!0,bool>,
                                                                                                                        class [mscorlib]System.Collections.Generic.IEnumerator`1<!0>,
                                                                                                                        class Microsoft.FSharp.Core.FSharpRef`1<bool>)
      IL_0010:  ret
    } // end of method IEnumerator::'filter'

  } // end of class IEnumerator

} // end of class Microsoft.FSharp.Collections.Internal

.class public abstract auto ansi sealed Microsoft.FSharp.Collections.SeqModule
       extends [mscorlib]System.Object
{
  .custom instance void Microsoft.FSharp.Core.CompilationRepresentationAttribute::.ctor(valuetype Microsoft.FSharp.Core.CompilationRepresentationFlags) = ( 01 00 04 00 00 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.RequireQualifiedAccessAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 07 00 00 00 00 00 )
  .class auto ansi serializable sealed nested assembly beforefieldinit Filter@583<T>
         extends class Microsoft.FSharp.Core.FSharpFunc`2<class [mscorlib]System.Collections.Generic.IEnumerator`1<!T>,class [mscorlib]System.Collections.Generic.IEnumerator`1<!T>>
  {
    .field public class Microsoft.FSharp.Core.FSharpFunc`2<!T,bool> f
    .method assembly specialname rtspecialname
            instance void  .ctor(class Microsoft.FSharp.Core.FSharpFunc`2<!T,bool> f) cil managed
    {
      .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      // Code size       14 (0xe)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void class Microsoft.FSharp.Core.FSharpFunc`2<class [mscorlib]System.Collections.Generic.IEnumerator`1<!T>,class [mscorlib]System.Collections.Generic.IEnumerator`1<!T>>::.ctor()
      IL_0006:  ldarg.0
      IL_0007:  ldarg.1
      IL_0008:  stfld      class Microsoft.FSharp.Core.FSharpFunc`2<!0,bool> class Microsoft.FSharp.Collections.SeqModule/Filter@583<!T>::f
      IL_000d:  ret
    } // end of method Filter@583::.ctor

    .method public strict virtual instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!T>
            Invoke(class [mscorlib]System.Collections.Generic.IEnumerator`1<!T> e) cil managed
    {
      // Code size       14 (0xe)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldfld      class Microsoft.FSharp.Core.FSharpFunc`2<!0,bool> class Microsoft.FSharp.Collections.SeqModule/Filter@583<!T>::f
      IL_0007:  ldarg.1
      IL_0008:  call       class [mscorlib]System.Collections.Generic.IEnumerator`1<!!0> Microsoft.FSharp.Collections.Internal/IEnumerator::'filter'<!T>(class Microsoft.FSharp.Core.FSharpFunc`2<!!0,bool>,
                                                                                                                                                         class [mscorlib]System.Collections.Generic.IEnumerator`1<!!0>)
      IL_000d:  ret
    } // end of method Filter@583::Invoke

  } // end of class Filter@583

  .class auto ansi serializable sealed nested assembly beforefieldinit revamp@574<b,a>
         extends class Microsoft.FSharp.Core.FSharpFunc`2<class Microsoft.FSharp.Core.Unit,class [mscorlib]System.Collections.Generic.IEnumerator`1<!b>>
  {
    .field public class Microsoft.FSharp.Core.FSharpFunc`2<class [mscorlib]System.Collections.Generic.IEnumerator`1<!a>,class [mscorlib]System.Collections.Generic.IEnumerator`1<!b>> f
    .field public class [mscorlib]System.Collections.Generic.IEnumerable`1<!a> ie
    .method assembly specialname rtspecialname
            instance void  .ctor(class Microsoft.FSharp.Core.FSharpFunc`2<class [mscorlib]System.Collections.Generic.IEnumerator`1<!a>,class [mscorlib]System.Collections.Generic.IEnumerator`1<!b>> f,
                                 class [mscorlib]System.Collections.Generic.IEnumerable`1<!a> ie) cil managed
    {
      .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      // Code size       21 (0x15)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  call       instance void class Microsoft.FSharp.Core.FSharpFunc`2<class Microsoft.FSharp.Core.Unit,class [mscorlib]System.Collections.Generic.IEnumerator`1<!b>>::.ctor()
      IL_0006:  ldarg.0
      IL_0007:  ldarg.1
      IL_0008:  stfld      class Microsoft.FSharp.Core.FSharpFunc`2<class [mscorlib]System.Collections.Generic.IEnumerator`1<!1>,class [mscorlib]System.Collections.Generic.IEnumerator`1<!0>> class Microsoft.FSharp.Collections.SeqModule/revamp@574<!b,!a>::f
      IL_000d:  ldarg.0
      IL_000e:  ldarg.2
      IL_000f:  stfld      class [mscorlib]System.Collections.Generic.IEnumerable`1<!1> class Microsoft.FSharp.Collections.SeqModule/revamp@574<!b,!a>::ie
      IL_0014:  ret
    } // end of method revamp@574::.ctor

    .method public strict virtual instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!b>
            Invoke(class Microsoft.FSharp.Core.Unit unitVar0) cil managed
    {
      // Code size       26 (0x1a)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldfld      class Microsoft.FSharp.Core.FSharpFunc`2<class [mscorlib]System.Collections.Generic.IEnumerator`1<!1>,class [mscorlib]System.Collections.Generic.IEnumerator`1<!0>> class Microsoft.FSharp.Collections.SeqModule/revamp@574<!b,!a>::f
      IL_0007:  ldarg.0
      IL_0008:  ldfld      class [mscorlib]System.Collections.Generic.IEnumerable`1<!1> class Microsoft.FSharp.Collections.SeqModule/revamp@574<!b,!a>::ie
      IL_000d:  callvirt   instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<!a>::GetEnumerator()
      IL_0012:  tail.
      IL_0014:  callvirt   instance !1 class Microsoft.FSharp.Core.FSharpFunc`2<class [mscorlib]System.Collections.Generic.IEnumerator`1<!a>,class [mscorlib]System.Collections.Generic.IEnumerator`1<!b>>::Invoke(!0)
      IL_0019:  ret
    } // end of method revamp@574::Invoke

  } // end of class revamp@574

  .method public static class [mscorlib]System.Collections.Generic.IEnumerable`1<!!T>
          Filter<T>(class Microsoft.FSharp.Core.FSharpFunc`2<!!T,bool> predicate,
                    class [mscorlib]System.Collections.Generic.IEnumerable`1<!!T> source) cil managed
  {
    .custom instance void Microsoft.FSharp.Core.CompilationSourceNameAttribute::.ctor(string) = ( 01 00 06 66 69 6C 74 65 72 00 00 )                // ...filter..
    .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
    // Code size       36 (0x24)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  box        class [mscorlib]System.Collections.Generic.IEnumerable`1<!!T>
    IL_0006:  brfalse.s  IL_000b

    IL_0008:  nop
    IL_0009:  br.s       IL_0016

    IL_000b:  ldstr      "source"
    IL_0010:  newobj     instance void [mscorlib]System.ArgumentNullException::.ctor(string)
    IL_0015:  throw

    IL_0016:  nop
    IL_0017:  ldarg.0
    IL_0018:  newobj     instance void class Microsoft.FSharp.Collections.SeqModule/Filter@583<!!T>::.ctor(class Microsoft.FSharp.Core.FSharpFunc`2<!0,bool>)
    IL_001d:  ldarg.1
    IL_001e:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!1> Microsoft.FSharp.Collections.SeqModule::revamp<!!0,!!0>(class Microsoft.FSharp.Core.FSharpFunc`2<class [mscorlib]System.Collections.Generic.IEnumerator`1<!!0>,class [mscorlib]System.Collections.Generic.IEnumerator`1<!!1>>,
                                                                                                                                               class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0>)
    IL_0023:  ret
  } // end of method SeqModule::Filter

  .method public static int32  Length<T>(class [mscorlib]System.Collections.Generic.IEnumerable`1<!!T> source) cil managed
  {
    .custom instance void Microsoft.FSharp.Core.CompilationSourceNameAttribute::.ctor(string) = ( 01 00 06 6C 65 6E 67 74 68 00 00 )                // ...length..
    // Code size       148 (0x94)
    .maxstack  2
    .locals init (!!T[] V_0,
             !!T[] V_1,
             class Microsoft.FSharp.Collections.FSharpList`1<!!T> V_2,
             class Microsoft.FSharp.Collections.FSharpList`1<!!T> V_3,
             class [mscorlib]System.Collections.Generic.ICollection`1<!!T> V_4,
             class [mscorlib]System.Collections.Generic.ICollection`1<!!T> V_5,
             class [mscorlib]System.Collections.Generic.IEnumerator`1<!!T> V_6,
             int32 V_7,
             int32 V_8,
             class [mscorlib]System.IDisposable V_9)
    IL_0000:  ldarg.0
    IL_0001:  box        class [mscorlib]System.Collections.Generic.IEnumerable`1<!!T>
    IL_0006:  brfalse.s  IL_000b

    IL_0008:  nop
    IL_0009:  br.s       IL_0016

    IL_000b:  ldstr      "source"
    IL_0010:  newobj     instance void [mscorlib]System.ArgumentNullException::.ctor(string)
    IL_0015:  throw

    IL_0016:  ldarg.0
    IL_0017:  isinst     !!T[]
    IL_001c:  stloc.0
    IL_001d:  ldloc.0
    IL_001e:  brfalse.s  IL_0027

    IL_0020:  ldloc.0
    IL_0021:  stloc.1
    IL_0022:  nop
    IL_0023:  ldloc.1
    IL_0024:  ldlen
    IL_0025:  conv.i4
    IL_0026:  ret

    IL_0027:  ldarg.0
    IL_0028:  isinst     class Microsoft.FSharp.Collections.FSharpList`1<!!T>
    IL_002d:  stloc.2
    IL_002e:  ldloc.2
    IL_002f:  brfalse.s  IL_003a

    IL_0031:  ldloc.2
    IL_0032:  stloc.3
    IL_0033:  ldloc.3
    IL_0034:  callvirt   instance int32 class Microsoft.FSharp.Collections.FSharpList`1<!!T>::get_Length()
    IL_0039:  ret

    IL_003a:  ldarg.0
    IL_003b:  isinst     class [mscorlib]System.Collections.Generic.ICollection`1<!!T>
    IL_0040:  stloc.s    V_4
    IL_0042:  ldloc.s    V_4
    IL_0044:  brfalse.s  IL_0054

    IL_0046:  ldloc.s    V_4
    IL_0048:  stloc.s    V_5
    IL_004a:  ldloc.s    V_5
    IL_004c:  tail.
    IL_004e:  callvirt   instance int32 class [mscorlib]System.Collections.Generic.ICollection`1<!!T>::get_Count()
    IL_0053:  ret

    IL_0054:  ldarg.0
    IL_0055:  callvirt   instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<!!T>::GetEnumerator()
    IL_005a:  stloc.s    V_6
    .try
    {
      IL_005c:  ldc.i4.0
      IL_005d:  stloc.s    V_8
      IL_005f:  ldloc.s    V_6
      IL_0061:  callvirt   instance bool [mscorlib]System.Collections.IEnumerator::MoveNext()
      IL_0066:  brfalse.s  IL_0071

      IL_0068:  ldloc.s    V_8
      IL_006a:  ldc.i4.1
      IL_006b:  add
      IL_006c:  stloc.s    V_8
      IL_006e:  nop
      IL_006f:  br.s       IL_005f

      IL_0071:  ldloc.s    V_8
      IL_0073:  stloc.s    V_7
      IL_0075:  leave.s    IL_0091

    }  // end .try
    finally
    {
      IL_0077:  ldloc.s    V_6
      IL_0079:  isinst     [mscorlib]System.IDisposable
      IL_007e:  stloc.s    V_9
      IL_0080:  ldloc.s    V_9
      IL_0082:  brfalse.s  IL_008e

      IL_0084:  ldloc.s    V_9
      IL_0086:  callvirt   instance void [mscorlib]System.IDisposable::Dispose()
      IL_008b:  ldnull
      IL_008c:  pop
      IL_008d:  endfinally
      IL_008e:  ldnull
      IL_008f:  pop
      IL_0090:  endfinally
    }  // end handler
    IL_0091:  ldloc.s    V_7
    IL_0093:  ret
  } // end of method SeqModule::Length

  .method assembly static class [mscorlib]System.Collections.Generic.IEnumerable`1<!!b>
          revamp<a,b>(class Microsoft.FSharp.Core.FSharpFunc`2<class [mscorlib]System.Collections.Generic.IEnumerator`1<!!a>,class [mscorlib]System.Collections.Generic.IEnumerator`1<!!b>> f,
                      class [mscorlib]System.Collections.Generic.IEnumerable`1<!!a> ie) cil managed
  {
    .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  newobj     instance void class Microsoft.FSharp.Collections.SeqModule/revamp@574<!!b,!!a>::.ctor(class Microsoft.FSharp.Core.FSharpFunc`2<class [mscorlib]System.Collections.Generic.IEnumerator`1<!1>,class [mscorlib]System.Collections.Generic.IEnumerator`1<!0>>,
                                                                                                               class [mscorlib]System.Collections.Generic.IEnumerable`1<!1>)
    IL_0008:  call       class [mscorlib]System.Collections.Generic.IEnumerable`1<!!0> Microsoft.FSharp.Collections.IEnumerator::mkSeq<!!1>(class Microsoft.FSharp.Core.FSharpFunc`2<class Microsoft.FSharp.Core.Unit,class [mscorlib]System.Collections.Generic.IEnumerator`1<!!0>>)
    IL_000d:  ret
  } // end of method SeqModule::revamp

} // end of class Microsoft.FSharp.Collections.SeqModule

.class private abstract auto ansi sealed Microsoft.FSharp.Collections.PrivateListHelpers
       extends [mscorlib]System.Object
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 07 00 00 00 00 00 )
  .class auto ansi serializable nested assembly beforefieldinit ListEnumerator`1<T>
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEnumerator`1<!T>,
                    [mscorlib]System.Collections.IEnumerator,
                    [mscorlib]System.IDisposable
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
    .field assembly class Microsoft.FSharp.Collections.FSharpList`1<!T> s
    .field assembly class Microsoft.FSharp.Collections.FSharpList`1<!T> curr
    .field assembly bool started
    .method public specialname rtspecialname
            instance void  .ctor(class Microsoft.FSharp.Collections.FSharpList`1<!T> s) cil managed
    {
      // Code size       36 (0x24)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_0006:  ldarg.0
      IL_0007:  pop
      IL_0008:  nop
      IL_0009:  ldarg.0
      IL_000a:  ldarg.1
      IL_000b:  stfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::s
      IL_0010:  ldarg.0
      IL_0011:  ldarg.0
      IL_0012:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::s
      IL_0017:  stfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::curr
      IL_001c:  ldarg.0
      IL_001d:  ldc.i4.0
      IL_001e:  stfld      bool class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::started
      IL_0023:  ret
    } // end of method ListEnumerator`1::.ctor

    .method assembly hidebysig instance !T
            GetCurrent() cil managed
    {
      // Code size       43 (0x2b)
      .maxstack  1
      .locals init (class Microsoft.FSharp.Collections.FSharpList`1<!T> V_0)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldfld      bool class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::started
      IL_0007:  brfalse.s  IL_0025

      IL_0009:  ldarg.0
      IL_000a:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::curr
      IL_000f:  stloc.0
      IL_0010:  ldloc.0
      IL_0011:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
      IL_0016:  brfalse.s  IL_001f

      IL_0018:  ldloc.0
      IL_0019:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
      IL_001e:  ret

      IL_001f:  call       !!0 Microsoft.FSharp.Collections.PrivateListHelpers::alreadyFinished<!T>()
      IL_0024:  ret

      IL_0025:  call       !!0 Microsoft.FSharp.Collections.PrivateListHelpers::notStarted<!T>()
      IL_002a:  ret
    } // end of method ListEnumerator`1::GetCurrent

    .method private hidebysig newslot specialname virtual
            instance !T  'System-Collections-Generic-IEnumerator`1-get_Current'() cil managed
    {
      .override  method instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<!T>::get_Current()
      // Code size       8 (0x8)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance !0 class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::GetCurrent()
      IL_0007:  ret
    } // end of method ListEnumerator`1::'System-Collections-Generic-IEnumerator`1-get_Current'

    .method private hidebysig newslot virtual
            instance bool  'System-Collections-IEnumerator-MoveNext'() cil managed
    {
      .override [mscorlib]System.Collections.IEnumerator::MoveNext
      // Code size       85 (0x55)
      .maxstack  2
      .locals init (class Microsoft.FSharp.Collections.FSharpList`1<!T> V_0,
               class Microsoft.FSharp.Collections.FSharpList`1<!T> V_1,
               class Microsoft.FSharp.Collections.FSharpList`1<!T> V_2)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldfld      bool class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::started
      IL_0007:  brfalse.s  IL_003c

      IL_0009:  ldarg.0
      IL_000a:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::curr
      IL_000f:  stloc.0
      IL_0010:  ldloc.0
      IL_0011:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
      IL_0016:  brfalse.s  IL_003a

      IL_0018:  ldloc.0
      IL_0019:  stloc.1
      IL_001a:  ldloc.1
      IL_001b:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
      IL_0020:  stloc.2
      IL_0021:  ldarg.0
      IL_0022:  ldloc.2
      IL_0023:  stfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::curr
      IL_0028:  ldarg.0
      IL_0029:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::curr
      IL_002e:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
      IL_0033:  ldnull
      IL_0034:  ceq
      IL_0036:  ldc.i4.0
      IL_0037:  ceq
      IL_0039:  ret

      IL_003a:  ldc.i4.0
      IL_003b:  ret

      IL_003c:  ldarg.0
      IL_003d:  ldc.i4.1
      IL_003e:  stfld      bool class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::started
      IL_0043:  ldarg.0
      IL_0044:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::curr
      IL_0049:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
      IL_004e:  ldnull
      IL_004f:  ceq
      IL_0051:  ldc.i4.0
      IL_0052:  ceq
      IL_0054:  ret
    } // end of method ListEnumerator`1::'System-Collections-IEnumerator-MoveNext'

    .method private hidebysig newslot specialname virtual
            instance object  'System-Collections-IEnumerator-get_Current'() cil managed
    {
      .override [mscorlib]System.Collections.IEnumerator::get_Current
      // Code size       13 (0xd)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  callvirt   instance !0 class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::GetCurrent()
      IL_0007:  box        !T
      IL_000c:  ret
    } // end of method ListEnumerator`1::'System-Collections-IEnumerator-get_Current'

    .method private hidebysig newslot virtual
            instance void  'System-Collections-IEnumerator-Reset'() cil managed
    {
      .override [mscorlib]System.Collections.IEnumerator::Reset
      // Code size       21 (0x15)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldc.i4.0
      IL_0003:  stfld      bool class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::started
      IL_0008:  ldarg.0
      IL_0009:  ldarg.0
      IL_000a:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::s
      IL_000f:  stfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::curr
      IL_0014:  ret
    } // end of method ListEnumerator`1::'System-Collections-IEnumerator-Reset'

    .method private hidebysig newslot virtual
            instance void  'System-IDisposable-Dispose'() cil managed
    {
      .override [mscorlib]System.IDisposable::Dispose
      // Code size       2 (0x2)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ret
    } // end of method ListEnumerator`1::'System-IDisposable-Dispose'

  } // end of class ListEnumerator`1

  .method assembly static !!a  notStarted<a>() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldstr      "enumerationNotStarted"
    IL_0006:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor(string)
    IL_000b:  throw
  } // end of method PrivateListHelpers::notStarted

  .method assembly static !!a  alreadyFinished<a>() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldstr      "enumerationAlreadyFinished"
    IL_0006:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor(string)
    IL_000b:  throw
  } // end of method PrivateListHelpers::alreadyFinished

  .method assembly static int32  lengthAcc<a>(int32 acc,
                                              class Microsoft.FSharp.Collections.FSharpList`1<!!a> xs) cil managed
  {
    .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 02 00 00 00 01 00 00 00 01 00 00 00 00 00 )
    // Code size       28 (0x1c)
    .maxstack  2
    .locals init (class Microsoft.FSharp.Collections.FSharpList`1<!!a> V_0)
    IL_0000:  ldarg.1
    IL_0001:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!!a>::tail
    IL_0006:  brfalse.s  IL_001a

    IL_0008:  ldarg.1
    IL_0009:  stloc.0
    IL_000a:  nop
    IL_000b:  ldarg.0
    IL_000c:  ldc.i4.1
    IL_000d:  add
    IL_000e:  ldloc.0
    IL_000f:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!!a>::tail
    IL_0014:  starg.s    xs
    IL_0016:  starg.s    acc
    IL_0018:  br.s       IL_0000

    IL_001a:  ldarg.0
    IL_001b:  ret
  } // end of method PrivateListHelpers::lengthAcc

} // end of class Microsoft.FSharp.Collections.PrivateListHelpers

.class public auto autochar serializable sealed beforefieldinit Microsoft.FSharp.Collections.FSharpList`1<T>
       extends [mscorlib]System.Object
       implements class [mscorlib]System.IEquatable`1<class Microsoft.FSharp.Collections.FSharpList`1<!T>>,
                  [mscorlib]System.Collections.IStructuralEquatable,
                  [mscorlib]System.IComparable,
                  class [mscorlib]System.IComparable`1<class Microsoft.FSharp.Collections.FSharpList`1<!T>>,
                  class [mscorlib]System.Collections.Generic.IReadOnlyCollection`1<!T>,
                  [mscorlib]System.Collections.IStructuralComparable,
                  [mscorlib]System.Collections.IEnumerable,
                  class [mscorlib]System.Collections.Generic.IEnumerable`1<!T>
{
  .custom instance void [mscorlib]System.Reflection.DefaultMemberAttribute::.ctor(string) = ( 01 00 04 49 74 65 6D 00 00 )                      // ...Item..
  .custom instance void Microsoft.FSharp.Core.StructuralEqualityAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.StructuralComparisonAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.CompiledNameAttribute::.ctor(string) = ( 01 00 0C 46 53 68 61 72 70 4C 69 73 74 60 31 00   // ...FSharpList`1.
                                                                                       00 )
  .custom instance void [mscorlib]System.Diagnostics.DebuggerTypeProxyAttribute::.ctor(class [mscorlib]System.Type) = ( 01 00 2C 4D 69 63 72 6F 73 6F 66 74 2E 46 53 68   // ..,Microsoft.FSh
                                                                                                                        61 72 70 2E 43 6F 6C 6C 65 63 74 69 6F 6E 73 2E   // arp.Collections.
                                                                                                                        4C 69 73 74 44 65 62 75 67 56 69 65 77 60 31 00   // ListDebugView`1.
                                                                                                                        00 )
  .custom instance void Microsoft.FSharp.Core.DefaultAugmentationAttribute::.ctor(bool) = ( 01 00 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 01 00 00 00 00 00 )
  .custom instance void [mscorlib]System.Diagnostics.DebuggerDisplayAttribute::.ctor(string) = ( 01 00 11 7B 44 65 62 75 67 44 69 73 70 6C 61 79   // ...{DebugDisplay
                                                                                                 2C 6E 71 7D 00 00 )                               // ,nq}..
  .field assembly !T head
  .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .field assembly class Microsoft.FSharp.Collections.FSharpList`1<!T> tail
  .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
  .field static assembly initonly class Microsoft.FSharp.Collections.FSharpList`1<!T> _unique_Empty
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
  .method private specialname rtspecialname static
          void  .cctor() cil managed
  {
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  newobj     instance void class Microsoft.FSharp.Collections.FSharpList`1<!T>::.ctor()
    IL_0005:  stsfld     class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::_unique_Empty
    IL_000a:  ret
  } // end of method FSharpList`1::.cctor

  .method assembly specialname rtspecialname
          instance void  .ctor() cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  } // end of method FSharpList`1::.ctor

  .method public hidebysig virtual final
          instance int32  CompareTo(class Microsoft.FSharp.Collections.FSharpList`1<!T> obj) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       160 (0xa0)
    .maxstack  3
    .locals init (int32 V_0,
             int32 V_1,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_2,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_3,
             int32 V_4,
             class [mscorlib]System.Collections.IComparer V_5,
             !T V_6,
             !T V_7,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_8,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_9)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldnull
    IL_0003:  cgt.un
    IL_0005:  brfalse    IL_0096

    IL_000a:  ldarg.1
    IL_000b:  ldnull
    IL_000c:  cgt.un
    IL_000e:  brfalse    IL_0094

    IL_0013:  ldarg.0
    IL_0014:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0019:  ldnull
    IL_001a:  cgt.un
    IL_001c:  stloc.0
    IL_001d:  ldarg.1
    IL_001e:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0023:  ldnull
    IL_0024:  cgt.un
    IL_0026:  stloc.1
    IL_0027:  ldloc.0
    IL_0028:  ldloc.1
    IL_0029:  bne.un.s   IL_0090

    IL_002b:  ldarg.0
    IL_002c:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0031:  brfalse.s  IL_008e

    IL_0033:  ldarg.0
    IL_0034:  stloc.2
    IL_0035:  ldarg.1
    IL_0036:  stloc.3
    IL_0037:  call       class [mscorlib]System.Collections.IComparer Microsoft.FSharp.Core.LanguagePrimitives::get_GenericComparer()
    IL_003c:  stloc.s    V_5
    IL_003e:  ldloc.2
    IL_003f:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_0044:  stloc.s    V_6
    IL_0046:  ldloc.3
    IL_0047:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_004c:  stloc.s    V_7
    IL_004e:  ldloc.s    V_5
    IL_0050:  ldloc.s    V_6
    IL_0052:  ldloc.s    V_7
    IL_0054:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericComparisonWithComparerIntrinsic<!T>(class [mscorlib]System.Collections.IComparer,
                                                                                                                                !!0,
                                                                                                                                !!0)
    IL_0059:  stloc.s    V_4
    IL_005b:  ldloc.s    V_4
    IL_005d:  ldc.i4.0
    IL_005e:  bge.s      IL_0063

    IL_0060:  ldloc.s    V_4
    IL_0062:  ret

    IL_0063:  ldloc.s    V_4
    IL_0065:  ldc.i4.0
    IL_0066:  ble.s      IL_006b

    IL_0068:  ldloc.s    V_4
    IL_006a:  ret

    IL_006b:  call       class [mscorlib]System.Collections.IComparer Microsoft.FSharp.Core.LanguagePrimitives::get_GenericComparer()
    IL_0070:  stloc.s    V_5
    IL_0072:  ldloc.2
    IL_0073:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0078:  stloc.s    V_8
    IL_007a:  ldloc.3
    IL_007b:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0080:  stloc.s    V_9
    IL_0082:  ldloc.s    V_8
    IL_0084:  ldloc.s    V_9
    IL_0086:  ldloc.s    V_5
    IL_0088:  callvirt   instance int32 class Microsoft.FSharp.Collections.FSharpList`1<!T>::CompareTo(object,
                                                                                                       class [mscorlib]System.Collections.IComparer)
    IL_008d:  ret

    IL_008e:  ldc.i4.0
    IL_008f:  ret

    IL_0090:  ldloc.0
    IL_0091:  ldloc.1
    IL_0092:  sub
    IL_0093:  ret

    IL_0094:  ldc.i4.1
    IL_0095:  ret

    IL_0096:  ldarg.1
    IL_0097:  ldnull
    IL_0098:  cgt.un
    IL_009a:  brfalse.s  IL_009e

    IL_009c:  ldc.i4.m1
    IL_009d:  ret

    IL_009e:  ldc.i4.0
    IL_009f:  ret
  } // end of method FSharpList`1::CompareTo

  .method public hidebysig virtual final
          instance int32  CompareTo(object obj) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  unbox.any  class Microsoft.FSharp.Collections.FSharpList`1<!T>
    IL_0008:  callvirt   instance int32 class Microsoft.FSharp.Collections.FSharpList`1<!T>::CompareTo(class Microsoft.FSharp.Collections.FSharpList`1<!0>)
    IL_000d:  ret
  } // end of method FSharpList`1::CompareTo

  .method public hidebysig virtual final
          instance int32  CompareTo(object obj,
                                    class [mscorlib]System.Collections.IComparer comp) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       169 (0xa9)
    .maxstack  3
    .locals init (class Microsoft.FSharp.Collections.FSharpList`1<!T> V_0,
             int32 V_1,
             int32 V_2,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_3,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_4,
             int32 V_5,
             !T V_6,
             !T V_7,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_8,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_9)
    IL_0000:  nop
    IL_0001:  ldarg.1
    IL_0002:  unbox.any  class Microsoft.FSharp.Collections.FSharpList`1<!T>
    IL_0007:  stloc.0
    IL_0008:  ldarg.0
    IL_0009:  ldnull
    IL_000a:  cgt.un
    IL_000c:  brfalse    IL_009a

    IL_0011:  ldarg.1
    IL_0012:  unbox.any  class Microsoft.FSharp.Collections.FSharpList`1<!T>
    IL_0017:  ldnull
    IL_0018:  cgt.un
    IL_001a:  brfalse    IL_0098

    IL_001f:  ldarg.0
    IL_0020:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0025:  ldnull
    IL_0026:  cgt.un
    IL_0028:  stloc.1
    IL_0029:  ldloc.0
    IL_002a:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_002f:  ldnull
    IL_0030:  cgt.un
    IL_0032:  stloc.2
    IL_0033:  ldloc.1
    IL_0034:  ldloc.2
    IL_0035:  bne.un.s   IL_0094

    IL_0037:  ldarg.0
    IL_0038:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_003d:  brfalse.s  IL_0092

    IL_003f:  ldarg.0
    IL_0040:  stloc.3
    IL_0041:  ldloc.0
    IL_0042:  stloc.s    V_4
    IL_0044:  ldloc.3
    IL_0045:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_004a:  stloc.s    V_6
    IL_004c:  ldloc.s    V_4
    IL_004e:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_0053:  stloc.s    V_7
    IL_0055:  ldarg.2
    IL_0056:  ldloc.s    V_6
    IL_0058:  ldloc.s    V_7
    IL_005a:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericComparisonWithComparerIntrinsic<!T>(class [mscorlib]System.Collections.IComparer,
                                                                                                                                !!0,
                                                                                                                                !!0)
    IL_005f:  stloc.s    V_5
    IL_0061:  ldloc.s    V_5
    IL_0063:  ldc.i4.0
    IL_0064:  bge.s      IL_0069

    IL_0066:  ldloc.s    V_5
    IL_0068:  ret

    IL_0069:  ldloc.s    V_5
    IL_006b:  ldc.i4.0
    IL_006c:  ble.s      IL_0071

    IL_006e:  ldloc.s    V_5
    IL_0070:  ret

    IL_0071:  ldloc.3
    IL_0072:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0077:  stloc.s    V_8
    IL_0079:  ldloc.s    V_4
    IL_007b:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0080:  stloc.s    V_9
    IL_0082:  ldloc.s    V_8
    IL_0084:  ldloc.s    V_9
    IL_0086:  ldarg.2
    IL_0087:  starg.s    comp
    IL_0089:  starg.s    obj
    IL_008b:  starg.s    0
    IL_008d:  br         IL_0000

    IL_0092:  ldc.i4.0
    IL_0093:  ret

    IL_0094:  ldloc.1
    IL_0095:  ldloc.2
    IL_0096:  sub
    IL_0097:  ret

    IL_0098:  ldc.i4.1
    IL_0099:  ret

    IL_009a:  ldarg.1
    IL_009b:  unbox.any  class Microsoft.FSharp.Collections.FSharpList`1<!T>
    IL_00a0:  ldnull
    IL_00a1:  cgt.un
    IL_00a3:  brfalse.s  IL_00a7

    IL_00a5:  ldc.i4.m1
    IL_00a6:  ret

    IL_00a7:  ldc.i4.0
    IL_00a8:  ret
  } // end of method FSharpList`1::CompareTo

  .method public hidebysig virtual final
          instance int32  GetHashCode(class [mscorlib]System.Collections.IEqualityComparer comp) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       96 (0x60)
    .maxstack  5
    .locals init (int32 V_0,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_1,
             !T V_2)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldnull
    IL_0003:  cgt.un
    IL_0005:  brfalse.s  IL_005e

    IL_0007:  ldc.i4.0
    IL_0008:  stloc.0
    IL_0009:  ldarg.0
    IL_000a:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_000f:  brfalse.s  IL_0054

    IL_0011:  ldarg.0
    IL_0012:  stloc.1
    IL_0013:  ldc.i4.1
    IL_0014:  stloc.0
    IL_0015:  ldc.i4     0x9e3779b9
    IL_001a:  ldloc.1
    IL_001b:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0020:  ldarg.1
    IL_0021:  callvirt   instance int32 class Microsoft.FSharp.Collections.FSharpList`1<!T>::GetHashCode(class [mscorlib]System.Collections.IEqualityComparer)
    IL_0026:  ldloc.0
    IL_0027:  ldc.i4.6
    IL_0028:  shl
    IL_0029:  ldloc.0
    IL_002a:  ldc.i4.2
    IL_002b:  shr
    IL_002c:  add
    IL_002d:  add
    IL_002e:  add
    IL_002f:  stloc.0
    IL_0030:  ldc.i4     0x9e3779b9
    IL_0035:  ldloc.1
    IL_0036:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_003b:  stloc.2
    IL_003c:  ldarg.1
    IL_003d:  ldloc.2
    IL_003e:  box        !T
    IL_0043:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashParamObj(class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                         object)
    IL_0048:  ldloc.0
    IL_0049:  ldc.i4.6
    IL_004a:  shl
    IL_004b:  ldloc.0
    IL_004c:  ldc.i4.2
    IL_004d:  shr
    IL_004e:  add
    IL_004f:  add
    IL_0050:  add
    IL_0051:  stloc.0
    IL_0052:  ldloc.0
    IL_0053:  ret

    IL_0054:  ldarg.0
    IL_0055:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_005a:  ldnull
    IL_005b:  cgt.un
    IL_005d:  ret

    IL_005e:  ldc.i4.0
    IL_005f:  ret
  } // end of method FSharpList`1::GetHashCode

  .method public hidebysig virtual final
          instance int32  GetHashCode() cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  call       class [mscorlib]System.Collections.IEqualityComparer Microsoft.FSharp.Core.LanguagePrimitives::get_GenericEqualityComparer()
    IL_0007:  callvirt   instance int32 class Microsoft.FSharp.Collections.FSharpList`1<!T>::GetHashCode(class [mscorlib]System.Collections.IEqualityComparer)
    IL_000c:  ret
  } // end of method FSharpList`1::GetHashCode

  .method public hidebysig virtual final
          instance bool  Equals(object obj,
                                class [mscorlib]System.Collections.IEqualityComparer comp) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       135 (0x87)
    .maxstack  3
    .locals init (class Microsoft.FSharp.Collections.FSharpList`1<!T> V_0,
             int32 V_1,
             int32 V_2,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_3,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_4,
             !T V_5,
             !T V_6,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_7,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_8)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldnull
    IL_0003:  cgt.un
    IL_0005:  brfalse    IL_007f

    IL_000a:  ldarg.1
    IL_000b:  isinst     class Microsoft.FSharp.Collections.FSharpList`1<!T>
    IL_0010:  stloc.0
    IL_0011:  ldloc.0
    IL_0012:  brfalse.s  IL_007d

    IL_0014:  ldarg.0
    IL_0015:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_001a:  ldnull
    IL_001b:  cgt.un
    IL_001d:  stloc.1
    IL_001e:  ldloc.0
    IL_001f:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0024:  ldnull
    IL_0025:  cgt.un
    IL_0027:  stloc.2
    IL_0028:  ldloc.1
    IL_0029:  ldloc.2
    IL_002a:  bne.un.s   IL_007b

    IL_002c:  ldarg.0
    IL_002d:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0032:  brfalse.s  IL_0079

    IL_0034:  ldarg.0
    IL_0035:  stloc.3
    IL_0036:  ldloc.0
    IL_0037:  stloc.s    V_4
    IL_0039:  ldloc.3
    IL_003a:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_003f:  stloc.s    V_5
    IL_0041:  ldloc.s    V_4
    IL_0043:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_0048:  stloc.s    V_6
    IL_004a:  ldarg.2
    IL_004b:  ldloc.s    V_5
    IL_004d:  ldloc.s    V_6
    IL_004f:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityWithComparerIntrinsic<!T>(class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                                             !!0,
                                                                                                                             !!0)
    IL_0054:  brfalse.s  IL_0077

    IL_0056:  ldloc.3
    IL_0057:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_005c:  stloc.s    V_7
    IL_005e:  ldloc.s    V_4
    IL_0060:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0065:  stloc.s    V_8
    IL_0067:  ldloc.s    V_7
    IL_0069:  ldloc.s    V_8
    IL_006b:  ldarg.2
    IL_006c:  starg.s    comp
    IL_006e:  starg.s    obj
    IL_0070:  starg.s    0
    IL_0072:  br         IL_0001

    IL_0077:  ldc.i4.0
    IL_0078:  ret

    IL_0079:  ldc.i4.1
    IL_007a:  ret

    IL_007b:  ldc.i4.0
    IL_007c:  ret

    IL_007d:  ldc.i4.0
    IL_007e:  ret

    IL_007f:  ldarg.1
    IL_0080:  ldnull
    IL_0081:  cgt.un
    IL_0083:  ldc.i4.0
    IL_0084:  ceq
    IL_0086:  ret
  } // end of method FSharpList`1::Equals

  .method public hidebysig virtual final
          instance bool  Equals(class Microsoft.FSharp.Collections.FSharpList`1<!T> obj) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       110 (0x6e)
    .maxstack  2
    .locals init (int32 V_0,
             int32 V_1,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_2,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_3,
             !T V_4,
             !T V_5)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldnull
    IL_0003:  cgt.un
    IL_0005:  brfalse.s  IL_0066

    IL_0007:  ldarg.1
    IL_0008:  ldnull
    IL_0009:  cgt.un
    IL_000b:  brfalse.s  IL_0064

    IL_000d:  ldarg.0
    IL_000e:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0013:  ldnull
    IL_0014:  cgt.un
    IL_0016:  stloc.0
    IL_0017:  ldarg.1
    IL_0018:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_001d:  ldnull
    IL_001e:  cgt.un
    IL_0020:  stloc.1
    IL_0021:  ldloc.0
    IL_0022:  ldloc.1
    IL_0023:  bne.un.s   IL_0062

    IL_0025:  ldarg.0
    IL_0026:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_002b:  brfalse.s  IL_0060

    IL_002d:  ldarg.0
    IL_002e:  stloc.2
    IL_002f:  ldarg.1
    IL_0030:  stloc.3
    IL_0031:  ldloc.2
    IL_0032:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_0037:  stloc.s    V_4
    IL_0039:  ldloc.3
    IL_003a:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_003f:  stloc.s    V_5
    IL_0041:  ldloc.s    V_4
    IL_0043:  ldloc.s    V_5
    IL_0045:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityERIntrinsic<!T>(!!0,
                                                                                                                   !!0)
    IL_004a:  brfalse.s  IL_005e

    IL_004c:  ldloc.2
    IL_004d:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0052:  ldloc.3
    IL_0053:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0058:  starg.s    obj
    IL_005a:  starg.s    0
    IL_005c:  br.s       IL_0001

    IL_005e:  ldc.i4.0
    IL_005f:  ret

    IL_0060:  ldc.i4.1
    IL_0061:  ret

    IL_0062:  ldc.i4.0
    IL_0063:  ret

    IL_0064:  ldc.i4.0
    IL_0065:  ret

    IL_0066:  ldarg.1
    IL_0067:  ldnull
    IL_0068:  cgt.un
    IL_006a:  ldc.i4.0
    IL_006b:  ceq
    IL_006d:  ret
  } // end of method FSharpList`1::Equals

  .method public hidebysig virtual final
          instance bool  Equals(object obj) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       21 (0x15)
    .maxstack  2
    .locals init (class Microsoft.FSharp.Collections.FSharpList`1<!T> V_0)
    IL_0000:  nop
    IL_0001:  ldarg.1
    IL_0002:  isinst     class Microsoft.FSharp.Collections.FSharpList`1<!T>
    IL_0007:  stloc.0
    IL_0008:  ldloc.0
    IL_0009:  brfalse.s  IL_0013

    IL_000b:  ldarg.0
    IL_000c:  ldloc.0
    IL_000d:  callvirt   instance bool class Microsoft.FSharp.Collections.FSharpList`1<!T>::Equals(class Microsoft.FSharp.Collections.FSharpList`1<!0>)
    IL_0012:  ret

    IL_0013:  ldc.i4.0
    IL_0014:  ret
  } // end of method FSharpList`1::Equals

  .method public hidebysig specialname instance int32
          get_Length() cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldc.i4.0
    IL_0002:  ldarg.0
    IL_0003:  call       int32 Microsoft.FSharp.Collections.PrivateListHelpers::lengthAcc<!T>(int32,
                                                                                              class Microsoft.FSharp.Collections.FSharpList`1<!!0>)
    IL_0008:  ret
  } // end of method FSharpList`1::get_Length

  .method public hidebysig virtual instance string
          ToString() cil managed
  {
    // Code size       433 (0x1b1)
    .maxstack  2
    .locals init (class Microsoft.FSharp.Collections.FSharpList`1<!T> V_0,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_1,
             class Microsoft.FSharp.Collections.FSharpList`1<!T> V_2,
             !T V_3,
             !T V_4,
             !T V_5)
    IL_0000:  ldarg.0
    IL_0001:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0006:  brfalse    IL_01ab

    IL_000b:  ldarg.0
    IL_000c:  stloc.0
    IL_000d:  ldloc.0
    IL_000e:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0013:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0018:  brfalse    IL_0178

    IL_001d:  ldloc.0
    IL_001e:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0023:  stloc.1
    IL_0024:  ldloc.1
    IL_0025:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_002a:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_002f:  brfalse    IL_0127

    IL_0034:  ldloc.1
    IL_0035:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_003a:  stloc.2
    IL_003b:  ldloc.2
    IL_003c:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0041:  ldfld      class Microsoft.FSharp.Collections.FSharpList`1<!0> class Microsoft.FSharp.Collections.FSharpList`1<!T>::tail
    IL_0046:  brtrue.s   IL_00b8

    IL_0048:  ldloc.2
    IL_0049:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_004e:  stloc.3
    IL_004f:  ldloc.1
    IL_0050:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_0055:  stloc.s    V_4
    IL_0057:  ldloc.0
    IL_0058:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_005d:  stloc.s    V_5
    IL_005f:  nop
    IL_0060:  newobj     instance void [mscorlib]System.Text.StringBuilder::.ctor()
    IL_0065:  ldstr      "["
    IL_006a:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_006f:  ldloc.s    V_5
    IL_0071:  call       string Microsoft.FSharp.Core.LanguagePrimitives::anyToStringShowingNull<!T>(!!0)
    IL_0076:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_007b:  ldstr      "; "
    IL_0080:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_0085:  ldloc.s    V_4
    IL_0087:  call       string Microsoft.FSharp.Core.LanguagePrimitives::anyToStringShowingNull<!T>(!!0)
    IL_008c:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_0091:  ldstr      "; "
    IL_0096:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_009b:  ldloc.3
    IL_009c:  call       string Microsoft.FSharp.Core.LanguagePrimitives::anyToStringShowingNull<!T>(!!0)
    IL_00a1:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_00a6:  ldstr      "]"
    IL_00ab:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_00b0:  tail.
    IL_00b2:  callvirt   instance string [mscorlib]System.Object::ToString()
    IL_00b7:  ret

    IL_00b8:  ldloc.2
    IL_00b9:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_00be:  stloc.3
    IL_00bf:  ldloc.1
    IL_00c0:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_00c5:  stloc.s    V_4
    IL_00c7:  ldloc.0
    IL_00c8:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_00cd:  stloc.s    V_5
    IL_00cf:  newobj     instance void [mscorlib]System.Text.StringBuilder::.ctor()
    IL_00d4:  ldstr      "["
    IL_00d9:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_00de:  ldloc.s    V_5
    IL_00e0:  call       string Microsoft.FSharp.Core.LanguagePrimitives::anyToStringShowingNull<!T>(!!0)
    IL_00e5:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_00ea:  ldstr      "; "
    IL_00ef:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_00f4:  ldloc.s    V_4
    IL_00f6:  call       string Microsoft.FSharp.Core.LanguagePrimitives::anyToStringShowingNull<!T>(!!0)
    IL_00fb:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_0100:  ldstr      "; "
    IL_0105:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_010a:  ldloc.3
    IL_010b:  call       string Microsoft.FSharp.Core.LanguagePrimitives::anyToStringShowingNull<!T>(!!0)
    IL_0110:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_0115:  ldstr      "; ... ]"
    IL_011a:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_011f:  tail.
    IL_0121:  callvirt   instance string [mscorlib]System.Object::ToString()
    IL_0126:  ret

    IL_0127:  ldloc.1
    IL_0128:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_012d:  stloc.3
    IL_012e:  ldloc.0
    IL_012f:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_0134:  stloc.s    V_4
    IL_0136:  newobj     instance void [mscorlib]System.Text.StringBuilder::.ctor()
    IL_013b:  ldstr      "["
    IL_0140:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_0145:  ldloc.s    V_4
    IL_0147:  call       string Microsoft.FSharp.Core.LanguagePrimitives::anyToStringShowingNull<!T>(!!0)
    IL_014c:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_0151:  ldstr      "; "
    IL_0156:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_015b:  ldloc.3
    IL_015c:  call       string Microsoft.FSharp.Core.LanguagePrimitives::anyToStringShowingNull<!T>(!!0)
    IL_0161:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_0166:  ldstr      "]"
    IL_016b:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_0170:  tail.
    IL_0172:  callvirt   instance string [mscorlib]System.Object::ToString()
    IL_0177:  ret

    IL_0178:  ldloc.0
    IL_0179:  ldfld      !0 class Microsoft.FSharp.Collections.FSharpList`1<!T>::head
    IL_017e:  stloc.3
    IL_017f:  newobj     instance void [mscorlib]System.Text.StringBuilder::.ctor()
    IL_0184:  ldstr      "["
    IL_0189:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_018e:  ldloc.3
    IL_018f:  call       string Microsoft.FSharp.Core.LanguagePrimitives::anyToStringShowingNull<!T>(!!0)
    IL_0194:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_0199:  ldstr      "]"
    IL_019e:  callvirt   instance class [mscorlib]System.Text.StringBuilder [mscorlib]System.Text.StringBuilder::Append(string)
    IL_01a3:  tail.
    IL_01a5:  callvirt   instance string [mscorlib]System.Object::ToString()
    IL_01aa:  ret

    IL_01ab:  ldstr      "[]"
    IL_01b0:  ret
  } // end of method FSharpList`1::ToString

  .method private hidebysig newslot virtual
          instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!T>
          'System-Collections-Generic-IEnumerable`1-GetEnumerator'() cil managed
  {
    .override  method instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<!T>::GetEnumerator()
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  newobj     instance void class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::.ctor(class Microsoft.FSharp.Collections.FSharpList`1<!0>)
    IL_0007:  ret
  } // end of method FSharpList`1::'System-Collections-Generic-IEnumerable`1-GetEnumerator'

  .method private hidebysig newslot virtual
          instance class [mscorlib]System.Collections.IEnumerator
          'System-Collections-IEnumerable-GetEnumerator'() cil managed
  {
    .override [mscorlib]System.Collections.IEnumerable::GetEnumerator
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  newobj     instance void class Microsoft.FSharp.Collections.PrivateListHelpers/ListEnumerator`1<!T>::.ctor(class Microsoft.FSharp.Collections.FSharpList`1<!0>)
    IL_0007:  ret
  } // end of method FSharpList`1::'System-Collections-IEnumerable-GetEnumerator'

  .method private hidebysig newslot specialname virtual
          instance int32  'System-Collections-Generic-IReadOnlyCollection`1-get_Count'() cil managed
  {
    .override  method instance int32 class [mscorlib]System.Collections.Generic.IReadOnlyCollection`1<!T>::get_Count()
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldc.i4.0
    IL_0002:  ldarg.0
    IL_0003:  call       int32 Microsoft.FSharp.Collections.PrivateListHelpers::lengthAcc<!T>(int32,
                                                                                              class Microsoft.FSharp.Collections.FSharpList`1<!!0>)
    IL_0008:  ret
  } // end of method FSharpList`1::'System-Collections-Generic-IReadOnlyCollection`1-get_Count'

  .property instance int32 Length()
  {
    .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
    .get instance int32 Microsoft.FSharp.Collections.FSharpList`1::get_Length()
  } // end of property FSharpList`1::Length
} // end of class Microsoft.FSharp.Collections.FSharpList`1

.class private abstract auto ansi sealed Microsoft.FSharp.Collections.IEnumerator
       extends [mscorlib]System.Object
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 07 00 00 00 00 00 )
  .class auto autochar serializable sealed nested assembly beforefieldinit specialname mkSeq@133<TResult>
         extends [mscorlib]System.Object
         implements class [mscorlib]System.Collections.Generic.IEnumerable`1<!TResult>,
                    [mscorlib]System.Collections.IEnumerable
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
    .field public class Microsoft.FSharp.Core.FSharpFunc`2<class Microsoft.FSharp.Core.Unit,class [mscorlib]System.Collections.Generic.IEnumerator`1<!TResult>> f
    .method public specialname rtspecialname
            instance void  .ctor(class Microsoft.FSharp.Core.FSharpFunc`2<class Microsoft.FSharp.Core.Unit,class [mscorlib]System.Collections.Generic.IEnumerator`1<!TResult>> f) cil managed
    {
      // Code size       17 (0x11)
      .maxstack  8
      IL_0000:  ldarg.0
      IL_0001:  ldarg.1
      IL_0002:  stfld      class Microsoft.FSharp.Core.FSharpFunc`2<class Microsoft.FSharp.Core.Unit,class [mscorlib]System.Collections.Generic.IEnumerator`1<!0>> class Microsoft.FSharp.Collections.IEnumerator/mkSeq@133<!TResult>::f
      IL_0007:  nop
      IL_0008:  ldarg.0
      IL_0009:  callvirt   instance void [mscorlib]System.Object::.ctor()
      IL_000e:  ldarg.0
      IL_000f:  pop
      IL_0010:  ret
    } // end of method mkSeq@133::.ctor

    .method private hidebysig newslot virtual final
            instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!TResult>
            'System-Collections-Generic-IEnumerable`1-GetEnumerator'() cil managed
    {
      .override  method instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<!TResult>::GetEnumerator()
      // Code size       16 (0x10)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldfld      class Microsoft.FSharp.Core.FSharpFunc`2<class Microsoft.FSharp.Core.Unit,class [mscorlib]System.Collections.Generic.IEnumerator`1<!0>> class Microsoft.FSharp.Collections.IEnumerator/mkSeq@133<!TResult>::f
      IL_0007:  ldnull
      IL_0008:  tail.
      IL_000a:  callvirt   instance !1 class Microsoft.FSharp.Core.FSharpFunc`2<class Microsoft.FSharp.Core.Unit,class [mscorlib]System.Collections.Generic.IEnumerator`1<!TResult>>::Invoke(!0)
      IL_000f:  ret
    } // end of method mkSeq@133::'System-Collections-Generic-IEnumerable`1-GetEnumerator'

    .method private hidebysig newslot virtual final
            instance class [mscorlib]System.Collections.IEnumerator
            'System-Collections-IEnumerable-GetEnumerator'() cil managed
    {
      .override [mscorlib]System.Collections.IEnumerable::GetEnumerator
      // Code size       14 (0xe)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldfld      class Microsoft.FSharp.Core.FSharpFunc`2<class Microsoft.FSharp.Core.Unit,class [mscorlib]System.Collections.Generic.IEnumerator`1<!0>> class Microsoft.FSharp.Collections.IEnumerator/mkSeq@133<!TResult>::f
      IL_0007:  ldnull
      IL_0008:  callvirt   instance !1 class Microsoft.FSharp.Core.FSharpFunc`2<class Microsoft.FSharp.Core.Unit,class [mscorlib]System.Collections.Generic.IEnumerator`1<!TResult>>::Invoke(!0)
      IL_000d:  ret
    } // end of method mkSeq@133::'System-Collections-IEnumerable-GetEnumerator'

  } // end of class mkSeq@133

  .method assembly static !!a  notStarted<a>() cil managed
  {
    // Code size       12 (0xc)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldstr      "enumerationNotStarted"
    IL_0006:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor(string)
    IL_000b:  throw
  } // end of method IEnumerator::notStarted

  .method assembly static class [mscorlib]System.Collections.Generic.IEnumerable`1<!!TResult>
          mkSeq<TResult>(class Microsoft.FSharp.Core.FSharpFunc`2<class Microsoft.FSharp.Core.Unit,class [mscorlib]System.Collections.Generic.IEnumerator`1<!!TResult>> f) cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  newobj     instance void class Microsoft.FSharp.Collections.IEnumerator/mkSeq@133<!!TResult>::.ctor(class Microsoft.FSharp.Core.FSharpFunc`2<class Microsoft.FSharp.Core.Unit,class [mscorlib]System.Collections.Generic.IEnumerator`1<!0>>)
    IL_0007:  ret
  } // end of method IEnumerator::mkSeq

} // end of class Microsoft.FSharp.Collections.IEnumerator

.class private auto ansi serializable beforefieldinit Microsoft.FSharp.Collections.ListDebugView`1<T>
       extends [mscorlib]System.Object
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
} // end of class Microsoft.FSharp.Collections.ListDebugView`1

.class public auto ansi serializable sealed Microsoft.FSharp.Core.AbstractClassAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 04 00 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .method public specialname rtspecialname
          instance void  .ctor() cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ret
  } // end of method AbstractClassAttribute::.ctor

} // end of class Microsoft.FSharp.Core.AbstractClassAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.AllowNullLiteralAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 04 00 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .field assembly bool 'value'
  .method public specialname rtspecialname
          instance void  .ctor(bool 'value') cil managed
  {
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ldarg.0
    IL_000a:  ldarg.1
    IL_000b:  stfld      bool Microsoft.FSharp.Core.AllowNullLiteralAttribute::'value'
    IL_0010:  ret
  } // end of method AllowNullLiteralAttribute::.ctor

  .method public specialname rtspecialname
          instance void  .ctor() cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldc.i4.1
    IL_0003:  callvirt   instance void Microsoft.FSharp.Core.AllowNullLiteralAttribute::.ctor(bool)
    IL_0008:  ret
  } // end of method AllowNullLiteralAttribute::.ctor

} // end of class Microsoft.FSharp.Core.AllowNullLiteralAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.AutoOpenAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 05 00 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 01 )                      // Multiple.
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .field assembly string path
  .method public specialname rtspecialname
          instance void  .ctor(string path) cil managed
  {
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ldarg.0
    IL_000a:  ldarg.1
    IL_000b:  stfld      string Microsoft.FSharp.Core.AutoOpenAttribute::path
    IL_0010:  ret
  } // end of method AutoOpenAttribute::.ctor

  .method public specialname rtspecialname
          instance void  .ctor() cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldstr      ""
    IL_0007:  callvirt   instance void Microsoft.FSharp.Core.AutoOpenAttribute::.ctor(string)
    IL_000c:  ret
  } // end of method AutoOpenAttribute::.ctor

} // end of class Microsoft.FSharp.Core.AutoOpenAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.CompilationArgumentCountsAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 40 00 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ..@.....T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .field assembly int32[] counts
  .method public specialname rtspecialname
          instance void  .ctor(int32[] counts) cil managed
  {
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ldarg.0
    IL_000a:  ldarg.1
    IL_000b:  stfld      int32[] Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::counts
    IL_0010:  ret
  } // end of method CompilationArgumentCountsAttribute::.ctor

} // end of class Microsoft.FSharp.Core.CompilationArgumentCountsAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.CompilationMappingAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 FF 7F 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .field assembly int32 variantNumber
  .field assembly class [mscorlib]System.Type[] typeDefinitions
  .field assembly valuetype Microsoft.FSharp.Core.SourceConstructFlags sourceConstructFlags
  .field assembly int32 sequenceNumber
  .field assembly string resourceName
  .method public specialname rtspecialname
          instance void  .ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags sourceConstructFlags) cil managed
  {
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  ldc.i4.0
    IL_0004:  ldc.i4.0
    IL_0005:  callvirt   instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags,
                                                                                                int32,
                                                                                                int32)
    IL_000a:  ret
  } // end of method CompilationMappingAttribute::.ctor

  .method assembly specialname rtspecialname
          instance void  .ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags sourceConstructFlags,
                               int32 variantNumber,
                               int32 sequenceNumber,
                               string resourceName,
                               class [mscorlib]System.Type[] typeDefinitions) cil managed
  {
    // Code size       47 (0x2f)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ldarg.0
    IL_000a:  ldarg.1
    IL_000b:  stfld      valuetype Microsoft.FSharp.Core.SourceConstructFlags Microsoft.FSharp.Core.CompilationMappingAttribute::sourceConstructFlags
    IL_0010:  ldarg.0
    IL_0011:  ldarg.2
    IL_0012:  stfld      int32 Microsoft.FSharp.Core.CompilationMappingAttribute::variantNumber
    IL_0017:  ldarg.0
    IL_0018:  ldarg.3
    IL_0019:  stfld      int32 Microsoft.FSharp.Core.CompilationMappingAttribute::sequenceNumber
    IL_001e:  ldarg.0
    IL_001f:  ldarg.s    resourceName
    IL_0021:  stfld      string Microsoft.FSharp.Core.CompilationMappingAttribute::resourceName
    IL_0026:  ldarg.0
    IL_0027:  ldarg.s    typeDefinitions
    IL_0029:  stfld      class [mscorlib]System.Type[] Microsoft.FSharp.Core.CompilationMappingAttribute::typeDefinitions
    IL_002e:  ret
  } // end of method CompilationMappingAttribute::.ctor

  .method public specialname rtspecialname
          instance void  .ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags sourceConstructFlags,
                               int32 variantNumber,
                               int32 sequenceNumber) cil managed
  {
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  ldarg.2
    IL_0004:  ldarg.3
    IL_0005:  ldnull
    IL_0006:  ldnull
    IL_0007:  callvirt   instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags,
                                                                                                int32,
                                                                                                int32,
                                                                                                string,
                                                                                                class [mscorlib]System.Type[])
    IL_000c:  ret
  } // end of method CompilationMappingAttribute::.ctor

  .method public specialname rtspecialname
          instance void  .ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags sourceConstructFlags,
                               int32 sequenceNumber) cil managed
  {
    // Code size       11 (0xb)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  ldc.i4.0
    IL_0004:  ldarg.2
    IL_0005:  callvirt   instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags,
                                                                                                int32,
                                                                                                int32)
    IL_000a:  ret
  } // end of method CompilationMappingAttribute::.ctor

} // end of class Microsoft.FSharp.Core.CompilationMappingAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.CompilationRepresentationAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 FF 7F 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .field assembly valuetype Microsoft.FSharp.Core.CompilationRepresentationFlags 'flags'
  .method public specialname rtspecialname
          instance void  .ctor(valuetype Microsoft.FSharp.Core.CompilationRepresentationFlags 'flags') cil managed
  {
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ldarg.0
    IL_000a:  ldarg.1
    IL_000b:  stfld      valuetype Microsoft.FSharp.Core.CompilationRepresentationFlags Microsoft.FSharp.Core.CompilationRepresentationAttribute::'flags'
    IL_0010:  ret
  } // end of method CompilationRepresentationAttribute::.ctor

} // end of class Microsoft.FSharp.Core.CompilationRepresentationAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.CompilationRepresentationFlags
       extends [mscorlib]System.Enum
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void [mscorlib]System.FlagsAttribute::.ctor() = ( 01 00 00 00 )
  .field public specialname rtspecialname int32 value__
  .field public static literal valuetype Microsoft.FSharp.Core.CompilationRepresentationFlags None = int32(0x00000000)
  .field public static literal valuetype Microsoft.FSharp.Core.CompilationRepresentationFlags Static = int32(0x00000001)
  .field public static literal valuetype Microsoft.FSharp.Core.CompilationRepresentationFlags Instance = int32(0x00000002)
  .field public static literal valuetype Microsoft.FSharp.Core.CompilationRepresentationFlags ModuleSuffix = int32(0x00000004)
  .field public static literal valuetype Microsoft.FSharp.Core.CompilationRepresentationFlags UseNullAsTrueValue = int32(0x00000008)
  .field public static literal valuetype Microsoft.FSharp.Core.CompilationRepresentationFlags Event = int32(0x00000010)
} // end of class Microsoft.FSharp.Core.CompilationRepresentationFlags

.class public auto ansi serializable sealed Microsoft.FSharp.Core.CompilationSourceNameAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 FF 7F 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .field assembly string sourceName
  .method public specialname rtspecialname
          instance void  .ctor(string sourceName) cil managed
  {
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ldarg.0
    IL_000a:  ldarg.1
    IL_000b:  stfld      string Microsoft.FSharp.Core.CompilationSourceNameAttribute::sourceName
    IL_0010:  ret
  } // end of method CompilationSourceNameAttribute::.ctor

} // end of class Microsoft.FSharp.Core.CompilationSourceNameAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.CompiledNameAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 DC 15 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .field assembly string compiledName
  .method public specialname rtspecialname
          instance void  .ctor(string compiledName) cil managed
  {
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ldarg.0
    IL_000a:  ldarg.1
    IL_000b:  stfld      string Microsoft.FSharp.Core.CompiledNameAttribute::compiledName
    IL_0010:  ret
  } // end of method CompiledNameAttribute::.ctor

} // end of class Microsoft.FSharp.Core.CompiledNameAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.CompilerMessageAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 FF 7F 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .field assembly int32 messageNumber
  .field assembly string message
  .field assembly bool isError
  .field assembly bool isHidden
  .method public specialname rtspecialname
          instance void  .ctor(string message,
                               int32 messageNumber) cil managed
  {
    // Code size       38 (0x26)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ldarg.0
    IL_000a:  ldarg.1
    IL_000b:  stfld      string Microsoft.FSharp.Core.CompilerMessageAttribute::message
    IL_0010:  ldarg.0
    IL_0011:  ldarg.2
    IL_0012:  stfld      int32 Microsoft.FSharp.Core.CompilerMessageAttribute::messageNumber
    IL_0017:  ldarg.0
    IL_0018:  ldc.i4.0
    IL_0019:  stfld      bool Microsoft.FSharp.Core.CompilerMessageAttribute::isError
    IL_001e:  ldarg.0
    IL_001f:  ldc.i4.0
    IL_0020:  stfld      bool Microsoft.FSharp.Core.CompilerMessageAttribute::isHidden
    IL_0025:  ret
  } // end of method CompilerMessageAttribute::.ctor

  .method public hidebysig specialname instance void
          set_IsHidden(bool v) cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  stfld      bool Microsoft.FSharp.Core.CompilerMessageAttribute::isHidden
    IL_0008:  ret
  } // end of method CompilerMessageAttribute::set_IsHidden

  .property instance bool IsHidden()
  {
    .set instance void Microsoft.FSharp.Core.CompilerMessageAttribute::set_IsHidden(bool)
  } // end of property CompilerMessageAttribute::IsHidden
} // end of class Microsoft.FSharp.Core.CompilerMessageAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.DefaultAugmentationAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 04 00 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .field assembly bool 'value'
  .method public specialname rtspecialname
          instance void  .ctor(bool 'value') cil managed
  {
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ldarg.0
    IL_000a:  ldarg.1
    IL_000b:  stfld      bool Microsoft.FSharp.Core.DefaultAugmentationAttribute::'value'
    IL_0010:  ret
  } // end of method DefaultAugmentationAttribute::.ctor

} // end of class Microsoft.FSharp.Core.DefaultAugmentationAttribute

.class public auto ansi serializable sealed beforefieldinit Microsoft.FSharp.Core.FSharpRef`1<T>
       extends [mscorlib]System.Object
       implements [mscorlib]System.Collections.IStructuralComparable,
                  [mscorlib]System.IComparable,
                  class [mscorlib]System.IComparable`1<class Microsoft.FSharp.Core.FSharpRef`1<!T>>,
                  [mscorlib]System.Collections.IStructuralEquatable,
                  class [mscorlib]System.IEquatable`1<class Microsoft.FSharp.Core.FSharpRef`1<!T>>
{
  .custom instance void [mscorlib]System.Diagnostics.DebuggerDisplayAttribute::.ctor(string) = ( 01 00 0A 7B 63 6F 6E 74 65 6E 74 73 7D 00 00 )    // ...{contents}..
  .custom instance void Microsoft.FSharp.Core.StructuralComparisonAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.StructuralEqualityAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.CompiledNameAttribute::.ctor(string) = ( 01 00 0B 46 53 68 61 72 70 52 65 66 60 31 00 00 ) // ...FSharpRef`1..
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 02 00 00 00 00 00 )
  .field public !T contents@
  .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
  .method public hidebysig specialname instance void
          set_contents(!T 'value') cil managed
  {
    // Code size       8 (0x8)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  ldarg.1
    IL_0002:  stfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<!T>::contents@
    IL_0007:  ret
  } // end of method FSharpRef`1::set_contents

  .method public specialname rtspecialname
          instance void  .ctor(!T contents) cil managed
  {
    // Code size       14 (0xe)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  ldarg.1
    IL_0008:  stfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<!T>::contents@
    IL_000d:  ret
  } // end of method FSharpRef`1::.ctor

  .method public hidebysig virtual final
          instance int32  CompareTo(class Microsoft.FSharp.Core.FSharpRef`1<!T> obj) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       56 (0x38)
    .maxstack  3
    .locals init (class [mscorlib]System.Collections.IComparer V_0,
             !T V_1,
             !T V_2)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldnull
    IL_0003:  cgt.un
    IL_0005:  brfalse.s  IL_002e

    IL_0007:  ldarg.1
    IL_0008:  ldnull
    IL_0009:  cgt.un
    IL_000b:  brfalse.s  IL_002c

    IL_000d:  call       class [mscorlib]System.Collections.IComparer Microsoft.FSharp.Core.LanguagePrimitives::get_GenericComparer()
    IL_0012:  stloc.0
    IL_0013:  ldarg.0
    IL_0014:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<!T>::contents@
    IL_0019:  stloc.1
    IL_001a:  ldarg.1
    IL_001b:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<!T>::contents@
    IL_0020:  stloc.2
    IL_0021:  ldloc.0
    IL_0022:  ldloc.1
    IL_0023:  ldloc.2
    IL_0024:  tail.
    IL_0026:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericComparisonWithComparerIntrinsic<!T>(class [mscorlib]System.Collections.IComparer,
                                                                                                                                !!0,
                                                                                                                                !!0)
    IL_002b:  ret

    IL_002c:  ldc.i4.1
    IL_002d:  ret

    IL_002e:  ldarg.1
    IL_002f:  ldnull
    IL_0030:  cgt.un
    IL_0032:  brfalse.s  IL_0036

    IL_0034:  ldc.i4.m1
    IL_0035:  ret

    IL_0036:  ldc.i4.0
    IL_0037:  ret
  } // end of method FSharpRef`1::CompareTo

  .method public hidebysig virtual final
          instance int32  CompareTo(object obj) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       16 (0x10)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldarg.1
    IL_0003:  unbox.any  class Microsoft.FSharp.Core.FSharpRef`1<!T>
    IL_0008:  tail.
    IL_000a:  callvirt   instance int32 class Microsoft.FSharp.Core.FSharpRef`1<!T>::CompareTo(class Microsoft.FSharp.Core.FSharpRef`1<!0>)
    IL_000f:  ret
  } // end of method FSharpRef`1::CompareTo

  .method public hidebysig virtual final
          instance int32  CompareTo(object obj,
                                    class [mscorlib]System.Collections.IComparer comp) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       67 (0x43)
    .maxstack  3
    .locals init (class Microsoft.FSharp.Core.FSharpRef`1<!T> V_0,
             !T V_1,
             !T V_2)
    IL_0000:  nop
    IL_0001:  ldarg.1
    IL_0002:  unbox.any  class Microsoft.FSharp.Core.FSharpRef`1<!T>
    IL_0007:  stloc.0
    IL_0008:  ldarg.0
    IL_0009:  ldnull
    IL_000a:  cgt.un
    IL_000c:  brfalse.s  IL_0034

    IL_000e:  ldarg.1
    IL_000f:  unbox.any  class Microsoft.FSharp.Core.FSharpRef`1<!T>
    IL_0014:  ldnull
    IL_0015:  cgt.un
    IL_0017:  brfalse.s  IL_0032

    IL_0019:  ldarg.0
    IL_001a:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<!T>::contents@
    IL_001f:  stloc.1
    IL_0020:  ldloc.0
    IL_0021:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<!T>::contents@
    IL_0026:  stloc.2
    IL_0027:  ldarg.2
    IL_0028:  ldloc.1
    IL_0029:  ldloc.2
    IL_002a:  tail.
    IL_002c:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericComparisonWithComparerIntrinsic<!T>(class [mscorlib]System.Collections.IComparer,
                                                                                                                                !!0,
                                                                                                                                !!0)
    IL_0031:  ret

    IL_0032:  ldc.i4.1
    IL_0033:  ret

    IL_0034:  ldarg.1
    IL_0035:  unbox.any  class Microsoft.FSharp.Core.FSharpRef`1<!T>
    IL_003a:  ldnull
    IL_003b:  cgt.un
    IL_003d:  brfalse.s  IL_0041

    IL_003f:  ldc.i4.m1
    IL_0040:  ret

    IL_0041:  ldc.i4.0
    IL_0042:  ret
  } // end of method FSharpRef`1::CompareTo

  .method public hidebysig virtual final
          instance int32  GetHashCode(class [mscorlib]System.Collections.IEqualityComparer comp) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       47 (0x2f)
    .maxstack  5
    .locals init (int32 V_0,
             !T V_1)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldnull
    IL_0003:  cgt.un
    IL_0005:  brfalse.s  IL_002d

    IL_0007:  ldc.i4.0
    IL_0008:  stloc.0
    IL_0009:  ldc.i4     0x9e3779b9
    IL_000e:  ldarg.0
    IL_000f:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<!T>::contents@
    IL_0014:  stloc.1
    IL_0015:  ldarg.1
    IL_0016:  ldloc.1
    IL_0017:  box        !T
    IL_001c:  call       int32 Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericHashParamObj(class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                         object)
    IL_0021:  ldloc.0
    IL_0022:  ldc.i4.6
    IL_0023:  shl
    IL_0024:  ldloc.0
    IL_0025:  ldc.i4.2
    IL_0026:  shr
    IL_0027:  add
    IL_0028:  add
    IL_0029:  add
    IL_002a:  stloc.0
    IL_002b:  ldloc.0
    IL_002c:  ret

    IL_002d:  ldc.i4.0
    IL_002e:  ret
  } // end of method FSharpRef`1::GetHashCode

  .method public hidebysig virtual final
          instance int32  GetHashCode() cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       13 (0xd)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  call       class [mscorlib]System.Collections.IEqualityComparer Microsoft.FSharp.Core.LanguagePrimitives::get_GenericEqualityComparer()
    IL_0007:  callvirt   instance int32 class Microsoft.FSharp.Core.FSharpRef`1<!T>::GetHashCode(class [mscorlib]System.Collections.IEqualityComparer)
    IL_000c:  ret
  } // end of method FSharpRef`1::GetHashCode

  .method public hidebysig virtual final
          instance bool  Equals(object obj,
                                class [mscorlib]System.Collections.IEqualityComparer comp) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       52 (0x34)
    .maxstack  3
    .locals init (class Microsoft.FSharp.Core.FSharpRef`1<!T> V_0,
             !T V_1,
             !T V_2)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldnull
    IL_0003:  cgt.un
    IL_0005:  brfalse.s  IL_002c

    IL_0007:  ldarg.1
    IL_0008:  isinst     class Microsoft.FSharp.Core.FSharpRef`1<!T>
    IL_000d:  stloc.0
    IL_000e:  ldloc.0
    IL_000f:  brfalse.s  IL_002a

    IL_0011:  ldarg.0
    IL_0012:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<!T>::contents@
    IL_0017:  stloc.1
    IL_0018:  ldloc.0
    IL_0019:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<!T>::contents@
    IL_001e:  stloc.2
    IL_001f:  ldarg.2
    IL_0020:  ldloc.1
    IL_0021:  ldloc.2
    IL_0022:  tail.
    IL_0024:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityWithComparerIntrinsic<!T>(class [mscorlib]System.Collections.IEqualityComparer,
                                                                                                                             !!0,
                                                                                                                             !!0)
    IL_0029:  ret

    IL_002a:  ldc.i4.0
    IL_002b:  ret

    IL_002c:  ldarg.1
    IL_002d:  ldnull
    IL_002e:  cgt.un
    IL_0030:  ldc.i4.0
    IL_0031:  ceq
    IL_0033:  ret
  } // end of method FSharpRef`1::Equals

  .method public hidebysig virtual final
          instance bool  Equals(class Microsoft.FSharp.Core.FSharpRef`1<!T> obj) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       47 (0x2f)
    .maxstack  2
    .locals init (!T V_0,
             !T V_1)
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldnull
    IL_0003:  cgt.un
    IL_0005:  brfalse.s  IL_0027

    IL_0007:  ldarg.1
    IL_0008:  ldnull
    IL_0009:  cgt.un
    IL_000b:  brfalse.s  IL_0025

    IL_000d:  ldarg.0
    IL_000e:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<!T>::contents@
    IL_0013:  stloc.0
    IL_0014:  ldarg.1
    IL_0015:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<!T>::contents@
    IL_001a:  stloc.1
    IL_001b:  ldloc.0
    IL_001c:  ldloc.1
    IL_001d:  tail.
    IL_001f:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/HashCompare::GenericEqualityERIntrinsic<!T>(!!0,
                                                                                                                   !!0)
    IL_0024:  ret

    IL_0025:  ldc.i4.0
    IL_0026:  ret

    IL_0027:  ldarg.1
    IL_0028:  ldnull
    IL_0029:  cgt.un
    IL_002b:  ldc.i4.0
    IL_002c:  ceq
    IL_002e:  ret
  } // end of method FSharpRef`1::Equals

  .method public hidebysig virtual final
          instance bool  Equals(object obj) cil managed
  {
    .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
    // Code size       23 (0x17)
    .maxstack  2
    .locals init (class Microsoft.FSharp.Core.FSharpRef`1<!T> V_0)
    IL_0000:  nop
    IL_0001:  ldarg.1
    IL_0002:  isinst     class Microsoft.FSharp.Core.FSharpRef`1<!T>
    IL_0007:  stloc.0
    IL_0008:  ldloc.0
    IL_0009:  brfalse.s  IL_0015

    IL_000b:  ldarg.0
    IL_000c:  ldloc.0
    IL_000d:  tail.
    IL_000f:  callvirt   instance bool class Microsoft.FSharp.Core.FSharpRef`1<!T>::Equals(class Microsoft.FSharp.Core.FSharpRef`1<!0>)
    IL_0014:  ret

    IL_0015:  ldc.i4.0
    IL_0016:  ret
  } // end of method FSharpRef`1::Equals

  .property instance !T contents()
  {
    .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags,
                                                                                   int32) = ( 01 00 04 00 00 00 00 00 00 00 00 00 )
    .set instance void Microsoft.FSharp.Core.FSharpRef`1::set_contents(!T)
  } // end of property FSharpRef`1::contents
} // end of class Microsoft.FSharp.Core.FSharpRef`1

.class public auto ansi serializable sealed Microsoft.FSharp.Core.NoComparisonAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 1C 14 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .method public specialname rtspecialname
          instance void  .ctor() cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ret
  } // end of method NoComparisonAttribute::.ctor

} // end of class Microsoft.FSharp.Core.NoComparisonAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.NoEqualityAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 1C 14 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .method public specialname rtspecialname
          instance void  .ctor() cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ret
  } // end of method NoEqualityAttribute::.ctor

} // end of class Microsoft.FSharp.Core.NoEqualityAttribute

.class public abstract auto ansi sealed Microsoft.FSharp.Core.Operators
       extends [mscorlib]System.Object
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 07 00 00 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.AutoOpenAttribute::.ctor() = ( 01 00 00 00 )
  .class abstract auto ansi sealed nested public OperatorIntrinsics
         extends [mscorlib]System.Object
  {
    .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 07 00 00 00 00 00 )
    .class auto autochar serializable sealed nested assembly beforefieldinit specialname RangeInt32@5421
           extends [mscorlib]System.Object
           implements class [mscorlib]System.Collections.Generic.IEnumerator`1<int32>,
                      [mscorlib]System.Collections.IEnumerator
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
      .field public int32 n
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
      .field public int32 step
      .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      .field public int32 m
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
      .field public class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> state
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
      .method public specialname rtspecialname
              instance void  .ctor(int32 n,
                                   int32 step,
                                   int32 m,
                                   class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> state) cil managed
      {
        // Code size       39 (0x27)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  ldarg.1
        IL_0002:  stfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::n
        IL_0007:  ldarg.0
        IL_0008:  ldarg.2
        IL_0009:  stfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::step
        IL_000e:  ldarg.0
        IL_000f:  ldarg.3
        IL_0010:  stfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::m
        IL_0015:  ldarg.0
        IL_0016:  ldarg.s    state
        IL_0018:  stfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_001d:  nop
        IL_001e:  ldarg.0
        IL_001f:  callvirt   instance void [mscorlib]System.Object::.ctor()
        IL_0024:  ldarg.0
        IL_0025:  pop
        IL_0026:  ret
      } // end of method RangeInt32@5421::.ctor

      .method private hidebysig newslot virtual final
              instance void  'System-IDisposable-Dispose'() cil managed
      {
        .override [mscorlib]System.IDisposable::Dispose
        // Code size       2 (0x2)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ret
      } // end of method RangeInt32@5421::'System-IDisposable-Dispose'

      .method private hidebysig newslot virtual final
              instance int32  'System-Collections-Generic-IEnumerator`1-get_Current'() cil managed
      {
        .override  method instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<int32>::get_Current()
        // Code size       14 (0xe)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0007:  ldnull
        IL_0008:  call       int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics::current@5249(class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>,
                                                                                                    class Microsoft.FSharp.Core.Unit)
        IL_000d:  ret
      } // end of method RangeInt32@5421::'System-Collections-Generic-IEnumerator`1-get_Current'

      .method private hidebysig newslot virtual final
              instance object  'System-Collections-IEnumerator-get_Current'() cil managed
      {
        .override [mscorlib]System.Collections.IEnumerator::get_Current
        // Code size       19 (0x13)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0007:  ldnull
        IL_0008:  call       int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics::current@5249(class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>,
                                                                                                    class Microsoft.FSharp.Core.Unit)
        IL_000d:  box        [mscorlib]System.Int32
        IL_0012:  ret
      } // end of method RangeInt32@5421::'System-Collections-IEnumerator-get_Current'

      .method private hidebysig newslot virtual final
              instance void  'System-Collections-IEnumerator-Reset'() cil managed
      {
        .override [mscorlib]System.Collections.IEnumerator::Reset
        // Code size       38 (0x26)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0006:  ldc.i4.0
        IL_0007:  call       instance void class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::set_Started(bool)
        IL_000c:  ldarg.0
        IL_000d:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0012:  ldc.i4.0
        IL_0013:  call       instance void class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::set_Complete(bool)
        IL_0018:  nop
        IL_0019:  ldarg.0
        IL_001a:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_001f:  ldc.i4.0
        IL_0020:  call       instance void class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::set_Current(!0)
        IL_0025:  ret
      } // end of method RangeInt32@5421::'System-Collections-IEnumerator-Reset'

      .method private hidebysig newslot virtual final
              instance bool  'System-Collections-IEnumerator-MoveNext'() cil managed
      {
        .override [mscorlib]System.Collections.IEnumerator::MoveNext
        // Code size       299 (0x12b)
        .maxstack  3
        .locals init (int32 V_0)
        IL_0000:  ldarg.0
        IL_0001:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0006:  ldfld      bool class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::Started@
        IL_000b:  brtrue     IL_0086

        IL_0010:  ldarg.0
        IL_0011:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0016:  ldc.i4.1
        IL_0017:  call       instance void class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::set_Started(bool)
        IL_001c:  ldarg.0
        IL_001d:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0022:  ldarg.0
        IL_0023:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::n
        IL_0028:  call       instance void class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::set_Current(!0)
        IL_002d:  ldarg.0
        IL_002e:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0033:  ldarg.0
        IL_0034:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::step
        IL_0039:  ldc.i4.0
        IL_003a:  ble.s      IL_0052

        IL_003c:  ldarg.0
        IL_003d:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0042:  ldfld      !0 class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::Current@
        IL_0047:  ldarg.0
        IL_0048:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::m
        IL_004d:  cgt
        IL_004f:  nop
        IL_0050:  br.s       IL_0054

        IL_0052:  ldc.i4.0
        IL_0053:  nop
        IL_0054:  brfalse.s  IL_005a

        IL_0056:  ldc.i4.1
        IL_0057:  nop
        IL_0058:  br.s       IL_007b

        IL_005a:  ldarg.0
        IL_005b:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::step
        IL_0060:  ldc.i4.0
        IL_0061:  bge.s      IL_0079

        IL_0063:  ldarg.0
        IL_0064:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0069:  ldfld      !0 class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::Current@
        IL_006e:  ldarg.0
        IL_006f:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::m
        IL_0074:  clt
        IL_0076:  nop
        IL_0077:  br.s       IL_007b

        IL_0079:  ldc.i4.0
        IL_007a:  nop
        IL_007b:  call       instance void class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::set_Complete(bool)
        IL_0080:  nop
        IL_0081:  br         IL_011b

        IL_0086:  ldarg.0
        IL_0087:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_008c:  ldfld      !0 class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::Current@
        IL_0091:  ldarg.0
        IL_0092:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::step
        IL_0097:  add
        IL_0098:  stloc.0
        IL_0099:  ldarg.0
        IL_009a:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::step
        IL_009f:  ldc.i4.0
        IL_00a0:  ble.s      IL_00b3

        IL_00a2:  ldloc.0
        IL_00a3:  ldarg.0
        IL_00a4:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_00a9:  ldfld      !0 class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::Current@
        IL_00ae:  cgt
        IL_00b0:  nop
        IL_00b1:  br.s       IL_00b5

        IL_00b3:  ldc.i4.0
        IL_00b4:  nop
        IL_00b5:  brfalse.s  IL_00c6

        IL_00b7:  ldloc.0
        IL_00b8:  ldarg.0
        IL_00b9:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::m
        IL_00be:  cgt
        IL_00c0:  ldc.i4.0
        IL_00c1:  ceq
        IL_00c3:  nop
        IL_00c4:  br.s       IL_00c8

        IL_00c6:  ldc.i4.0
        IL_00c7:  nop
        IL_00c8:  brfalse.s  IL_00ce

        IL_00ca:  ldc.i4.1
        IL_00cb:  nop
        IL_00cc:  br.s       IL_00fd

        IL_00ce:  ldarg.0
        IL_00cf:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::step
        IL_00d4:  ldc.i4.0
        IL_00d5:  bge.s      IL_00e8

        IL_00d7:  ldloc.0
        IL_00d8:  ldarg.0
        IL_00d9:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_00de:  ldfld      !0 class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::Current@
        IL_00e3:  clt
        IL_00e5:  nop
        IL_00e6:  br.s       IL_00ea

        IL_00e8:  ldc.i4.0
        IL_00e9:  nop
        IL_00ea:  brfalse.s  IL_00fb

        IL_00ec:  ldloc.0
        IL_00ed:  ldarg.0
        IL_00ee:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::m
        IL_00f3:  clt
        IL_00f5:  ldc.i4.0
        IL_00f6:  ceq
        IL_00f8:  nop
        IL_00f9:  br.s       IL_00fd

        IL_00fb:  ldc.i4.0
        IL_00fc:  nop
        IL_00fd:  brfalse.s  IL_010e

        IL_00ff:  ldarg.0
        IL_0100:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0105:  ldloc.0
        IL_0106:  call       instance void class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::set_Current(!0)
        IL_010b:  nop
        IL_010c:  br.s       IL_011b

        IL_010e:  ldarg.0
        IL_010f:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0114:  ldc.i4.1
        IL_0115:  call       instance void class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::set_Complete(bool)
        IL_011a:  nop
        IL_011b:  nop
        IL_011c:  ldarg.0
        IL_011d:  ldfld      class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::state
        IL_0122:  ldfld      bool class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::Complete@
        IL_0127:  ldc.i4.0
        IL_0128:  ceq
        IL_012a:  ret
      } // end of method RangeInt32@5421::'System-Collections-IEnumerator-MoveNext'

    } // end of class RangeInt32@5421

    .class auto autochar serializable sealed nested assembly beforefieldinit specialname 'RangeInt32@5421-1'
           extends [mscorlib]System.Object
           implements class [mscorlib]System.Collections.Generic.IEnumerable`1<int32>,
                      [mscorlib]System.Collections.IEnumerable
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
      .field public int32 n
      .field public int32 step
      .field public int32 m
      .method public specialname rtspecialname
              instance void  .ctor(int32 n,
                                   int32 step,
                                   int32 m) cil managed
      {
        // Code size       31 (0x1f)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  ldarg.1
        IL_0002:  stfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-1'::n
        IL_0007:  ldarg.0
        IL_0008:  ldarg.2
        IL_0009:  stfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-1'::step
        IL_000e:  ldarg.0
        IL_000f:  ldarg.3
        IL_0010:  stfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-1'::m
        IL_0015:  nop
        IL_0016:  ldarg.0
        IL_0017:  callvirt   instance void [mscorlib]System.Object::.ctor()
        IL_001c:  ldarg.0
        IL_001d:  pop
        IL_001e:  ret
      } // end of method 'RangeInt32@5421-1'::.ctor

      .method private hidebysig newslot virtual final
              instance class [mscorlib]System.Collections.Generic.IEnumerator`1<int32>
              'System-Collections-Generic-IEnumerable`1-GetEnumerator'() cil managed
      {
        .override  method instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<int32>::GetEnumerator()
        // Code size       26 (0x1a)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-1'::n
        IL_0007:  ldarg.0
        IL_0008:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-1'::step
        IL_000d:  ldarg.0
        IL_000e:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-1'::m
        IL_0013:  ldnull
        IL_0014:  call       class [mscorlib]System.Collections.Generic.IEnumerator`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics::variableStepRangeEnumerator@5242(int32,
                                                                                                                                                                                  int32,
                                                                                                                                                                                  int32,
                                                                                                                                                                                  class Microsoft.FSharp.Core.Unit)
        IL_0019:  ret
      } // end of method 'RangeInt32@5421-1'::'System-Collections-Generic-IEnumerable`1-GetEnumerator'

      .method private hidebysig newslot virtual final
              instance class [mscorlib]System.Collections.IEnumerator
              'System-Collections-IEnumerable-GetEnumerator'() cil managed
      {
        .override [mscorlib]System.Collections.IEnumerable::GetEnumerator
        // Code size       26 (0x1a)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-1'::n
        IL_0007:  ldarg.0
        IL_0008:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-1'::step
        IL_000d:  ldarg.0
        IL_000e:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-1'::m
        IL_0013:  ldnull
        IL_0014:  call       class [mscorlib]System.Collections.Generic.IEnumerator`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics::variableStepRangeEnumerator@5242(int32,
                                                                                                                                                                                  int32,
                                                                                                                                                                                  int32,
                                                                                                                                                                                  class Microsoft.FSharp.Core.Unit)
        IL_0019:  ret
      } // end of method 'RangeInt32@5421-1'::'System-Collections-IEnumerable-GetEnumerator'

    } // end of class 'RangeInt32@5421-1'

    .class auto autochar serializable sealed nested assembly beforefieldinit specialname 'RangeInt32@5421-2'
           extends [mscorlib]System.Object
           implements [mscorlib]System.Collections.IEnumerator,
                      class [mscorlib]System.Collections.Generic.IEnumerator`1<int32>
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
      .field public int32 n
      .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
      .field public int32 m
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
      .field public class Microsoft.FSharp.Core.FSharpRef`1<int32> 'value'
      .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
      .custom instance void [mscorlib]System.Diagnostics.DebuggerNonUserCodeAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      .method public specialname rtspecialname
              instance void  .ctor(int32 n,
                                   int32 m,
                                   class Microsoft.FSharp.Core.FSharpRef`1<int32> 'value') cil managed
      {
        // Code size       31 (0x1f)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  ldarg.1
        IL_0002:  stfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::n
        IL_0007:  ldarg.0
        IL_0008:  ldarg.2
        IL_0009:  stfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::m
        IL_000e:  ldarg.0
        IL_000f:  ldarg.3
        IL_0010:  stfld      class Microsoft.FSharp.Core.FSharpRef`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::'value'
        IL_0015:  nop
        IL_0016:  ldarg.0
        IL_0017:  callvirt   instance void [mscorlib]System.Object::.ctor()
        IL_001c:  ldarg.0
        IL_001d:  pop
        IL_001e:  ret
      } // end of method 'RangeInt32@5421-2'::.ctor

      .method private hidebysig newslot virtual final
              instance void  'System-IDisposable-Dispose'() cil managed
      {
        .override [mscorlib]System.IDisposable::Dispose
        // Code size       2 (0x2)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ret
      } // end of method 'RangeInt32@5421-2'::'System-IDisposable-Dispose'

      .method private hidebysig newslot virtual final
              instance int32  'System-Collections-Generic-IEnumerator`1-get_Current'() cil managed
      {
        .override  method instance !0 class [mscorlib]System.Collections.Generic.IEnumerator`1<int32>::get_Current()
        // Code size       45 (0x2d)
        .maxstack  2
        .locals init (int32 V_0)
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      class Microsoft.FSharp.Core.FSharpRef`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::'value'
        IL_0007:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<int32>::contents@
        IL_000c:  stloc.0
        IL_000d:  ldloc.0
        IL_000e:  ldarg.0
        IL_000f:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::n
        IL_0014:  bge.s      IL_001c

        IL_0016:  call       !!0 Microsoft.FSharp.Core.Operators/OperatorIntrinsics::notStarted<int32>()
        IL_001b:  ret

        IL_001c:  ldloc.0
        IL_001d:  ldarg.0
        IL_001e:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::m
        IL_0023:  ble.s      IL_002b

        IL_0025:  call       !!0 Microsoft.FSharp.Core.Operators/OperatorIntrinsics::alreadyFinished<int32>()
        IL_002a:  ret

        IL_002b:  ldloc.0
        IL_002c:  ret
      } // end of method 'RangeInt32@5421-2'::'System-Collections-Generic-IEnumerator`1-get_Current'

      .method private hidebysig newslot virtual final
              instance object  'System-Collections-IEnumerator-get_Current'() cil managed
      {
        .override [mscorlib]System.Collections.IEnumerator::get_Current
        // Code size       55 (0x37)
        .maxstack  2
        .locals init (int32 V_0)
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      class Microsoft.FSharp.Core.FSharpRef`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::'value'
        IL_0007:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<int32>::contents@
        IL_000c:  stloc.0
        IL_000d:  ldloc.0
        IL_000e:  ldarg.0
        IL_000f:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::n
        IL_0014:  bge.s      IL_001e

        IL_0016:  call       !!0 Microsoft.FSharp.Core.Operators/OperatorIntrinsics::notStarted<int32>()
        IL_001b:  nop
        IL_001c:  br.s       IL_0031

        IL_001e:  ldloc.0
        IL_001f:  ldarg.0
        IL_0020:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::m
        IL_0025:  ble.s      IL_002f

        IL_0027:  call       !!0 Microsoft.FSharp.Core.Operators/OperatorIntrinsics::alreadyFinished<int32>()
        IL_002c:  nop
        IL_002d:  br.s       IL_0031

        IL_002f:  ldloc.0
        IL_0030:  nop
        IL_0031:  box        [mscorlib]System.Int32
        IL_0036:  ret
      } // end of method 'RangeInt32@5421-2'::'System-Collections-IEnumerator-get_Current'

      .method private hidebysig newslot virtual final
              instance void  'System-Collections-IEnumerator-Reset'() cil managed
      {
        .override [mscorlib]System.Collections.IEnumerator::Reset
        // Code size       21 (0x15)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      class Microsoft.FSharp.Core.FSharpRef`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::'value'
        IL_0007:  ldarg.0
        IL_0008:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::n
        IL_000d:  ldc.i4.1
        IL_000e:  sub
        IL_000f:  call       instance void class Microsoft.FSharp.Core.FSharpRef`1<int32>::set_contents(!0)
        IL_0014:  ret
      } // end of method 'RangeInt32@5421-2'::'System-Collections-IEnumerator-Reset'

      .method private hidebysig newslot virtual final
              instance bool  'System-Collections-IEnumerator-MoveNext'() cil managed
      {
        .override [mscorlib]System.Collections.IEnumerator::MoveNext
        // Code size       65 (0x41)
        .maxstack  3
        .locals init (int32 V_0)
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      class Microsoft.FSharp.Core.FSharpRef`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::'value'
        IL_0007:  ldfld      !0 class Microsoft.FSharp.Core.FSharpRef`1<int32>::contents@
        IL_000c:  stloc.0
        IL_000d:  ldloc.0
        IL_000e:  ldarg.0
        IL_000f:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::m
        IL_0014:  bge.s      IL_0026

        IL_0016:  ldarg.0
        IL_0017:  ldfld      class Microsoft.FSharp.Core.FSharpRef`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::'value'
        IL_001c:  ldloc.0
        IL_001d:  ldc.i4.1
        IL_001e:  add
        IL_001f:  call       instance void class Microsoft.FSharp.Core.FSharpRef`1<int32>::set_contents(!0)
        IL_0024:  ldc.i4.1
        IL_0025:  ret

        IL_0026:  ldloc.0
        IL_0027:  ldarg.0
        IL_0028:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::m
        IL_002d:  bne.un.s   IL_003f

        IL_002f:  ldarg.0
        IL_0030:  ldfld      class Microsoft.FSharp.Core.FSharpRef`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::'value'
        IL_0035:  ldloc.0
        IL_0036:  ldc.i4.1
        IL_0037:  add
        IL_0038:  call       instance void class Microsoft.FSharp.Core.FSharpRef`1<int32>::set_contents(!0)
        IL_003d:  ldc.i4.0
        IL_003e:  ret

        IL_003f:  ldc.i4.0
        IL_0040:  ret
      } // end of method 'RangeInt32@5421-2'::'System-Collections-IEnumerator-MoveNext'

    } // end of class 'RangeInt32@5421-2'

    .class auto autochar serializable sealed nested assembly beforefieldinit specialname 'RangeInt32@5421-3'
           extends [mscorlib]System.Object
           implements class [mscorlib]System.Collections.Generic.IEnumerable`1<int32>,
                      [mscorlib]System.Collections.IEnumerable
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 06 00 00 00 00 00 )
      .field public int32 n
      .field public int32 m
      .method public specialname rtspecialname
              instance void  .ctor(int32 n,
                                   int32 m) cil managed
      {
        // Code size       24 (0x18)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  ldarg.1
        IL_0002:  stfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-3'::n
        IL_0007:  ldarg.0
        IL_0008:  ldarg.2
        IL_0009:  stfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-3'::m
        IL_000e:  nop
        IL_000f:  ldarg.0
        IL_0010:  callvirt   instance void [mscorlib]System.Object::.ctor()
        IL_0015:  ldarg.0
        IL_0016:  pop
        IL_0017:  ret
      } // end of method 'RangeInt32@5421-3'::.ctor

      .method private hidebysig newslot virtual final
              instance class [mscorlib]System.Collections.Generic.IEnumerator`1<int32>
              'System-Collections-Generic-IEnumerable`1-GetEnumerator'() cil managed
      {
        .override  method instance class [mscorlib]System.Collections.Generic.IEnumerator`1<!0> class [mscorlib]System.Collections.Generic.IEnumerable`1<int32>::GetEnumerator()
        // Code size       20 (0x14)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-3'::n
        IL_0007:  ldarg.0
        IL_0008:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-3'::m
        IL_000d:  ldnull
        IL_000e:  call       class [mscorlib]System.Collections.Generic.IEnumerator`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics::singleStepRangeEnumerator@5303(int32,
                                                                                                                                                                                int32,
                                                                                                                                                                                class Microsoft.FSharp.Core.Unit)
        IL_0013:  ret
      } // end of method 'RangeInt32@5421-3'::'System-Collections-Generic-IEnumerable`1-GetEnumerator'

      .method private hidebysig newslot virtual final
              instance class [mscorlib]System.Collections.IEnumerator
              'System-Collections-IEnumerable-GetEnumerator'() cil managed
      {
        .override [mscorlib]System.Collections.IEnumerable::GetEnumerator
        // Code size       20 (0x14)
        .maxstack  8
        IL_0000:  nop
        IL_0001:  ldarg.0
        IL_0002:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-3'::n
        IL_0007:  ldarg.0
        IL_0008:  ldfld      int32 Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-3'::m
        IL_000d:  ldnull
        IL_000e:  call       class [mscorlib]System.Collections.Generic.IEnumerator`1<int32> Microsoft.FSharp.Core.Operators/OperatorIntrinsics::singleStepRangeEnumerator@5303(int32,
                                                                                                                                                                                int32,
                                                                                                                                                                                class Microsoft.FSharp.Core.Unit)
        IL_0013:  ret
      } // end of method 'RangeInt32@5421-3'::'System-Collections-IEnumerable-GetEnumerator'

    } // end of class 'RangeInt32@5421-3'

    .class auto ansi serializable sealed nested assembly beforefieldinit VariableStepIntegralRangeState`1<T>
           extends [mscorlib]System.Object
    {
      .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 22 00 00 00 00 00 )                         // ..".....
      .custom instance void Microsoft.FSharp.Core.NoEqualityAttribute::.ctor() = ( 01 00 00 00 )
      .custom instance void Microsoft.FSharp.Core.NoComparisonAttribute::.ctor() = ( 01 00 00 00 )
      .field assembly bool Started@
      .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
      .field assembly bool Complete@
      .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
      .field assembly !T Current@
      .custom instance void [mscorlib]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggerBrowsableState) = ( 01 00 00 00 00 00 00 00 )
      .method assembly hidebysig specialname
              instance void  set_Started(bool 'value') cil managed
      {
        // Code size       8 (0x8)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  ldarg.1
        IL_0002:  stfld      bool class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<!T>::Started@
        IL_0007:  ret
      } // end of method VariableStepIntegralRangeState`1::set_Started

      .method assembly hidebysig specialname
              instance void  set_Complete(bool 'value') cil managed
      {
        // Code size       8 (0x8)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  ldarg.1
        IL_0002:  stfld      bool class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<!T>::Complete@
        IL_0007:  ret
      } // end of method VariableStepIntegralRangeState`1::set_Complete

      .method assembly hidebysig specialname
              instance void  set_Current(!T 'value') cil managed
      {
        // Code size       8 (0x8)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  ldarg.1
        IL_0002:  stfld      !0 class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<!T>::Current@
        IL_0007:  ret
      } // end of method VariableStepIntegralRangeState`1::set_Current

      .method assembly specialname rtspecialname
              instance void  .ctor(bool started,
                                   bool complete,
                                   !T current) cil managed
      {
        // Code size       28 (0x1c)
        .maxstack  8
        IL_0000:  ldarg.0
        IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
        IL_0006:  ldarg.0
        IL_0007:  ldarg.1
        IL_0008:  stfld      bool class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<!T>::Started@
        IL_000d:  ldarg.0
        IL_000e:  ldarg.2
        IL_000f:  stfld      bool class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<!T>::Complete@
        IL_0014:  ldarg.0
        IL_0015:  ldarg.3
        IL_0016:  stfld      !0 class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<!T>::Current@
        IL_001b:  ret
      } // end of method VariableStepIntegralRangeState`1::.ctor

      .property instance bool Started()
      {
        .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags,
                                                                                       int32) = ( 01 00 04 00 00 00 00 00 00 00 00 00 )
        .set instance void Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1::set_Started(bool)
      } // end of property VariableStepIntegralRangeState`1::Started
      .property instance bool Complete()
      {
        .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags,
                                                                                       int32) = ( 01 00 04 00 00 00 01 00 00 00 00 00 )
        .set instance void Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1::set_Complete(bool)
      } // end of property VariableStepIntegralRangeState`1::Complete
      .property instance !T Current()
      {
        .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags,
                                                                                       int32) = ( 01 00 04 00 00 00 02 00 00 00 00 00 )
        .set instance void Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1::set_Current(!T)
      } // end of property VariableStepIntegralRangeState`1::Current
    } // end of class VariableStepIntegralRangeState`1

    .method public static class [mscorlib]System.Collections.Generic.IEnumerable`1<int32>
            RangeInt32(int32 start,
                       int32 step,
                       int32 stop) cil managed
    {
      .custom instance void Microsoft.FSharp.Core.CompilerMessageAttribute::.ctor(string,
                                                                                  int32) = ( 01 00 4C 54 68 69 73 20 66 75 6E 63 74 69 6F 6E   // ..LThis function
                                                                                             20 69 73 20 66 6F 72 20 75 73 65 20 62 79 20 63   //  is for use by c
                                                                                             6F 6D 70 69 6C 65 64 20 46 23 20 63 6F 64 65 20   // ompiled F# code
                                                                                             61 6E 64 20 73 68 6F 75 6C 64 20 6E 6F 74 20 62   // and should not b
                                                                                             65 20 75 73 65 64 20 64 69 72 65 63 74 6C 79 B4   // e used directly.
                                                                                             04 00 00 01 00 54 02 08 49 73 48 69 64 64 65 6E   // .....T..IsHidden
                                                                                             01 )
      .custom instance void Microsoft.FSharp.Core.CompilationArgumentCountsAttribute::.ctor(int32[]) = ( 01 00 03 00 00 00 01 00 00 00 01 00 00 00 01 00
                                                                                                         00 00 00 00 )
      // Code size       85 (0x55)
      .maxstack  3
      .locals init (string V_0)
      IL_0000:  nop
      IL_0001:  ldarg.1
      IL_0002:  ldc.i4.1
      IL_0003:  beq.s      IL_0009

      IL_0005:  ldc.i4.1
      IL_0006:  nop
      IL_0007:  br.s       IL_000e

      IL_0009:  ldarg.0
      IL_000a:  ldarg.2
      IL_000b:  cgt
      IL_000d:  nop
      IL_000e:  brfalse.s  IL_0014

      IL_0010:  ldc.i4.1
      IL_0011:  nop
      IL_0012:  br.s       IL_001d

      IL_0014:  ldarg.0
      IL_0015:  ldc.i4     0x80000000
      IL_001a:  ceq
      IL_001c:  nop
      IL_001d:  brfalse.s  IL_0023

      IL_001f:  ldc.i4.1
      IL_0020:  nop
      IL_0021:  br.s       IL_002c

      IL_0023:  ldarg.2
      IL_0024:  ldc.i4     0x7fffffff
      IL_0029:  ceq
      IL_002b:  nop
      IL_002c:  brfalse.s  IL_004d

      IL_002e:  ldarg.1
      IL_002f:  brtrue.s   IL_0043

      IL_0031:  ldstr      "stepCannotBeZero"
      IL_0036:  stloc.0
      IL_0037:  ldloc.0
      IL_0038:  ldstr      "step"
      IL_003d:  newobj     instance void [mscorlib]System.ArgumentException::.ctor(string,
                                                                                   string)
      IL_0042:  throw

      IL_0043:  nop
      IL_0044:  ldarg.0
      IL_0045:  ldarg.1
      IL_0046:  ldarg.2
      IL_0047:  newobj     instance void Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-1'::.ctor(int32,
                                                                                                                       int32,
                                                                                                                       int32)
      IL_004c:  ret

      IL_004d:  ldarg.0
      IL_004e:  ldarg.2
      IL_004f:  newobj     instance void Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-3'::.ctor(int32,
                                                                                                                       int32)
      IL_0054:  ret
    } // end of method OperatorIntrinsics::RangeInt32

    .method assembly static !!a  notStarted<a>() cil managed
    {
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldstr      "enumerationNotStarted"
      IL_0006:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor(string)
      IL_000b:  throw
    } // end of method OperatorIntrinsics::notStarted

    .method assembly static !!a  alreadyFinished<a>() cil managed
    {
      // Code size       12 (0xc)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldstr      "enumerationAlreadyFinished"
      IL_0006:  newobj     instance void [mscorlib]System.InvalidOperationException::.ctor(string)
      IL_000b:  throw
    } // end of method OperatorIntrinsics::alreadyFinished

    .method assembly static int32  current@5249(class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> state,
                                                class Microsoft.FSharp.Core.Unit unitVar0) cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      // Code size       36 (0x24)
      .maxstack  8
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldfld      bool class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::Started@
      IL_0007:  brtrue.s   IL_000f

      IL_0009:  call       !!0 Microsoft.FSharp.Core.Operators/OperatorIntrinsics::notStarted<int32>()
      IL_000e:  ret

      IL_000f:  ldarg.0
      IL_0010:  ldfld      bool class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::Complete@
      IL_0015:  brfalse.s  IL_001d

      IL_0017:  call       !!0 Microsoft.FSharp.Core.Operators/OperatorIntrinsics::alreadyFinished<int32>()
      IL_001c:  ret

      IL_001d:  ldarg.0
      IL_001e:  ldfld      !0 class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::Current@
      IL_0023:  ret
    } // end of method OperatorIntrinsics::current@5249

    .method assembly static class [mscorlib]System.Collections.Generic.IEnumerator`1<int32>
            variableStepRangeEnumerator@5242(int32 n,
                                             int32 step,
                                             int32 m,
                                             class Microsoft.FSharp.Core.Unit unitVar0) cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      // Code size       20 (0x14)
      .maxstack  4
      .locals init (class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32> V_0)
      IL_0000:  nop
      IL_0001:  ldc.i4.0
      IL_0002:  ldc.i4.0
      IL_0003:  ldc.i4.0
      IL_0004:  newobj     instance void class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>::.ctor(bool,
                                                                                                                                                 bool,
                                                                                                                                                 !0)
      IL_0009:  stloc.0
      IL_000a:  ldarg.0
      IL_000b:  ldarg.1
      IL_000c:  ldarg.2
      IL_000d:  ldloc.0
      IL_000e:  newobj     instance void Microsoft.FSharp.Core.Operators/OperatorIntrinsics/RangeInt32@5421::.ctor(int32,
                                                                                                                   int32,
                                                                                                                   int32,
                                                                                                                   class Microsoft.FSharp.Core.Operators/OperatorIntrinsics/VariableStepIntegralRangeState`1<int32>)
      IL_0013:  ret
    } // end of method OperatorIntrinsics::variableStepRangeEnumerator@5242

    .method assembly static class [mscorlib]System.Collections.Generic.IEnumerator`1<int32>
            singleStepRangeEnumerator@5303(int32 n,
                                           int32 m,
                                           class Microsoft.FSharp.Core.Unit unitVar0) cil managed
    {
      .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor() = ( 01 00 00 00 )
      // Code size       19 (0x13)
      .maxstack  3
      .locals init (class Microsoft.FSharp.Core.FSharpRef`1<int32> V_0)
      IL_0000:  nop
      IL_0001:  ldarg.0
      IL_0002:  ldc.i4.1
      IL_0003:  sub
      IL_0004:  newobj     instance void class Microsoft.FSharp.Core.FSharpRef`1<int32>::.ctor(!0)
      IL_0009:  stloc.0
      IL_000a:  ldarg.0
      IL_000b:  ldarg.1
      IL_000c:  ldloc.0
      IL_000d:  newobj     instance void Microsoft.FSharp.Core.Operators/OperatorIntrinsics/'RangeInt32@5421-2'::.ctor(int32,
                                                                                                                       int32,
                                                                                                                       class Microsoft.FSharp.Core.FSharpRef`1<int32>)
      IL_0012:  ret
    } // end of method OperatorIntrinsics::singleStepRangeEnumerator@5303

  } // end of class OperatorIntrinsics

  .method public static !!T  Exit<T>(int32 exitcode) cil managed
  {
    .custom instance void Microsoft.FSharp.Core.CompilationSourceNameAttribute::.ctor(string) = ( 01 00 04 65 78 69 74 00 00 )                      // ...exit..
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  call       void [mscorlib]System.Environment::Exit(int32)
    IL_0007:  ldstr      "System.Environment.Exit did not exit!"
    IL_000c:  newobj     instance void [mscorlib]System.Exception::.ctor(string)
    IL_0011:  throw
  } // end of method Operators::Exit

} // end of class Microsoft.FSharp.Core.Operators

.class public auto ansi serializable sealed Microsoft.FSharp.Core.RequireQualifiedAccessAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 04 00 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .method public specialname rtspecialname
          instance void  .ctor() cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ret
  } // end of method RequireQualifiedAccessAttribute::.ctor

} // end of class Microsoft.FSharp.Core.RequireQualifiedAccessAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.SealedAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 04 00 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .field assembly bool 'value'
  .method public specialname rtspecialname
          instance void  .ctor() cil managed
  {
    // Code size       9 (0x9)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  ldc.i4.1
    IL_0003:  callvirt   instance void Microsoft.FSharp.Core.SealedAttribute::.ctor(bool)
    IL_0008:  ret
  } // end of method SealedAttribute::.ctor

  .method public specialname rtspecialname
          instance void  .ctor(bool 'value') cil managed
  {
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ldarg.0
    IL_000a:  ldarg.1
    IL_000b:  stfld      bool Microsoft.FSharp.Core.SealedAttribute::'value'
    IL_0010:  ret
  } // end of method SealedAttribute::.ctor

} // end of class Microsoft.FSharp.Core.SealedAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.SourceConstructFlags
       extends [mscorlib]System.Enum
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .field public specialname rtspecialname int32 value__
  .field public static literal valuetype Microsoft.FSharp.Core.SourceConstructFlags None = int32(0x00000000)
  .field public static literal valuetype Microsoft.FSharp.Core.SourceConstructFlags SumType = int32(0x00000001)
  .field public static literal valuetype Microsoft.FSharp.Core.SourceConstructFlags RecordType = int32(0x00000002)
  .field public static literal valuetype Microsoft.FSharp.Core.SourceConstructFlags ObjectType = int32(0x00000003)
  .field public static literal valuetype Microsoft.FSharp.Core.SourceConstructFlags Field = int32(0x00000004)
  .field public static literal valuetype Microsoft.FSharp.Core.SourceConstructFlags Exception = int32(0x00000005)
  .field public static literal valuetype Microsoft.FSharp.Core.SourceConstructFlags Closure = int32(0x00000006)
  .field public static literal valuetype Microsoft.FSharp.Core.SourceConstructFlags Module = int32(0x00000007)
  .field public static literal valuetype Microsoft.FSharp.Core.SourceConstructFlags UnionCase = int32(0x00000008)
  .field public static literal valuetype Microsoft.FSharp.Core.SourceConstructFlags Value = int32(0x00000009)
  .field public static literal valuetype Microsoft.FSharp.Core.SourceConstructFlags KindMask = int32(0x0000001F)
  .field public static literal valuetype Microsoft.FSharp.Core.SourceConstructFlags NonPublicRepresentation = int32(0x00000020)
} // end of class Microsoft.FSharp.Core.SourceConstructFlags

.class public auto ansi serializable sealed Microsoft.FSharp.Core.StructuralComparisonAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 04 00 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .method public specialname rtspecialname
          instance void  .ctor() cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ret
  } // end of method StructuralComparisonAttribute::.ctor

} // end of class Microsoft.FSharp.Core.StructuralComparisonAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.StructuralEqualityAttribute
       extends [mscorlib]System.Attribute
{
  .custom instance void [mscorlib]System.AttributeUsageAttribute::.ctor(valuetype [mscorlib]System.AttributeTargets) = ( 01 00 04 00 00 00 01 00 54 02 0D 41 6C 6C 6F 77   // ........T..Allow
                                                                                                                         4D 75 6C 74 69 70 6C 65 00 )                      // Multiple.
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .custom instance void Microsoft.FSharp.Core.SealedAttribute::.ctor() = ( 01 00 00 00 )
  .method public specialname rtspecialname
          instance void  .ctor() cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldarg.0
    IL_0002:  callvirt   instance void [mscorlib]System.Attribute::.ctor()
    IL_0007:  ldarg.0
    IL_0008:  pop
    IL_0009:  ret
  } // end of method StructuralEqualityAttribute::.ctor

} // end of class Microsoft.FSharp.Core.StructuralEqualityAttribute

.class public auto ansi serializable sealed Microsoft.FSharp.Core.Unit
       extends [mscorlib]System.Object
       implements [mscorlib]System.IComparable
{
  .custom instance void Microsoft.FSharp.Core.CompilationMappingAttribute::.ctor(valuetype Microsoft.FSharp.Core.SourceConstructFlags) = ( 01 00 03 00 00 00 00 00 )
  .method assembly specialname rtspecialname
          instance void  .ctor() cil managed
  {
    // Code size       10 (0xa)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  callvirt   instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ldarg.0
    IL_0007:  pop
    IL_0008:  nop
    IL_0009:  ret
  } // end of method Unit::.ctor

  .method public hidebysig virtual instance int32
          GetHashCode() cil managed
  {
    // Code size       3 (0x3)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldc.i4.0
    IL_0002:  ret
  } // end of method Unit::GetHashCode

  .method public hidebysig virtual instance bool
          Equals(object obj) cil managed
  {
    // Code size       18 (0x12)
    .maxstack  8
    IL_0000:  ldarg.1
    IL_0001:  brfalse.s  IL_0010

    IL_0003:  ldarg.1
    IL_0004:  call       bool Microsoft.FSharp.Core.LanguagePrimitives/IntrinsicFunctions::TypeTestGeneric<class Microsoft.FSharp.Core.Unit>(object)
    IL_0009:  brtrue.s   IL_000e

    IL_000b:  nop
    IL_000c:  ldc.i4.0
    IL_000d:  ret

    IL_000e:  ldc.i4.1
    IL_000f:  ret

    IL_0010:  ldc.i4.1
    IL_0011:  ret
  } // end of method Unit::Equals

  .method private hidebysig newslot virtual
          instance int32  'System-IComparable-CompareTo'(object _obj) cil managed
  {
    .override [mscorlib]System.IComparable::CompareTo
    // Code size       3 (0x3)
    .maxstack  8
    IL_0000:  nop
    IL_0001:  ldc.i4.0
    IL_0002:  ret
  } // end of method Unit::'System-IComparable-CompareTo'

} // end of class Microsoft.FSharp.Core.Unit
