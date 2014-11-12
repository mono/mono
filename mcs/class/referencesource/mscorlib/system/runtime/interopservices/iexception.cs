// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Interface: _Exception
**
**
** Purpose: COM backwards compatibility with v1 Exception
**        object layout.
**
**
=============================================================================*/

namespace System.Runtime.InteropServices {
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    
    [GuidAttribute("b36b5c63-42ef-38bc-a07e-0b34c98f164a")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsDual)]
    [CLSCompliant(false)]
[System.Runtime.InteropServices.ComVisible(true)]
    public interface _Exception
    {
        // This contains all of our V1 Exception class's members.

        // From Object
        String ToString();
        bool Equals (Object obj);
        int GetHashCode ();
        Type GetType ();

        // From V1's Exception class
        String Message {
            get;
        }

        Exception GetBaseException();

        String StackTrace {
            get;
        }

        String HelpLink {
            get;
            set;
        }

        String Source {
            #if FEATURE_CORECLR
            [System.Security.SecurityCritical] // auto-generated
            #endif
            get;
            #if FEATURE_CORECLR
            [System.Security.SecurityCritical] // auto-generated
            #endif
            set;
        }
        [System.Security.SecurityCritical]  // auto-generated_required
        void GetObjectData(SerializationInfo info, StreamingContext context);

        Exception InnerException {
            get;
        }
        
        MethodBase TargetSite {
            get;
        }
   }

}
