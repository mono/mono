/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/
using System; using Microsoft;


#if !SILVERLIGHT // ComObject

using System.Runtime.InteropServices;
using ComTypes = System.Runtime.InteropServices.ComTypes;

#if CODEPLEX_40
namespace System.Dynamic {
#else
namespace Microsoft.Scripting {
#endif

    [
    ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIDispatch),
    Guid("00020400-0000-0000-C000-000000000046")
    ]
    internal interface IDispatchForReflection {
    }

    [
    ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("00020400-0000-0000-C000-000000000046"),
    ]
    internal interface IDispatch {

        [PreserveSig]
        int TryGetTypeInfoCount(out uint pctinfo);

        [PreserveSig]
        int TryGetTypeInfo(uint iTInfo, int lcid, out IntPtr info);

        [PreserveSig]
        int TryGetIDsOfNames(
            ref Guid iid,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)]
            string[] names,
            uint cNames,
            int lcid,
            [Out]
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeParamIndex = 2)]
            int[] rgDispId);

        [PreserveSig]
        int TryInvoke(
            int dispIdMember,
            ref Guid riid,
            int lcid,
            ComTypes.INVOKEKIND wFlags,
            ref ComTypes.DISPPARAMS pDispParams,
            out object VarResult,
            out ComTypes.EXCEPINFO pExcepInfo,
            out uint puArgErr);
    }

    /// <summary>
    /// Layout of the IDispatch vtable
    /// </summary>
    internal enum IDispatchMethodIndices {
        IUnknown_QueryInterface,
        IUnknown_AddRef,
        IUnknown_Release,

        IDispatch_GetTypeInfoCount ,
        IDispatch_GetTypeInfo,
        IDispatch_GetIDsOfNames,
        IDispatch_Invoke
    }

    [
    ComImport,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("B196B283-BAB4-101A-B69C-00AA00341D07")
    ]
    internal interface IProvideClassInfo {
        void GetClassInfo(out IntPtr info);
    }

}

#endif
