//------------------------------------------------------------------------------
// <copyright file="EventSchemaTraceListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace System.Diagnostics {

public enum TraceLogRetentionOption {

    // One file with no maxFileSize
    SingleFileUnboundedSize = 2,
    
    // One file with a maxFileSize 
    SingleFileBoundedSize = 4,
    
    // Infinite number of sequential files, each with maxFileSize 
    // When MaxFileSize is reached, writing starts in a new file with an incremented integer suffix.  
    UnlimitedSequentialFiles = 0,
    
    // Finite number of sequential files, each with maxFileSize 
    LimitedSequentialFiles = 3,
    
    // Finite number of circular sequential files, each with maxFileSize.  
    // When MaxFileSize is reached, writing starts in a new file with an incremented integer suffix.  
    // When MaxNumberOfFiles is reached first file is overwritten.  Files are then incrementally overwritten in a circular manner.
    LimitedCircularFiles = 1
}

}
