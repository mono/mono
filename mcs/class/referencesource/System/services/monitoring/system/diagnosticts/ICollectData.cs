//------------------------------------------------------------------------------
// <copyright file="ICollectData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

//---------------------------------------------------------------------------
// ICollectData.cs
//---------------------------------------------------------------------------
// WARNING: this file autogenerated
//---------------------------------------------------------------------------
// Copyright (c) 1999, Microsoft Corporation   All Rights Reserved
// Information Contained Herein Is Proprietary and Confidential.
//---------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.Runtime.InteropServices;

    using System.Diagnostics;
    using System;
    

    /// <internalonly/>
    [ComImport, Guid("73386977-D6FD-11D2-BED5-00C04F79E3AE"), System.Runtime.InteropServices.InterfaceTypeAttribute(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICollectData {

    	 /// <devdoc>
    	 ///    <para>[To be supplied.]</para>
    	 /// </devdoc>
    	[return: MarshalAs(UnmanagedType.I4 )]
    	 void CollectData(
    		[In, MarshalAs(UnmanagedType.I4 )] 
    		 int id,
    		[In, MarshalAs(UnmanagedType.SysInt )] 
    		 IntPtr valueName,
    		[In, MarshalAs(UnmanagedType.SysInt )] 
    		 IntPtr data,
    		[In, MarshalAs(UnmanagedType.I4 )] 
    		 int totalBytes,
            [Out, MarshalAs(UnmanagedType.SysInt)]
    		 out IntPtr res);

    	 /// <devdoc>
    	 ///    <para>[To be supplied.]</para>
    	 /// </devdoc>
    	 void CloseData();


    }
}
