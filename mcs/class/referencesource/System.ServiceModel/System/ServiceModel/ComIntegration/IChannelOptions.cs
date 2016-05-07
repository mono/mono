//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Channels;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.ServiceModel;

    [ComImport,
     Guid("d1bc6624-f145-4904-ac39-1ee483c8ca9c"),
     InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIDispatch)]
    interface IChannelOptions
    {
    }

}
