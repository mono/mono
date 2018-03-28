//------------------------------------------------------------------------------
// <copyright file="EtwTrace.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * EtwTrace class
 */
namespace System.Web {

    using System.Web.Hosting;
    using System.Web.Util;

    internal enum EtwTraceConfigType {
        DOWNLEVEL = 0,
        IIS7_ISAPI = 1,
        IIS7_INTEGRATED = 2            
    }

    internal enum EtwTraceType {
        ETW_TYPE_START                                    = 1,
        ETW_TYPE_END                                      = 2,
        ETW_TYPE_REQ_QUEUED                               = 3,
        ETW_TYPE_REQ_DEQUEUED                             = 4,
        ETW_TYPE_GETAPPDOMAIN_ENTER                       = 5,
        ETW_TYPE_GETAPPDOMAIN_LEAVE                       = 6, 
        ETW_TYPE_APPDOMAIN_ENTER                          = 7,
        ETW_TYPE_START_HANDLER                            = 8,
        ETW_TYPE_END_HANDLER                              = 9,
        ETW_TYPE_PIPELINE_ENTER                           = 10,
        ETW_TYPE_PIPELINE_LEAVE                           = 11,
        ETW_TYPE_MAPHANDLER_ENTER                         = 12,
        ETW_TYPE_MAPHANDLER_LEAVE                         = 13,
        ETW_TYPE_PARSE_ENTER                              = 14,
        ETW_TYPE_PARSE_LEAVE                              = 15,
        ETW_TYPE_COMPILE_ENTER                            = 16,
        ETW_TYPE_COMPILE_LEAVE                            = 17,
        ETW_TYPE_HTTPHANDLER_ENTER                        = 18,
        ETW_TYPE_HTTPHANDLER_LEAVE                        = 19,
        ETW_TYPE_SESSIONSTATE_PARTITION_START             = 20,
        ETW_TYPE_SESSIONSTATE_PARTITION_END               = 21,
        ETW_TYPE_PAGE_PRE_INIT_ENTER                      = 22,
        ETW_TYPE_PAGE_PRE_INIT_LEAVE                      = 23,
        ETW_TYPE_PAGE_INIT_ENTER                          = 24,
        ETW_TYPE_PAGE_INIT_LEAVE                          = 25,
        ETW_TYPE_PAGE_LOAD_VIEWSTATE_ENTER                = 26,
        ETW_TYPE_PAGE_LOAD_VIEWSTATE_LEAVE                = 27,
        ETW_TYPE_PAGE_LOAD_POSTDATA_ENTER                 = 28,
        ETW_TYPE_PAGE_LOAD_POSTDATA_LEAVE                 = 29,
        ETW_TYPE_PAGE_LOAD_ENTER                          = 30,
        ETW_TYPE_PAGE_LOAD_LEAVE                          = 31,
        ETW_TYPE_PAGE_POST_DATA_CHANGED_ENTER             = 32,
        ETW_TYPE_PAGE_POST_DATA_CHANGED_LEAVE             = 33,
        ETW_TYPE_PAGE_RAISE_POSTBACK_ENTER                = 34,
        ETW_TYPE_PAGE_RAISE_POSTBACK_LEAVE                = 35,
        ETW_TYPE_PAGE_PRE_RENDER_ENTER                    = 36,
        ETW_TYPE_PAGE_PRE_RENDER_LEAVE                    = 37,
        ETW_TYPE_PAGE_SAVE_VIEWSTATE_ENTER                = 38,
        ETW_TYPE_PAGE_SAVE_VIEWSTATE_LEAVE                = 39,
        ETW_TYPE_PAGE_RENDER_ENTER                        = 40,
        ETW_TYPE_PAGE_RENDER_LEAVE                        = 41,
        ETW_TYPE_SESSION_DATA_BEGIN                       = 42,
        ETW_TYPE_SESSION_DATA_END                         = 43,
        ETW_TYPE_PROFILE_BEGIN                            = 44,
        ETW_TYPE_PROFILE_END                              = 45,
        ETW_TYPE_ROLE_IS_USER_IN_ROLE                     = 46,
        ETW_TYPE_ROLE_GET_USER_ROLES                      = 47,
        ETW_TYPE_ROLE_BEGIN                               = 48,
        ETW_TYPE_ROLE_END                                 = 49,
        ETW_TYPE_WEB_EVENT_RAISE_START                    = 50,
        ETW_TYPE_WEB_EVENT_RAISE_END                      = 51,
        ETW_TYPE_WEB_EVENT_DELIVER_START                  = 52,
        ETW_TYPE_WEB_EVENT_DELIVER_END                    = 53
    }

