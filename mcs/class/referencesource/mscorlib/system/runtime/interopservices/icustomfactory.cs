// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.InteropServices {

    using System;

[System.Runtime.InteropServices.ComVisible(true)]
    public interface ICustomFactory
    {
        MarshalByRefObject CreateInstance(Type serverType);
    }

}
