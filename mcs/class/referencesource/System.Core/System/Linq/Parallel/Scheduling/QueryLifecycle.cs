// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// QueryLifecycle.cs
//
// <OWNER>Microsoft</OWNER>
//
// A convenient place to put things associated with entire queries and their lifecycle events.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace System.Linq.Parallel
{
    internal static class QueryLifecycle
    {
        // This method is called once per execution of a logical query.
        // (It is not called multiple time if repartitionings occur)
        internal static void LogicalQueryExecutionBegin(int queryID)
        {
            //We call NOCTD to inform the debugger that multiple threads will most likely be required to 
            //execute this query.  We do not attempt to run the query even if we think we could, for simplicity and consistency.
#if !PFX_LEGACY_3_5 && !SILVERLIGHT
            Debugger.NotifyOfCrossThreadDependency();
#endif

#if !FEATURE_PAL && !SILVERLIGHT    // PAL doesn't support  eventing
            PlinqEtwProvider.Log.ParallelQueryBegin(queryID);
#endif
        }


        // This method is called once per execution of a logical query.
        // (It is not called multiple time if repartitionings occur)
        internal static void LogicalQueryExecutionEnd(int queryID)
        {
#if !FEATURE_PAL && !SILVERLIGHT    // PAL doesn't support  eventing
            PlinqEtwProvider.Log.ParallelQueryEnd(queryID);
#endif
        }
    }
}
