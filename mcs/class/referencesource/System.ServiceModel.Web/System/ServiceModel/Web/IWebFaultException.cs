//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Web
{
    using System;
    using System.Net;

    interface IWebFaultException
    {
        HttpStatusCode StatusCode { get; }
        Type DetailType { get; }
        object DetailObject { get; }
        Type[] KnownTypes { get; }
    }
}
