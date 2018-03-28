// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace System {
    
    using System;
    using System.Runtime.InteropServices;


    public interface IServiceProvider
    {
        // Interface does not need to be marked with the serializable attribute
        Object GetService(Type serviceType);
    }
}
