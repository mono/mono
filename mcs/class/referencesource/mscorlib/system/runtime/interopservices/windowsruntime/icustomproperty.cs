// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// <OWNER>[....]</OWNER>

using System;
using System.Diagnostics.Contracts;

namespace System.Runtime.InteropServices.WindowsRuntime
{
    [ComImport]
    [Guid("30DA92C0-23E8-42A0-AE7C-734A0E5D2782")]
    [WindowsRuntimeImport]
    internal interface ICustomProperty
    {
        Type Type
        {
            [Pure]
            get;
        }
        
        string Name 
        { 
            [Pure]
            get; 
        }

        [Pure]
        object GetValue(object target);

        void SetValue(object target, object value);
        
        [Pure]
        object GetValue(object target, object indexValue);
        
        void SetValue(object target, object value, object indexValue);
        
        bool CanWrite 
        { 
            [Pure]
            get; 
        }

        bool CanRead  
        { 
            [Pure]
            get; 
        }               
    }
}
 
