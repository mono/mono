//------------------------------------------------------------------------------
// <copyright file="MissingMappingAction.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">markash</owner>
// <owner current="true" primary="false">laled</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    public enum MissingMappingAction {
        Passthrough = 1,
        Ignore      = 2,
        Error       = 3,
    }
}
