//------------------------------------------------------------------------------
// <copyright file="SerializationHints.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;

namespace System.Xml.Xsl.Qil {
    internal enum SerializationHints {
        None        = 0,
        CData,
        DisableOutputEscaping,
    }
}
