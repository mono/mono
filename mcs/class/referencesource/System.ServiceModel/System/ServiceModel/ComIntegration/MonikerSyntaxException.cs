//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
     using System;
     using System.Runtime.InteropServices;
     using System.Runtime.InteropServices.ComTypes;
     using Microsoft.Win32;
     using System.Reflection;
     using System.Collections.Generic;
     using System.Threading;
     using System.Text;
     
     internal class MonikerSyntaxException : COMException 
     {
          internal MonikerSyntaxException (string message) : base (message, HR.MK_E_SYNTAX)
          {
               
          }
     }
}
     
     
     