    struct EtwTraceLevel {
        internal const int None               = 0;
        internal const int Fatal              = 1;
        internal const int Error              = 2;
        internal const int Warning            = 3;
        internal const int Information        = 4;
        internal const int Verbose            = 5;
    }
    
    struct EtwTraceFlags {
        internal const int None              = 0;
        internal const int Infrastructure    = 1;
        internal const int Module            = 2;
        internal const int Page              = 4;
        internal const int AppSvc            = 8;
    }

    // these need to match the definitions in 
    // mgdeng:mgdhandler.hxx 
    // the internal WorkerRequest trace helpers
    // takes this enum as the type
    internal enum IntegratedTraceType {
        TraceWrite    = 0,
        TraceWarn     = 1,
        DiagCritical  = 2,
        DiagError     = 3,
        DiagWarning   = 4,
        DiagInfo      = 5,
        DiagVerbose   = 6,
        DiagStart     = 7,
        DiagStop      = 8,
        DiagSuspend   = 9,
        DiagResume    = 10,
        DiagTransfer  = 11,
    }

    internal enum EtwWorkerRequestType {
        Undefined = -1,
        InProc = 0,
        OutOfProc = 1,
        IIS7Integrated = 3,
        Unknown = 999
    }

    internal static class EtwTrace {
        private static int _traceLevel = 0;
        private static int _traceFlags = 0;
        private static EtwWorkerRequestType s_WrType = EtwWorkerRequestType.Undefined;

        internal static int InferVerbosity(IntegratedTraceType traceType) {

            int verbosity = EtwTraceLevel.Verbose;

            switch (traceType) {
                case IntegratedTraceType.TraceWrite:
                    verbosity = EtwTraceLevel.Verbose;
                    break;

                case IntegratedTraceType.TraceWarn:
                    verbosity = EtwTraceLevel.Warning;
                    break;

                case IntegratedTraceType.DiagCritical:
                    verbosity = EtwTraceLevel.Fatal;
                    break;

                case IntegratedTraceType.DiagWarning:
                    verbosity = EtwTraceLevel.Warning;
                    break;

                case IntegratedTraceType.DiagError:
                    verbosity = EtwTraceLevel.Error;
                    break;

                case IntegratedTraceType.DiagInfo:
                    verbosity = EtwTraceLevel.Information;
                    break;

                case IntegratedTraceType.DiagVerbose:
                    verbosity = EtwTraceLevel.Verbose;
                    break;

                case IntegratedTraceType.DiagStart:
                    verbosity = EtwTraceLevel.None;
                    break;

                case IntegratedTraceType.DiagStop:
                    verbosity = EtwTraceLevel.None;
                    break;

                case IntegratedTraceType.DiagResume:
                    verbosity = EtwTraceLevel.None;
                    break;

                case IntegratedTraceType.DiagSuspend:
                    verbosity = EtwTraceLevel.None;
                    break;

                case IntegratedTraceType.DiagTransfer:
                    verbosity = EtwTraceLevel.None;
                    break;

                default:
                    verbosity = EtwTraceLevel.Verbose;
                    break;

            }

            return verbosity;
        }

