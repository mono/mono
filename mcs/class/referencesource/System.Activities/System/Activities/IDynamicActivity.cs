//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Collections.ObjectModel;

    interface IDynamicActivity
    {
        string Name { get; set; }
        KeyedCollection<string, DynamicActivityProperty> Properties { get; }
    }
}
