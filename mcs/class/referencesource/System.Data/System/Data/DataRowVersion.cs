//------------------------------------------------------------------------------
// <copyright file="DataRowVersion.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">amirhmy</owner>
// <owner current="true" primary="false">markash</owner>
// <owner current="false" primary="false">jasonzhu</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System.Configuration.Assemblies;
    using System.Diagnostics;

    public enum DataRowVersion {
        Original =  0x0100,
        Current  =  0x0200,
        Proposed =  0x0400,
        Default  = Proposed | Current,
    }
}
