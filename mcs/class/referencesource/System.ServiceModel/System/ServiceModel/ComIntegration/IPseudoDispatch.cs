//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    [Guid("16BFA998-CA5B-4f29-B64F-123293EB159D")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    interface IPseudoDispatch
    {
        void GetIDsOfNames(UInt32 cNames, // size_is param for rgszNames
                    [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 0)] string[] rgszNames,
                    IntPtr pDispID);

        [PreserveSig]
        int Invoke(
                    UInt32 dispIdMember,
                    UInt32 cArgs,
                    UInt32 cNamedArgs,
                    IntPtr rgvarg,
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] UInt32[] rgdispidNamedArgs,
                    IntPtr pVarResult,
                    IntPtr pExcepInfo,
                    out UInt32 pArgErr
                );
    }
}
