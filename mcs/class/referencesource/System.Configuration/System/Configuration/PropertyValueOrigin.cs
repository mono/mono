//------------------------------------------------------------------------------
// <copyright file="PropertyValueOrigin.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Configuration {

    public enum PropertyValueOrigin {
        Default = 0,   // Default is retrieved
        Inherited = 1, // It is inherited
        SetHere = 2    // It was set here
    }
}
