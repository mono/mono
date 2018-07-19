//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
     using System;
     using System.Runtime.InteropServices;
     using System.Collections.Generic;
     using System.ServiceModel;
     using System.Runtime.Remoting.Proxies;
     internal interface IProxyCreator : IDisposable
     {
          
          ComProxy CreateProxy (IntPtr outer, ref Guid riid);
          bool SupportsErrorInfo (ref Guid riid);
          bool SupportsDispatch ();
          bool SupportsIntrinsics ();
    
     }
}
     
