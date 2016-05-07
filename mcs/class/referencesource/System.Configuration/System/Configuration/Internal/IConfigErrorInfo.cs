//------------------------------------------------------------------------------
// <copyright file="IConfigErrorInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration.Internal {

    public interface IConfigErrorInfo {
        string Filename   { get; }
        int    LineNumber { get; }
    }
}
