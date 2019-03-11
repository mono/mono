## Before:

Mac Desktop (unlinked):

```
-rw-r--r--  1 mabaul  wheel  4605440 Mar 11 17:33 mscorlib.dll
```

Mac Desktop (linked):

```
-rw-r--r--  1 mabaul  wheel   1328640 Mar 11 17:39 mscorlib.dll
```

Web Assembly (unlinked):

```
-rw-r--r--  1 mabaul  wheel  4427264 Mar 11 17:44 mscorlib.dll
```

Web Assembly (linked):

```
-rw-r--r--  1 mabaul  wheel    928768 Mar 11 17:45 mscorlib.dll
```

Size report (wasm):

```
NS: System.Security.Policy 7
NS: Microsoft.Win32 25
NS: Internal.Runtime.Augments 31
NS: System.Runtime.ConstrainedExecution 38
NS: System.Numerics.Hashing 39
NS: System.Diagnostics.Tracing 57
NS: System.Resources 63
NS: System.Diagnostics.Contracts 81
NS: System.Runtime.Remoting.Contexts 89
NS: Microsoft.Win32.SafeHandles 108
NS: System.Security 117
NS: System.Runtime.ExceptionServices 128
NS: System.Buffers.Text 279
NS: Mono 691
NS: System.Runtime.Remoting.Messaging 917
NS: System.Runtime.Serialization 1409
NS: System.Runtime.CompilerServices 1528
NS: System.Buffers 1673
NS: System.Runtime.InteropServices 1756
NS: System.Diagnostics 2019
NS: System.Collections 2642
NS: System.Collections.Concurrent 2946
NS: System.Threading 3885
NS: System.Reflection.Emit 4139
NS:  6802
NS: System.IO 7194
NS: System.Collections.Generic 7722
NS: System.Reflection 9469
NS: System.Text 10888
NS: Mono.Globalization.Unicode 12715
NS: System.Numerics 15259
NS: System.Globalization 28176
NS: System 79776
```

With `--exclude-feature sre`:

```
NS: System.Reflection.Emit 3230
-rw-r--r--  1 mabaul  wheel   924160 Mar 11 17:47 mscorlib.dll
```

## Reflection Emit Gate

Desktop (unlinked):

```
-rw-r--r--  1 mabaul  wheel  4606464 Mar 11 17:56 mscorlib.dll
```

Desktop (linked);

```
-rw-r--r--  1 mabaul  wheel  1307648 Mar 11 17:56 mscorlib.dll
```

Web Assembly (unlinked):

```
-rw-r--r--  1 mabaul  wheel  4427776 Mar 11 17:56 mscorlib.dll
```

Web Assembly (linked):

```
-rw-r--r--  1 mabaul  wheel   907776 Mar 11 17:58 mscorlib.dll
```

## Remoting Gate (experimental)

Desktop (unlinked):

```
-rw-r--r--  1 mabaul  wheel  4607488 Mar 11 18:05 mscorlib.dll
```

Desktop (linked):

```
-rw-r--r--  1 mabaul  wheel   1100288 Mar 11 18:06 mscorlib.dll
```

Web Assembly (unlinked):

```
-rw-r--r--  1 mabaul  wheel  4428800 Mar 11 18:05 mscorlib.dll
```

Web Assembly (linked):

```
-rw-r--r--  1 mabaul  wheel   898560 Mar 11 18:07 mscorlib.dll
```

Size report:

