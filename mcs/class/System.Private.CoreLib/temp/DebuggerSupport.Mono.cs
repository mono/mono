// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    //
    // This class encapsulates the infrastructure to emit AsyncCausality events and Task-ID tracking for the use of the debugger.
    //
    internal static partial class DebuggerSupport
    {
        //==============================================================================================================
        // This section of the class tracks adds the ability to retrieve an active Task from its ID. The debugger
        // is only component that needs this tracking so we only enable it when the debugger specifically requests it
        // by setting Task.s_asyncDebuggingEnabled.
        //==============================================================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddToActiveTasks(Task task)
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void AddToActiveTasksNonInlined(Task task)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveFromActiveTasks(Task task)
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RemoveFromActiveTasksNonInlined(Task task)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task GetActiveTaskFromId(int taskId)
        {
            Task task = null;
            return task;
        }

        //==============================================================================================================
        // This section of the class wraps calls to get the lazy-created Task object for the purpose of reporting
        // async causality events to the debugger.
        //==============================================================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task GetTaskIfDebuggingEnabled(this AsyncVoidMethodBuilder builder)
        {
                return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task GetTaskIfDebuggingEnabled(this AsyncTaskMethodBuilder builder)
        {
                return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task GetTaskIfDebuggingEnabled<TResult>(this AsyncTaskMethodBuilder<TResult> builder)
        {
                return null;
        }
    }
}
