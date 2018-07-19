// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    internal interface IMergeEnabledMessageProperty
    {
        bool TryMergeWithProperty(object propertyToMerge);
    }
}
