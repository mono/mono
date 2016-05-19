//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    public interface IExtensibleDataObject
    {
        ExtensionDataObject ExtensionData { get; set; }
    }
}
