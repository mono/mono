//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
     using System;
     using System.Runtime.InteropServices;
     using System.ServiceModel;
     using System.Runtime.Remoting.Proxies;
     using System.Runtime.Remoting;
     
     interface ICreateServiceChannel
     {
          RealProxy CreateChannel ();     
     }
     
}
