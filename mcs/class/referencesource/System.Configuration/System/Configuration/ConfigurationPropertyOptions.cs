//------------------------------------------------------------------------------
// <copyright file="ConfigurationPropertyOptions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace System.Configuration {
    [Flags]
    public enum ConfigurationPropertyOptions {
        None                                     = 0,
        IsDefaultCollection                      = 0x00000001,
        IsRequired                               = 0x00000002,
        IsKey                                    = 0x00000004,
        IsTypeStringTransformationRequired       = 0x00000008,
        IsAssemblyStringTransformationRequired   = 0x00000010,
        IsVersionCheckRequired                   = 0x00000020,
    }
}
