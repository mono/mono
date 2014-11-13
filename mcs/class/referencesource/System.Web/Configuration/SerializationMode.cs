//------------------------------------------------------------------------------
// <copyright file="SerializationMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration
{
    public enum SerializationMode
    {
        String = 0,
        Xml = 1,
        Binary = 2,
        ProviderSpecific = 3
    }
}