```
NS: System.Security.Policy 7
NS: Microsoft.Win32 25
NS: Internal.Runtime.Augments 31
NS: System.Runtime.ConstrainedExecution 38
NS: System.Numerics.Hashing 39
NS: System.Diagnostics.Tracing 57
NS: System.Resources 63
NS: System.Diagnostics.Contracts 81
NS: System.Runtime.Remoting.Contexts 89
NS: Microsoft.Win32.SafeHandles 108
NS: System.Security 117
NS: System.Runtime.ExceptionServices 128
NS: System.Runtime.Remoting.Messaging 229
NS: System.Buffers.Text 279
NS: System.Collections 445
NS: Mono 738
NS: System.Runtime.Serialization 1409
NS: System.Runtime.CompilerServices 1499
NS: System.Buffers 1673
NS: System.Runtime.InteropServices 1756
NS: System.Diagnostics 2019
NS: System.Collections.Concurrent 2946
NS: System.Threading 3778
NS:  6342
NS: System.IO 7194
NS: System.Collections.Generic 7631
NS: System.Reflection 9270
NS: System.Text 10869
NS: Mono.Globalization.Unicode 12715
NS: System.Numerics 15259
NS: System.Globalization 27949
NS: System 79492

TYPE: System.Security.SecurityManager 1
TYPE: Locale 2
TYPE: System.NotImplemented 6
TYPE: X 7
TYPE: Interop 8
TYPE: System.Threading.ManualResetEvent 9
TYPE: System.ByReference`1 12
TYPE: System.AssemblyLoadEventArgs 14
TYPE: System.Text.EncoderNLS 15
TYPE: Mono.Globalization.Unicode.SimpleCollator/PreviousInfo 16
TYPE: System.Threading.ThreadPoolGlobals 17
TYPE: System.Reflection.Missing 18
TYPE: System.Globalization.GlobalizationMode 19
TYPE: Mono.Globalization.Unicode.Level2Map 21
TYPE: System.ReflectionOnlyType 23
TYPE: System.InvalidTimeZoneException 24
TYPE: Microsoft.Win32.Win32Native 25
TYPE: System.DefaultBinder/<>c 26
TYPE: Microsoft.Win32.SafeHandles.SafeWaitHandle 28
TYPE: SR 29
TYPE: System.Collections.Concurrent.ConcurrentDictionary`2/Tables 30
TYPE: Internal.Runtime.Augments.RuntimeThread 31
TYPE: Mono.Globalization.Unicode.MSCompatUnicodeTable/<>c 32
TYPE: System.Threading.ThreadAbortException 33
TYPE: Microsoft.Win32.SafeHandles.SafeFileHandle 34
TYPE: System.ResolveEventArgs 35
TYPE: Mono.Globalization.Unicode.TailoringInfo 36
TYPE: System.Threading.NativeEventCalls 38
TYPE: System.Reflection.RuntimeAssembly/UnmanagedMemoryStreamForModule 39
TYPE: System.OrdinalCaseSensitiveComparer 40
TYPE: System.OrdinalIgnoreCaseComparer 42
TYPE: <PrivateImplementationDetails> 44
TYPE: Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid 46
TYPE: Mono.RemotingGate 47
TYPE: System.Reflection.ManifestResourceInfo 49
TYPE: Mono.SafeGPtrArrayHandle 50
TYPE: System.ArithmeticException 51
TYPE: System.Diagnostics.DebuggerDisplayAttribute 52
TYPE: Mono.Globalization.Unicode.SimpleCollator/Context 53
TYPE: System.Runtime.Serialization.SerializationEventsCache 54
TYPE: System.TypedReference 55
TYPE: System.Reflection.RuntimeModule 56
TYPE: System.Diagnostics.Tracing.EventSource 57
TYPE: System.Reflection.PropertyInfo 58
TYPE: Mono.Globalization.Unicode.CodePointIndexer/TableRange 60
TYPE: System.Runtime.Serialization.SerializationException 62
TYPE: System.ObsoleteAttribute 63
TYPE: System.DBNull 65
TYPE: System.Runtime.Serialization.StreamingContext 66
TYPE: System.Text.EncoderFallback 67
TYPE: System.Threading.ExecutionContext/Reader 69
TYPE: System.ApplicationException 71
TYPE: System.Text.InternalEncoderBestFitFallback 72
TYPE: System.Threading.LazyInitializer 73
TYPE: System.IO.Directory 75
TYPE: System.ArgumentNullException 76
TYPE: System.Collections.Generic.InternalStringComparer 77
TYPE: System.AttributeUsageAttribute 78
TYPE: System.GC 79
TYPE: System.Threading.WasmRuntime 80
TYPE: System.Text.StringBuilderCache 81
TYPE: System.Text.Encoder 82
TYPE: System.Runtime.CompilerServices.Unsafe 84
TYPE: System.Globalization.EraInfo 85
TYPE: Mono.RuntimeEventHandle 87
TYPE: System.Activator 89
TYPE: System.Collections.Generic.NullableComparer`1 92
TYPE: System.IO.FileSystem 94
TYPE: Mono.RuntimeGPtrArrayHandle 95
TYPE: System.TypeInitializationException 96
TYPE: System.ValueTuple 97
TYPE: System.Globalization.TimeSpanParse 104
TYPE: System.Reflection.MethodInfo 105
TYPE: Mono.Globalization.Unicode.ContractionComparer 107
TYPE: Mono.RuntimeClassHandle 109
TYPE: Mono.SafeStringMarshal 110
TYPE: System.Reflection.SignatureArrayType 112
TYPE: System.Gen2GcCallback 114
TYPE: Interop/ErrorInfo 115
TYPE: System.Security.SecurityException 116
TYPE: System.Reflection.SignatureHasElementType 123
TYPE: System.Runtime.ExceptionServices.ExceptionDispatchInfo 128
TYPE: System.UInt16 130
TYPE: System.MissingFieldException 135
TYPE: System.StringComparer 140
TYPE: System.Byte 145
TYPE: Mono.RuntimeGenericParamInfoHandle 146
TYPE: System.Reflection.ParameterInfo 148
TYPE: System.Boolean 149
TYPE: System.Array/InternalEnumerator`1 152
TYPE: System.ObjectDisposedException 154
TYPE: System.UInt32 155
TYPE: System.MissingMethodException 156
TYPE: System.Reflection.Assembly 157
TYPE: Interop/Sys 158
TYPE: System.Reflection.ExceptionHandlingClause 160
TYPE: System.UIntPtr 162
TYPE: System.Text.ValueUtf8Converter 167
TYPE: System.Runtime.Serialization.SerializationInfoEnumerator 169
TYPE: System.Int32 170
TYPE: System.Runtime.InteropServices.GCHandle 174
TYPE: System.Threading.ThreadPoolWorkQueueThreadLocals 176
TYPE: System.RuntimeFieldHandle 178
TYPE: System.Text.EncoderExceptionFallbackBuffer 180
TYPE: System.Text.EncoderFallbackException 182
TYPE: System.Threading.WaitHandle 183
TYPE: System.Collections.Generic.List`1/Enumerator 185
TYPE: System.ValueType 186
TYPE: System.ArgumentOutOfRangeException 187
TYPE: System.CharEnumerator 190
TYPE: System.Int64 193
TYPE: System.Globalization.TextInfoToUpperData 194
TYPE: System.OrdinalComparer 195
TYPE: System.Collections.Generic.GenericEqualityComparer`1 199
TYPE: System.Collections.Generic.Dictionary`2/Enumerator 200
TYPE: System.Threading.EventWaitHandle 201
TYPE: System.MissingMemberException 207
TYPE: System.Reflection.ReflectionTypeLoadException 208
TYPE: System.Collections.Generic.ObjectEqualityComparer`1 209
TYPE: System.Text.DecoderReplacementFallback 210
TYPE: System.Collections.Generic.NullableEqualityComparer`1 211
TYPE: System.Collections.HashHelpers 212
TYPE: System.Runtime.Remoting.Messaging.MonoMethodMessage 213
TYPE: System.Collections.Comparer 215
TYPE: System.Globalization.TextInfoToLowerData 217
TYPE: System.Threading.Thread 219
TYPE: System.Collections.Generic.ValueListBuilder`1 220
TYPE: System.Collections.Concurrent.ConcurrentDictionary`2/<GetEnumerator>d__35 233
TYPE: System.Text.EncoderReplacementFallback 236
TYPE: System.Globalization.CultureNotFoundException 237
TYPE: System.Nullable`1 240
TYPE: System.Guid/GuidResult 246
TYPE: System.ParamsArray 251
TYPE: System.Collections.Generic.Comparer`1 254
TYPE: System.SByte 255
TYPE: System.Int16 257
TYPE: System.Runtime.CompilerServices.ConditionalWeakTable`2/Enumerator 263
TYPE: System.Reflection.RuntimeConstructorInfo 266
TYPE: System.RuntimeMethodHandle 267
TYPE: System.Convert 271
TYPE: System.IntPtr 274
TYPE: System.Buffers.Text.FormattingHelpers 279
TYPE: Mono.Globalization.Unicode.CodePointIndexer 283
TYPE: System.Reflection.MemberInfo 285
TYPE: System.Threading.ThreadPoolWorkQueue/SparseArray`1 295
TYPE: System.RuntimeType/ListBuilder`1 302
TYPE: System.Threading.ThreadPoolWorkQueue/QueueSegment 304
TYPE: System.Single 306
TYPE: System.Environment 309
TYPE: System.Reflection.FieldInfo 314
TYPE: System.Runtime.Serialization.SerializationEvents 322
TYPE: System.Reflection.Module 325
TYPE: System.Reflection.SignatureConstructedGenericType 329
TYPE: System.CultureAwareComparer 343
TYPE: System.Runtime.InteropServices.DllImportAttribute 347
TYPE: System.TypeLoadException 349
TYPE: System.Globalization.CharUnicodeInfo 352
TYPE: Mono.Globalization.Unicode.MSCompatUnicodeTableUtil 376
TYPE: System.BadImageFormatException 379
TYPE: System.Threading.ExecutionContext 380
TYPE: System.Globalization.HebrewNumber 381
TYPE: System.Reflection.RuntimeEventInfo 382
TYPE: System.Double 390
TYPE: System.TimeZoneInfo/AdjustmentRule 393
TYPE: System.Text.EncoderFallbackBuffer 394
TYPE: System.TimeZoneInfo/TransitionTime 401
TYPE: System.Diagnostics.StackFrame 422
TYPE: System.Reflection.SignatureType 436
TYPE: System.Collections.Generic.EqualityComparer`1 444
TYPE: System.Runtime.InteropServices.SafeHandle 447
TYPE: System.Globalization.SortKey 450
TYPE: System.Reflection.MethodBase 454
TYPE: System.Runtime.InteropServices.Marshal 458
TYPE: System.Runtime.Serialization.SerializationInfo 465
TYPE: System.ValueTuple`2 466
TYPE: System.Globalization.DateTimeFormatInfoScanner 474
TYPE: System.Reflection.RuntimeParameterInfo 478
TYPE: System.Reflection.RuntimeAssembly 480
TYPE: System.Marvin 488
TYPE: System.AppDomain 494
TYPE: System.Text.EncoderReplacementFallbackBuffer 501
TYPE: System.Char 504
TYPE: System.MulticastDelegate 508
TYPE: System.Globalization.JapaneseCalendar 533
TYPE: System.IO.UnmanagedMemoryStream 535
TYPE: System.Reflection.RuntimeMethodInfo 553
TYPE: System.RuntimeTypeHandle 556
TYPE: System.Text.InternalEncoderBestFitFallbackBuffer 578
TYPE: System.Buffers.ArrayPoolEventSource 585
TYPE: System.TimeSpan 596
TYPE: System.ReadOnlySpan`1 599
TYPE: System.IO.__Error 636
TYPE: System.Globalization.GregorianCalendar 637
TYPE: System.Text.Encoding 641
TYPE: System.Threading.ThreadPoolWorkQueue 645
TYPE: System.Random 666
TYPE: System.Reflection.RuntimePropertyInfo 690
TYPE: System.Attribute 700
TYPE: System.Text.ValueStringBuilder 702
TYPE: System.Span`1 703
TYPE: System.Reflection.SignatureTypeExtensions 711
TYPE: System.MemoryExtensions 829
TYPE: System.Threading.ThreadPoolWorkQueue/WorkStealingQueue 863
TYPE: System.Buffer 871
TYPE: System.Collections.Generic.ArraySortHelper`1 878
TYPE: System.IO.MonoIO 879
TYPE: System.Reflection.AssemblyName 899
TYPE: System.Globalization.TimeSpanFormat/FormatLiterals 902
TYPE: System.Collections.Generic.ArraySortHelper`2 931
TYPE: System.Exception 938
TYPE: System.Buffers.TlsOverPerCoreLockedStacksArrayPool`1 975
TYPE: System.Globalization.NumberFormatInfo 1007
TYPE: System.Runtime.CompilerServices.ConditionalWeakTable`2 1028
TYPE: System.Globalization.GregorianCalendarHelper 1032
TYPE: System.Version 1044
TYPE: System.Threading.SpinLock 1072
TYPE: System.Collections.Generic.List`1 1087
TYPE: System.Enum 1340
TYPE: System.ThrowHelper 1348
TYPE: System.ParseNumbers 1407
TYPE: System.Delegate 1440
TYPE: System.Diagnostics.StackTrace 1464
TYPE: System.Globalization.TimeSpanFormat 1565
TYPE: System.IO.FileStream 1722
TYPE: Mono.Globalization.Unicode.SortKeyBuffer 1841
TYPE: System.DateTime 1963
TYPE: System.MonoCustomAttrs 1984
TYPE: System.Text.UTF8Encoding 2037
TYPE: System.Globalization.TextInfo 2125
TYPE: System.Array 2149
TYPE: System.IO.Path 2261
TYPE: System.Collections.Generic.Dictionary`2 2298
TYPE: System.Globalization.CalendarData 2324
TYPE: System.DefaultBinder 2404
TYPE: System.Globalization.CompareInfo 2439
TYPE: System.Type 2667
TYPE: Mono.Globalization.Unicode.MSCompatUnicodeTable 2689
TYPE: System.Globalization.DateTimeFormatInfo 2894
TYPE: System.Collections.Concurrent.ConcurrentDictionary`2 2908
TYPE: System.TimeZoneInfo 3041
TYPE: System.SpanHelpers 3045
TYPE: System.Text.StringBuilder 4553
TYPE: System.DateTimeFormat 5036
TYPE: System.Guid 5057
TYPE: System.RuntimeType 5164
TYPE: Mono.Globalization.Unicode.SimpleCollator 7326
TYPE: System.String 7531
TYPE: System.Globalization.CultureInfo 9321
TYPE: System.Number 14115
TYPE: System.Numerics.Vector`1 15127

```

## Web Assembly - Summary

Before:

```
-rw-r--r--  1 mabaul  wheel   924160 Mar 11 17:47 mscorlib.dll
```

Reflection Emit Gate:

```
-rw-r--r--  1 mabaul  wheel   907776 Mar 11 17:58 mscorlib.dll
```

Remoting Gate:

```
-rw-r--r--  1 mabaul  wheel   898560 Mar 11 18:07 mscorlib.dll
```


## Simple Collator (experimental)

Web Assembly (linked):

```
-rw-r--r--  1 mabaul  wheel   844288 Mar 11 18:27 mscorlib.dll
```