        internal static bool IsTraceEnabled(int level, int flag) {
            if (level < _traceLevel && ((flag & _traceFlags) != EtwTraceFlags.None))
                return true;
            return false;
        }

        private static void ResolveWorkerRequestType(HttpWorkerRequest workerRequest)
        {
            if (workerRequest is IIS7WorkerRequest) {
                s_WrType = EtwWorkerRequestType.IIS7Integrated;
            }
            else if (workerRequest is ISAPIWorkerRequestInProc) {
                    s_WrType = EtwWorkerRequestType.InProc;
            }
            else if (workerRequest is ISAPIWorkerRequestOutOfProc){
                s_WrType = EtwWorkerRequestType.OutOfProc;
            }
            else {
                s_WrType = EtwWorkerRequestType.Unknown;
            }
        }

        internal static void TraceEnableCheck(EtwTraceConfigType configType, IntPtr p)
        {
            // Don't activate if webengine.dll isn't loaded
            if (!HttpRuntime.IsEngineLoaded)
                return;
            
            switch (configType) {
                case EtwTraceConfigType.IIS7_INTEGRATED:
                    bool f;
                    UnsafeIISMethods.MgdEtwGetTraceConfig(p /*pRequestContext*/, out f, out _traceFlags, out _traceLevel);
                    break;
                case EtwTraceConfigType.IIS7_ISAPI:
                    int[] contentInfo = new int[3];
                    UnsafeNativeMethods.EcbGetTraceFlags(p /*pECB*/, contentInfo);
                    _traceFlags = contentInfo[0];
                    _traceLevel = contentInfo[1];
                    break;
                case EtwTraceConfigType.DOWNLEVEL:
                    UnsafeNativeMethods.GetEtwValues(out _traceLevel, out _traceFlags);
                    break;
                default:
                    break;
            }
        }

        internal static void Trace(EtwTraceType traceType, HttpWorkerRequest workerRequest)
        {
            Trace(traceType, workerRequest, null, null);
        }

        internal static void Trace(EtwTraceType traceType, HttpWorkerRequest workerRequest, string data1)
        {
            Trace(traceType, workerRequest, data1, null, null, null);
        }

        internal static void Trace(EtwTraceType traceType, HttpWorkerRequest workerRequest, string data1, string data2)
        {
            Trace(traceType, workerRequest, data1, data2, null, null);
        }

        internal static void Trace(EtwTraceType traceType, HttpWorkerRequest workerRequest, string data1, string data2, string data3, string data4)
        {
            if (s_WrType == EtwWorkerRequestType.Undefined) {
                ResolveWorkerRequestType(workerRequest);
            }

            if (s_WrType == EtwWorkerRequestType.Unknown) 
                return;

            if (workerRequest == null)
                return;
            
            if (s_WrType == EtwWorkerRequestType.IIS7Integrated) {
                UnsafeNativeMethods.TraceRaiseEventMgdHandler((int) traceType, ((IIS7WorkerRequest)workerRequest).RequestContext, data1, data2, data3, data4);
            }
            else if (s_WrType == EtwWorkerRequestType.InProc) {
                UnsafeNativeMethods.TraceRaiseEventWithEcb((int) traceType, ((ISAPIWorkerRequest)workerRequest).Ecb, data1, data2, data3, data4);
            }
            else if (s_WrType == EtwWorkerRequestType.OutOfProc) {
                UnsafeNativeMethods.PMTraceRaiseEvent((int) traceType, ((ISAPIWorkerRequest)workerRequest).Ecb, data1, data2, data3, data4);
            }
        }

        internal static void Trace(EtwTraceType traceType, IntPtr ecb, string data1, string data2, bool inProc)
        {
            if (inProc)
                UnsafeNativeMethods.TraceRaiseEventWithEcb((int) traceType, ecb, data1, data2, null, null);
            else 
                UnsafeNativeMethods.PMTraceRaiseEvent((int) traceType, ecb, data1, data2, null, null);
        }

    };
}

