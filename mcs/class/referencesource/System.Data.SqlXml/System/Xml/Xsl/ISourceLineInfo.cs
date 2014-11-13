//------------------------------------------------------------------------------
// <copyright file="ISourceLineInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl {
    internal interface ISourceLineInfo {
        string Uri      { get; }
        bool IsNoSource { get; }
        Location Start     { get; }
        Location End       { get; }
    }
}
