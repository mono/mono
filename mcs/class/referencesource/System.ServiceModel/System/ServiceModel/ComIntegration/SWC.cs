//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Channels;
    using System.Runtime.InteropServices;

    [Serializable]
    [ComVisible(false)]
    enum ThreadPoolOption
    {
        None = 0,
        Inherit = 1,
        STA = 2,
        MTA = 3
    }

    [Serializable]
    [ComVisible(false)]
    enum BindingOption
    {
        NoBinding = 0,
        BindingToPoolThread = 1
    }

    [Serializable]
    [ComVisible(false)]
    enum SxsOption
    {
        Ignore = 0,
        Inherit = 1,
        New = 2
    }

    [Serializable]
    [ComVisible(false)]
    enum PartitionOption
    {
        Ignore = 0,
        Inherit = 1,
        New = 2
    }

    [Serializable]
    [ComVisible(false)]
    enum TransactionConfig
    {
        NoTransaction = 0,
        IfContainerIsTransactional = 1,
        CreateTransactionIfNecessary = 2,
        NewTransaction = 3
    }

    [Serializable]
    [ComVisible(false)]
    enum CSC_SxsConfig
    {
        CSC_NoSxs = 0,
        CSC_InheritSxs = 1,
        CSC_NewSxs = 2
    }

    [ComImport]
    [Guid("186d89bc-f277-4bcc-80d5-4df7b836ef4a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IServiceThreadPoolConfig
    {
        void SelectThreadPool(ThreadPoolOption threadPool);
        void SetBindingInfo(BindingOption binding);
    }


    [ComImport]
    [Guid("80182d03-5ea4-4831-ae97-55beffc2e590")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IServicePartitionConfig
    {
        void PartitionConfig(PartitionOption partitionConfig);
        void PartitionID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPartitionID);
    }

    [ComImport]
    [Guid("33CAF1A1-FCB8-472b-B45E-967448DED6D8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IServiceSysTxnConfig
    {
        // NOTE: This is actually IServiceSysTxnConfigInternal.
        void ConfigureTransaction(TransactionConfig transactionConfig);
        void IsolationLevel(int option);
        void TransactionTimeout(uint ulTimeoutSec);
        void BringYourOwnTransaction([MarshalAs(UnmanagedType.LPWStr)] string szTipURL);
        void NewTransactionDescription([MarshalAs(UnmanagedType.LPWStr)] string szTxDesc);
        void ConfigureBYOT(IntPtr pITxByot);
        void ConfigureBYOTSysTxn(IntPtr pITxByot);
    }

    [ComImport]
    [Guid("C7CD7379-F3F2-4634-811B-703281D73E08")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IServiceSxsConfig
    {
        void SxsConfig(CSC_SxsConfig sxsConfig);
        void SxsName([MarshalAs(UnmanagedType.LPWStr)] string szSxsName);
        void SxsDirectory([MarshalAs(UnmanagedType.LPWStr)] string szSxsDirectory);
    }

    [ComImport]
    [Guid("59f4c2a3-d3d7-4a31-b6e4-6ab3177c50b9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IServiceTransactionConfig
    {
        // NOTE: This is actually IServiceSysTxnConfigInternal.
        void ConfigureTransaction(TransactionConfig transactionConfig);
        void IsolationLevel(int option);
        void TransactionTimeout(uint ulTimeoutSec);
        void BringYourOwnTransaction([MarshalAs(UnmanagedType.LPWStr)] string szTipURL);
        void NewTransactionDescription([MarshalAs(UnmanagedType.LPWStr)] string szTxDesc);
        void ConfigureBYOT(IntPtr pITxByot);
    }


    [ComImport]
    [Guid("ecabb0c8-7f19-11d2-978e-0000f8757e2a")]
    class CServiceConfig { }

    [ComImport]
    [Guid("BD3E2E12-42DD-40f4-A09A-95A50C58304B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IServiceCall
    {
        void OnCall();
    }


    [ComImport]
    [Guid("67532E0C-9E2F-4450-A354-035633944E17")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IServiceActivity
    {
        void SynchronousCall(IServiceCall pIServiceCall);
        void AsynchronousCall(IServiceCall pIServiceCall);
        void BindToCurrentThread();
        void UnbindFromThread();
    }

    [ComImport]
    [Guid("000001ce-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IComThreadingInfo
    {
        void GetCurrentApartmentType(out uint aptType);
        void GetCurrentThreadType(out uint threadType);
        void GetCurrentLogicalThreadId(out Guid guidLogicalThreadID);
        void SetCurrentLogicalThreadId([In, MarshalAs(UnmanagedType.LPStruct)] Guid guidLogicalThreadID);
    };

    [ComImport,
     Guid("75B52DDB-E8ED-11D1-93AD-00AA00BA3258"),
     InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IObjectContextInfo
    {
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsInTransaction();
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Interface)]
        Object GetTransaction();
        void GetTransactionId(out Guid guid);
        void GetActivityId(out Guid guid);
        void GetContextId(out Guid guid);
    }

}
