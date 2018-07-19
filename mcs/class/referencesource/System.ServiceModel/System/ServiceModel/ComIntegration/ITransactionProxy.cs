//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Transactions;

    internal enum DtcIsolationLevel
    {
        ISOLATIONLEVEL_UNSPECIFIED = -1,
        ISOLATIONLEVEL_CHAOS = 0x10,
        ISOLATIONLEVEL_READUNCOMMITTED = 0x100,
        ISOLATIONLEVEL_BROWSE = 0x100,
        ISOLATIONLEVEL_CURSORSTABILITY = 0x1000,
        ISOLATIONLEVEL_READCOMMITTED = 0x1000,
        ISOLATIONLEVEL_REPEATABLEREAD = 0x10000,
        ISOLATIONLEVEL_SERIALIZABLE = 0x100000,
        ISOLATIONLEVEL_ISOLATED = 0x100000
    }

    [SuppressUnmanagedCodeSecurity]
    [ComImport]
    [Guid("02558374-DF2E-4dae-BD6B-1D5C994F9BDC")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    interface ITransactionProxy
    {
        void Commit(Guid guid);
        void Abort();

        [return: MarshalAs(UnmanagedType.Interface)]
        IDtcTransaction Promote();

        void CreateVoter(
            [MarshalAs(UnmanagedType.Interface)] ITransactionVoterNotifyAsync2 voterNotification,
            IntPtr voterBallot);
        DtcIsolationLevel GetIsolationLevel();
        Guid GetIdentifier();
        bool IsReusable();
    }


    [SuppressUnmanagedCodeSecurity]
    [ComImport]
    [Guid("5433376C-414D-11d3-B206-00C04FC2F3EF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITransactionVoterBallotAsync2
    {
        void VoteRequestDone(
            int hr,
            int reason
            );
    }


    [SuppressUnmanagedCodeSecurity]
    [ComImport]
    [Guid("3A6AD9E2-23B9-11cf-AD60-00AA00A74CCD")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITransactionOutcomeEvents
    {
        void Committed([MarshalAs(UnmanagedType.Bool)] bool retaining,
           int newUow,
           int hr);

        void Aborted(int reason,
            [MarshalAs(UnmanagedType.Bool)] bool retaining,
            int newUow,
            int hr);

        void HeuristicDecision(
            int decision,
            int reason,
            int hr);

        void InDoubt();
    }


    [SuppressUnmanagedCodeSecurity]
    [ComImport]
    [Guid("5433376B-414D-11d3-B206-00C04FC2F3EF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITransactionVoterNotifyAsync2
    {
        void Committed(
            [MarshalAs(UnmanagedType.Bool)] bool retaining,
            int newUow,
            int hr);

        void Aborted(
            int reason,
            [MarshalAs(UnmanagedType.Bool)] bool retaining,
            int newUow,
            int hr);

        void HeuristicDecision(
            int decision,
            int reason,
            int hr);

        void InDoubt();


        void VoteRequest();
    }
}
