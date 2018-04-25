// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

// These have never existed in System.Core in any desktop CLR release.  However, in Silverlight 4 they
// were defined in System.Core (see file://../../core.small/system/Lazy.cs).  To preserve binary compatibility,
// we need these forwarders.
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Lazy<>))]
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Threading.LazyThreadSafetyMode))]
