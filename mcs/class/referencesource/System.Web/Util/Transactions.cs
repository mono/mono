//------------------------------------------------------------------------------
// <copyright file="Transactions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Transactions support for ASP.NET pages
 * 
 * Copyright (c) 2000, Microsoft Corporation
 */
namespace System.Web.Util {

using System.Collections;
using System.EnterpriseServices;
using System.Security.Permissions;

//
//  Delegate to the transacted managed code
//


/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public delegate void TransactedCallback();

//
//  Delegate for the internal transacted execution
//

internal enum TransactedExecState {
    CommitPending = 0,
    AbortPending = 1,
    Error = 2
}

internal delegate int TransactedExecCallback();  // return value 'int' for interop

//
//  Utility class with to be called to do transactions
//


/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public class Transactions {

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public static void InvokeTransacted(TransactedCallback callback, TransactionOption mode) {
        bool aborted = false;
        InvokeTransacted(callback, mode, ref aborted);
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public static void InvokeTransacted(TransactedCallback callback, TransactionOption mode, ref bool transactionAborted) {
        // check for hosting permission even if no user code on the stack
        HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Medium, SR.Transaction_not_supported_in_low_trust);

        bool executeWithoutTransaction = false;

#if !FEATURE_PAL // FEATURE_PAL does not enable Transactions
        if (Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version.Major <= 4)
            throw new PlatformNotSupportedException(SR.GetString(SR.RequiresNT));
#else // !FEATURE_PAL
        throw new NotImplementedException("ROTORTODO");
#endif // !FEATURE_PAL


        if (mode == TransactionOption.Disabled)
            executeWithoutTransaction = true;

        if (executeWithoutTransaction) {
            // bypass the transaction logic
            callback();
            transactionAborted = false;
            return;
        }

        TransactedInvocation call = new TransactedInvocation(callback);
        TransactedExecCallback execCallback = new TransactedExecCallback(call.ExecuteTransactedCode);

        PerfCounters.IncrementCounter(AppPerfCounter.TRANSACTIONS_PENDING);

        int rc;
        try {
            rc = UnsafeNativeMethods.TransactManagedCallback(execCallback, (int)mode);
        }
        finally {
            PerfCounters.DecrementCounter(AppPerfCounter.TRANSACTIONS_PENDING);
        }

        // rethrow the expection originally caught in managed code
        if (call.Error != null)
            throw new HttpException(null, call.Error); 

        PerfCounters.IncrementCounter(AppPerfCounter.TRANSACTIONS_TOTAL);

        if (rc == 1) {
            PerfCounters.IncrementCounter(AppPerfCounter.TRANSACTIONS_COMMITTED);
            transactionAborted = false;
        }
        else if (rc == 0) {
            PerfCounters.IncrementCounter(AppPerfCounter.TRANSACTIONS_ABORTED);
            transactionAborted = true;
        }
        else {
            throw new HttpException(SR.GetString(SR.Cannot_execute_transacted_code));
        }
    }

    // Class with wrappers to ContextUtil that don't throw

    internal class Utils {
        private Utils() {
        }

        /*
        internal static String TransactionId {
            get {
                String id = null;

                try {
                    id = ContextUtil.TransactionId.ToString();
                }
                catch {
                }

                return id;
            }
        }
        */

        internal static bool IsInTransaction {
            get {
                bool inTransaction = false;

                try {
                    inTransaction = ContextUtil.IsInTransaction;
                }
                catch {
                }

                return inTransaction;
            }
        }

        internal static bool AbortPending {
            get {
                bool aborted = false;

                try {
                    if (ContextUtil.MyTransactionVote == TransactionVote.Abort)
                        aborted = true;
                }
                catch {
                }

                return aborted;
            }
        }
    }

    // Managed class encapsulating the transacted call

    internal class TransactedInvocation {
        private TransactedCallback _callback;
        private Exception _error;

        internal TransactedInvocation(TransactedCallback callback) {
            _callback = callback;
        }

        internal int ExecuteTransactedCode() {
            TransactedExecState state = TransactedExecState.CommitPending;

            try {
                _callback();

                if (Transactions.Utils.AbortPending)
                    state = TransactedExecState.AbortPending;
            }
            catch (Exception e) {
                _error = e;  // remember exception to be rethrown back in managed code
                state = TransactedExecState.Error;
            }

            return (int)state;
        }

        internal Exception Error {
            get { return _error; }
        }
    }
}
}
