// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Threading.Tasks
{
    //
    // Dummy implementation of AsyncCausality events
    //
    internal static partial class DebuggerSupport
    {
        public static bool LoggingOn
        {
            get
            {
                return false;
            }
        }

        public static void TraceOperationCreation(CausalityTraceLevel traceLevel, Task task, String operationName, ulong relatedContext)
        {
        }

        public static void TraceOperationCompletion(CausalityTraceLevel traceLevel, Task task, AsyncCausalityStatus status)
        {
        }

        public static void TraceOperationRelation(CausalityTraceLevel traceLevel, Task task, CausalityRelation relation)
        {
        }

        public static void TraceSynchronousWorkStart(CausalityTraceLevel traceLevel, Task task, CausalitySynchronousWork work)
        {
        }

        public static void TraceSynchronousWorkCompletion(CausalityTraceLevel traceLevel, CausalitySynchronousWork work)
        {
        }
    }
}
