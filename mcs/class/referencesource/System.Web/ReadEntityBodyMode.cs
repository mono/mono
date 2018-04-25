//------------------------------------------------------------------------------
// <copyright file="ReadEntityBodyMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * ReadEntityBodyMode indicates how the entity is being read.
 *
 * Copyright (c) 2010 Microsoft Corporation
 */

namespace System.Web {
    using System;
    
    public enum ReadEntityBodyMode {
        None,
        Classic, // BinaryRead, Form, Files, InputStream
        Bufferless, // GetBufferlessInputStream
        Buffered // GetBufferedInputStream
    }
}
