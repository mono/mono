//---------------------------------------------------------------------------
// <copyright file="PerfService.cs" company="Microsoft">
//   Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// Description: Implements the Service class for perf diagnostics
//---------------------------------------------------------------------------

using System;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using SRCS = System.Runtime.CompilerServices;

using Microsoft.Win32;
using MS.Internal.PresentationCore;
using MS.Internal;
using MS.Utility;
using MS.Win32.PresentationCore;
using System.Reflection;

namespace MS.Utility
{
    [FriendAccessAllowed]
    static internal class PerfService
    {
        // Map of elements to IDs.  A couple of notes:
        // 1) Use a ConditionalWeakTable because it holds weak references to
        //     the keys and is self-cleaning.
        // 2) Use object, instead of long, for the values because
        //     ConditionalWeakTable requires the TValue parameter to be a
        //     reference type.
        private static SRCS.ConditionalWeakTable<object, object> perfElementIds = new SRCS.ConditionalWeakTable<object, object>();

        ///<summary>
        ///     Every element is uniquely identfied with an ID, and this ID
        ///     gets traced with ETW traces and can be mapped back to an
        ///     element in a tool.
        ///</summary>
        internal static long GetPerfElementID2(object element, string extraData)
        {
            return (long) perfElementIds.GetValue(
                element,
                delegate(object key)
                {
                    long eltId = SafeNativeMethods.GetNextPerfElementId();

                    // If this is the first time we see this object emit some useful info about it.
                    if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose))
                    {
                        Type type = key.GetType();
                        Assembly asm = type.Assembly;

                        // Eventtracing below does a recursive call to GetPerfElementID2.
                        // Break the recustion by not tracing if key matches with asm.
                        if (!Object.ReferenceEquals(key, asm))
                        {
                            EventTrace.EventProvider.TraceEvent(
                                EventTrace.Event.PerfElementIDAssignment,
                                EventTrace.Keyword.KeywordGeneral,
                                EventTrace.Level.Verbose,
                                eltId,
                                type.FullName,
                                extraData,
                                GetPerfElementID2(asm, asm.FullName));
                        }
                    }

                    return eltId;
                });
        }

        internal static long GetPerfElementID(object element)
        {
            return GetPerfElementID2(element, string.Empty);
        }
    }
}
