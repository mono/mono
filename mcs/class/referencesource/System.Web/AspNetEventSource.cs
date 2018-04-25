//------------------------------------------------------------------------------
// <copyright file="AspNetEventSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Web.Hosting;
    using System.Web.Util;

    // Name and Guid are part of the public contract (for identification by ETW listeners) so cannot
    // be changed. We're statically specifying a GUID using the same logic as EventSource.GetGuid,
    // as otherwise EventSource invokes crypto to generate the GUID and this results in an
    // unacceptable performance degradation (DevDiv #652801).
    [EventSource(Name = "Microsoft-Windows-ASPNET", Guid = "ee799f41-cfa5-550b-bf2c-344747c1c668")]
    internal sealed class AspNetEventSource : EventSource {

        // singleton
        public static readonly AspNetEventSource Instance = new AspNetEventSource();

        private unsafe delegate void WriteEventWithRelatedActivityIdCoreDelegate(int eventId, Guid* childActivityID, int eventDataCount, EventData* data);
        private readonly WriteEventWithRelatedActivityIdCoreDelegate _writeEventWithRelatedActivityIdCoreDel;

        private AspNetEventSource() {
            // We need to light up when running on .NET 4.5.1 since we can't compile directly
            // against the protected methods we might need to consume. Only ever try creating
            // this delegate if we're in full trust, otherwise exceptions could happen at
            // inopportune times (such as during invocation).

            if (AppDomain.CurrentDomain.IsHomogenous && AppDomain.CurrentDomain.IsFullyTrusted) {
                MethodInfo writeEventWithRelatedActivityIdCoreMethod = typeof(EventSource).GetMethod(
                    "WriteEventWithRelatedActivityIdCore", BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new Type[] { typeof(int), typeof(Guid*), typeof(int), typeof(EventData*) }, null);

                if (writeEventWithRelatedActivityIdCoreMethod != null) {
                    _writeEventWithRelatedActivityIdCoreDel = (WriteEventWithRelatedActivityIdCoreDelegate)Delegate.CreateDelegate(
                        typeof(WriteEventWithRelatedActivityIdCoreDelegate), this, writeEventWithRelatedActivityIdCoreMethod, throwOnBindFailure: false);
                }
            }
        }

        [NonEvent] // use the private member signature for deducing ETW parameters
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RequestEnteredAspNetPipeline(IIS7WorkerRequest wr, Guid childActivityId) {
            if (!IsEnabled()) {
                return;
            }

            Guid parentActivityId = wr.RequestTraceIdentifier;
            RequestEnteredAspNetPipelineImpl(parentActivityId, childActivityId);
        }

        [NonEvent] // use the private member signature for deducing ETW parameters
        private unsafe void RequestEnteredAspNetPipelineImpl(Guid iisActivityId, Guid aspNetActivityId) {
            if (ActivityIdHelper.Instance == null || _writeEventWithRelatedActivityIdCoreDel == null || iisActivityId == Guid.Empty) {
                return;
            }

            // IIS doesn't always set the current thread's activity ID before invoking user code. Instead,
            // its tracing APIs (IHttpTraceContext::RaiseTraceEvent) set the ID, write to ETW, then reset
            // the ID. If we want to write a transfer event but the current thread's activity ID is
            // incorrect, then we need to mimic this behavior. We don't use a try / finally since
            // exceptions here are fatal to the process.

            Guid originalThreadActivityId = ActivityIdHelper.Instance.CurrentThreadActivityId;
            bool needToSetThreadActivityId = (originalThreadActivityId != iisActivityId);

            // Step 1: Set the ID (if necessary)
            if (needToSetThreadActivityId) {
                ActivityIdHelper.Instance.SetCurrentThreadActivityId(iisActivityId, out originalThreadActivityId);
            }

            // Step 2: Write to ETW, providing the recipient activity ID.
            _writeEventWithRelatedActivityIdCoreDel((int)Events.RequestEnteredAspNetPipeline, &aspNetActivityId, 0, null);

            // Step 3: Reset the ID (if necessary)
            if (needToSetThreadActivityId) {
                Guid unused;
                ActivityIdHelper.Instance.SetCurrentThreadActivityId(originalThreadActivityId, out unused);
            }
        }

        // Transfer event signals that control has transitioned from IIS -> ASP.NET.
        // Overload used only for deducing ETW parameters; use the public entry point instead.
        //
        // !! WARNING !!
        // The logic in RequestEnteredAspNetPipelineImpl must be kept in sync with these parameters, otherwise
        // type safety violations could occur.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "ETW looks at this method using reflection.")]
        [Event((int)Events.RequestEnteredAspNetPipeline, Level = EventLevel.Informational, Task = (EventTask)Tasks.Request, Opcode = EventOpcode.Send, Version = 1)]
        private void RequestEnteredAspNetPipeline() {
            throw new NotImplementedException();
        }

        [NonEvent] // use the private member signature for deducing ETW parameters
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void RequestStarted(IIS7WorkerRequest wr) {
            if (!IsEnabled()) {
                return;
            }

            RequestStartedImpl(wr);
        }

        [NonEvent] // use the private member signature for deducing ETW parameters
        private unsafe void RequestStartedImpl(IIS7WorkerRequest wr) {
            string httpVerb = wr.GetHttpVerbName();
            HTTP_COOKED_URL* pCookedUrl = wr.GetCookedUrl();
            Guid iisEtwActivityId = wr.RequestTraceIdentifier;
            Guid requestCorrelationId = wr.GetRequestCorrelationId();

            fixed (char* pHttpVerb = httpVerb) {
                // !! WARNING !!
                // This logic must be kept in sync with the ETW-deduced parameters in RequestStarted,
                // otherwise type safety violations could occur.
                const int EVENTDATA_COUNT = 3;
                EventData* pEventData = stackalloc EventData[EVENTDATA_COUNT];

                FillInEventData(&pEventData[0], httpVerb, pHttpVerb);

                // We have knowledge that pFullUrl is null-terminated so we can optimize away
                // the copy we'd otherwise have to perform. Still need to adjust the length
                // to account for the null terminator, though.
                Debug.Assert(pCookedUrl->pFullUrl != null);
                pEventData[1].DataPointer = (IntPtr)pCookedUrl->pFullUrl;
                pEventData[1].Size = checked(pCookedUrl->FullUrlLength + sizeof(char));

                FillInEventData(&pEventData[2], &requestCorrelationId);
                WriteEventCore((int)Events.RequestStarted, EVENTDATA_COUNT, pEventData);
            }
        }

        // Event signals that ASP.NET has started processing a request.
        // Overload used only for deducing ETW parameters; use the public entry point instead.
        //
        // Visual Studio Online #222067 - This event is hardcoded to opt-out of EventSource activityID tracking. 
        // This would normally be done by setting ActivityOptions = EventActivityOptions.Disable in the 
        // Event attribute, but this causes a dependency between System.Web and mscorlib that breaks servicing. 
        // 
        // !! WARNING !!
        // The logic in RequestStartedImpl must be kept in sync with these parameters, otherwise
        // type safety violations could occur.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "ETW looks at this method using reflection.")]
        [Event((int)Events.RequestStarted, Level = EventLevel.Informational, Task = (EventTask)Tasks.Request, Opcode = EventOpcode.Start, Version = 1)]
        private unsafe void RequestStarted(string HttpVerb, string FullUrl, Guid RequestCorrelationId) {
            throw new NotImplementedException();
        }

        // Event signals that ASP.NET has completed processing a request.
        //
        // Visual Studio Online #222067 - This event is hardcoded to opt-out of EventSource activityID tracking. 
        // This would normally be done by setting ActivityOptions = EventActivityOptions.Disable in the 
        // Event attribute, but this causes a dependency between System.Web and mscorlib that breaks servicing. 
        [Event((int)Events.RequestCompleted, Level = EventLevel.Informational, Task = (EventTask)Tasks.Request, Opcode = EventOpcode.Stop, Version = 1)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RequestCompleted() {
            if (!IsEnabled()) {
                return;
            }

            WriteEvent((int)Events.RequestCompleted);
        }

        /*
         * Helpers to populate the EventData structure
         */

        // prerequisite: str must be pinned and provided as pStr; may be null.
        // we'll convert null strings to empty strings if necessary.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static void FillInEventData(EventData* pEventData, string str, char* pStr) {
#if DBG
            fixed (char* pStr2 = str) { Debug.Assert(pStr == pStr2); }
#endif

            if (pStr != null) {
                pEventData->DataPointer = (IntPtr)pStr;
                pEventData->Size = checked((str.Length + 1) * sizeof(char)); // size is specified in bytes, including null wide char
            }
            else {
                pEventData->DataPointer = NullHelper.Instance.PtrToNullChar; // empty string
                pEventData->Size = sizeof(char);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static void FillInEventData(EventData* pEventData, Guid* pGuid) {
            Debug.Assert(pGuid != null);
            pEventData->DataPointer = (IntPtr)pGuid;
            pEventData->Size = sizeof(Guid);
        }

        // Each ETW event should have its own entry here.
        private enum Events {
            RequestEnteredAspNetPipeline = 1,
            RequestStarted,
            RequestCompleted
        }

        // Tasks are used for correlating events; we're free to define our own.
        // For example, Tasks.Request with Opcode = Start matches Tasks.Request with Opcode = Stop,
        // and Tasks.Application with Opcode = Start matches Tasks.Application with Opcode = Stop.
        //
        // EventSource requires that this be a public static class with public const fields,
        // otherwise manifest generation could fail at runtime.
        public static class Tasks {
            public const EventTask Request = (EventTask)1;
        }

        private sealed class NullHelper : CriticalFinalizerObject {
            public static readonly NullHelper Instance = new NullHelper();

            [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = @"Containing type is a CriticalFinalizerObject.")]
            public readonly IntPtr PtrToNullChar;

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            private unsafe NullHelper() {
                // allocate a single null character
                PtrToNullChar = Marshal.AllocHGlobal(sizeof(char));
                *((char*)PtrToNullChar) = '\0';
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            ~NullHelper() {
                if (PtrToNullChar != IntPtr.Zero) {
                    Marshal.FreeHGlobal(PtrToNullChar);
                }
            }
        }
    }
}
