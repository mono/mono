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

