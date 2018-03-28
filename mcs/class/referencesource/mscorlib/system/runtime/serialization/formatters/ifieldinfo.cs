// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
 /*============================================================
 **
 ** Class: IFieldInfo
 **
 **
 ** Purpose: Interface For Returning FieldNames and FieldTypes
 **
 **
 ===========================================================*/

namespace System.Runtime.Serialization.Formatters {

    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System;

[System.Runtime.InteropServices.ComVisible(true)]
    public interface IFieldInfo
    {
        // Name of parameters, if null the default param names will be used
        String[] FieldNames 
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
            [System.Security.SecurityCritical]  // auto-generated_required
            set;
        }
        Type[] FieldTypes 
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
            [System.Security.SecurityCritical]  // auto-generated_required
            set;
        }        
    }
}
