// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

// moved to mscorlib.dll
[assembly:System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Action))]
[assembly:System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Action<,>))]
[assembly:System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Action<,,>))]
[assembly:System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Action<,,,>))]
[assembly:System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Func<>))]
[assembly:System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Func<,>))]
[assembly:System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Func<,,>))]
[assembly:System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Func<,,,>))]
[assembly:System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Func<,,,,>))]

// Action and Func types exist in mscorlib (up to 8 generic argument paremeters)
// and in System.Core.dll (See Microsoft\Scripting\Utils\[Action.cs | Function.cs])
// for 9-16 generic argument parameter versions.
