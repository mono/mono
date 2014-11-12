#if !FEATURE_PAL && !FEATURE_CORECLR
// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// <OWNER>AlfreMen</OWNER>
//

using System;
using System.Security;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

using WFD = Windows.Foundation.Diagnostics;

namespace System.Threading.Tasks
{

    [FriendAccessAllowed]
    internal enum CausalityTraceLevel
    {
        Required = WFD.CausalityTraceLevel.Required,
        Important = WFD.CausalityTraceLevel.Important,
        Verbose = WFD.CausalityTraceLevel.Verbose
    }

    [FriendAccessAllowed]
    internal enum AsyncCausalityStatus
    {
        Canceled = WFD.AsyncCausalityStatus.Canceled,
        Completed = WFD.AsyncCausalityStatus.Completed,
        Error = WFD.AsyncCausalityStatus.Error,
        Started = WFD.AsyncCausalityStatus.Started
    }

    internal enum CausalityRelation
    {
        AssignDelegate = WFD.CausalityRelation.AssignDelegate,
        Join = WFD.CausalityRelation.Join,
        Choice = WFD.CausalityRelation.Choice,
        Cancel = WFD.CausalityRelation.Cancel,
        Error = WFD.CausalityRelation.Error
    }

    internal enum CausalitySynchronousWork
    {
        CompletionNotification = WFD.CausalitySynchronousWork.CompletionNotification,
        ProgressNotification = WFD.CausalitySynchronousWork.ProgressNotification,
        Execution = WFD.CausalitySynchronousWork.Execution
    }

    [FriendAccessAllowed]
    internal static class AsyncCausalityTracer
    {
        //s_PlatformId = {4B0171A6-F3D0-41A0-9B33-02550652B995}
        private static readonly Guid s_PlatformId = new Guid(0x4B0171A6, 0xF3D0, 0x41A0, 0x9B, 0x33, 0x02, 0x55, 0x06, 0x52, 0xB9, 0x95);

        //Indicates this information comes from the BCL Library
        private const WFD.CausalitySource s_CausalitySource = WFD.CausalitySource.Library;

        //Lazy initialize the actual factory
        private static WFD.IAsyncCausalityTracerStatics s_TracerFactory;

        //We receive the actual value for these as a callback
        private static bool f_LoggingOn; //assumes false by default

        [FriendAccessAllowed]
        internal static bool LoggingOn
        {
            [FriendAccessAllowed]
            get
            {
                if (!f_FactoryInitialized)
                    FactoryInitialized();

                return f_LoggingOn;
            }
        }

        private static bool f_FactoryInitialized; //assumes false by default
        private static object _InitializationLock = new object();

        //explicit cache
        private static readonly Func<WFD.IAsyncCausalityTracerStatics> s_loadFactoryDelegate = LoadFactory;

        [SecuritySafeCritical]
        private static WFD.IAsyncCausalityTracerStatics LoadFactory()
        {
            if (!Environment.IsWinRTSupported) return null;
            
            //COM Class Id
            string ClassId = "Windows.Foundation.Diagnostics.AsyncCausalityTracer";

            //COM Interface GUID  {50850B26-267E-451B-A890-AB6A370245EE}
            Guid guid = new Guid(0x50850B26, 0x267E, 0x451B, 0xA8, 0x90, 0XAB, 0x6A, 0x37, 0x02, 0x45, 0xEE);

            Object factory = null;
            
            WFD.IAsyncCausalityTracerStatics validFactory = null;

            try
            {
                int hresult = Microsoft.Win32.UnsafeNativeMethods.RoGetActivationFactory(ClassId, ref guid, out factory);

                if (hresult < 0 || factory == null) return null; //This prevents having an exception thrown in case IAsyncCausalityTracerStatics isn't registered.
                
                validFactory = (WFD.IAsyncCausalityTracerStatics)factory;

                EventRegistrationToken token = validFactory.add_TracingStatusChanged(new EventHandler<WFD.TracingStatusChangedEventArgs>(TracingStatusChangedHandler));
                Contract.Assert(token != null, "EventRegistrationToken is null");
            }
            catch (Exception)
            {
                // Although catching generic Exception is not recommended, this file is one exception
                // since we don't want to propagate any kind of exception to the user since all we are
                // doing here depends on internal state.
                return null;
            }

            return validFactory;
        }

        private static bool FactoryInitialized()
        {
            return (LazyInitializer.EnsureInitialized(ref s_TracerFactory, ref f_FactoryInitialized, ref _InitializationLock, s_loadFactoryDelegate) != null);
        }

        [SecuritySafeCritical]
        private static void TracingStatusChangedHandler(Object sender, WFD.TracingStatusChangedEventArgs args)
        {
            f_LoggingOn = args.Enabled;
        }

        [FriendAccessAllowed]
        internal static void TraceOperationCreation(CausalityTraceLevel traceLevel, int taskId, string operationName, ulong relatedContext)
        {
            if (LoggingOn)
            {
                s_TracerFactory.TraceOperationCreation((WFD.CausalityTraceLevel)traceLevel, s_CausalitySource, s_PlatformId, GetOperationId((uint)taskId), operationName, relatedContext);
            }
        }

        [FriendAccessAllowed]
        internal static void TraceOperationCompletion(CausalityTraceLevel traceLevel, int taskId, AsyncCausalityStatus status)
        {
            if (LoggingOn)
            {
                s_TracerFactory.TraceOperationCompletion((WFD.CausalityTraceLevel)traceLevel, s_CausalitySource, s_PlatformId, GetOperationId((uint)taskId), (WFD.AsyncCausalityStatus)status);
            }
        }

        internal static void TraceOperationRelation(CausalityTraceLevel traceLevel, int taskId, CausalityRelation relation)
        {
            if (LoggingOn)
            {
                s_TracerFactory.TraceOperationRelation((WFD.CausalityTraceLevel)traceLevel, s_CausalitySource, s_PlatformId, GetOperationId((uint)taskId), (WFD.CausalityRelation)relation);
            }
        }

        internal static void TraceSynchronousWorkStart(CausalityTraceLevel traceLevel, int taskId, CausalitySynchronousWork work)
        {
            if (LoggingOn)
            {
                s_TracerFactory.TraceSynchronousWorkStart((WFD.CausalityTraceLevel)traceLevel, s_CausalitySource, s_PlatformId, GetOperationId((uint)taskId), (WFD.CausalitySynchronousWork)work);
            }
        }

        internal static void TraceSynchronousWorkCompletion(CausalityTraceLevel traceLevel, CausalitySynchronousWork work)
        {
            if (LoggingOn)
            {
                s_TracerFactory.TraceSynchronousWorkCompletion((WFD.CausalityTraceLevel)traceLevel, s_CausalitySource, (WFD.CausalitySynchronousWork)work);
            }
        }

        private static ulong GetOperationId(uint taskId)
        {
            return (((ulong)AppDomain.CurrentDomain.Id) << 32) + taskId;
        }

    }
}

#endif
