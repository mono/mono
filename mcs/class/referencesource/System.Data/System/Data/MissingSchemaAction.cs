//------------------------------------------------------------------------------
// <copyright file="MissingSchemaAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    public enum MissingSchemaAction {
        Add        = 1,
        Ignore     = 2,
        Error      = 3,
        AddWithKey = 4
    }
}
