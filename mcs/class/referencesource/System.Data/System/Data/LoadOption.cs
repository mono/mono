//------------------------------------------------------------------------------
// <copyright file="LoadOption.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">amirhmy</owner>
// <owner current="true" primary="false">markash</owner>
// <owner current="false" primary="false">jasonzhu</owner>
//------------------------------------------------------------------------------

namespace System.Data {

    public enum LoadOption {
        OverwriteChanges      = 1,
        PreserveChanges       = 2,
        Upsert                = 3,
    }
}

