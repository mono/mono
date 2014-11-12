// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// <OWNER>[....]</OWNER>
// 
// Interface Map.  This struct returns the mapping of an interface into the actual methods on a class
//    that implement that interface.
//
// <EMAIL>Author: darylo</EMAIL>
// Date: [....] 2000
//
namespace System.Reflection {
    using System;

[System.Runtime.InteropServices.ComVisible(true)]
    public struct InterfaceMapping {
[System.Runtime.InteropServices.ComVisible(true)]
        public Type                TargetType;            // The type implementing the interface
[System.Runtime.InteropServices.ComVisible(true)]
        public Type                InterfaceType;        // The type representing the interface
[System.Runtime.InteropServices.ComVisible(true)]
        public MethodInfo[]        TargetMethods;        // The methods implementing the interface
[System.Runtime.InteropServices.ComVisible(true)]
        public MethodInfo[]        InterfaceMethods;    // The methods defined on the interface
    }
}
