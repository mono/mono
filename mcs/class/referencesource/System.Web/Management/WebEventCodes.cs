//------------------------------------------------------------------------------
// <copyright file="WebEventCodes.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Management {

    using System.Globalization;
    using System.Collections;
    using Debug=System.Web.Util.Debug;
    using System.Security.Permissions;

    // this class is a container for pre-defined event codes
    // all APIs will take integers so application defined
    // codes or new codes added through servicing are supported
    public sealed class WebEventCodes {

        private WebEventCodes() {
        }
        
        static WebEventCodes()
        {
            InitEventArrayDimensions();
        }
        
        // (not bit flags)
        // we're not using an enum for extensibility reasons
        public const int InvalidEventCode = -1;
        public const int UndefinedEventCode = 0;
        public const int UndefinedEventDetailCode = 0;

        // ----------------------------------
        // Application Codes
        // ----------------------------------
        public const int ApplicationCodeBase = 1000;
        public const int ApplicationStart = ApplicationCodeBase + 1;
        public const int ApplicationShutdown = ApplicationCodeBase + 2;
        public const int ApplicationCompilationStart = ApplicationCodeBase + 3;
        public const int ApplicationCompilationEnd = ApplicationCodeBase + 4;
        public const int ApplicationHeartbeat = ApplicationCodeBase + 5;

        internal const int ApplicationCodeBaseLast = ApplicationCodeBase + 5;

    
        // ----------------------------------
        // Request Codes
        // ----------------------------------
        public const int RequestCodeBase = 2000;
        public const int RequestTransactionComplete = RequestCodeBase+1;
        public const int RequestTransactionAbort = RequestCodeBase+2;

        internal const int RequestCodeBaseLast = RequestCodeBase+2;


        // ----------------------------------
        // Error Codes
        // ----------------------------------
        public const int ErrorCodeBase = 3000;
    
        // Errors during request processing related to client input
        // or behavior
        public const int RuntimeErrorRequestAbort = ErrorCodeBase + 1;
        public const int RuntimeErrorViewStateFailure = ErrorCodeBase + 2;
        public const int RuntimeErrorValidationFailure = ErrorCodeBase + 3;
        public const int RuntimeErrorPostTooLarge = ErrorCodeBase + 4;
        public const int RuntimeErrorUnhandledException = ErrorCodeBase + 5;

        // Errors related to configuration or invalid code
        public const int WebErrorParserError = ErrorCodeBase + 6;
        public const int WebErrorCompilationError = ErrorCodeBase + 7;
        public const int WebErrorConfigurationError = ErrorCodeBase + 8;
        public const int WebErrorOtherError = ErrorCodeBase + 9;
        public const int WebErrorPropertyDeserializationError = ErrorCodeBase + 10;
        public const int WebErrorObjectStateFormatterDeserializationError = ErrorCodeBase + 11;

        public const int RuntimeErrorWebResourceFailure = ErrorCodeBase + 12;

        internal const int ErrorCodeBaseLast = ErrorCodeBase + 12;
        
    
        // ----------------------------------
        // Audit codes    
        // ----------------------------------
        public const int AuditCodeBase = 4000;
        
        // success codes
        public const int AuditFormsAuthenticationSuccess = AuditCodeBase + 1;
        public const int AuditMembershipAuthenticationSuccess = AuditCodeBase + 2;
        public const int AuditUrlAuthorizationSuccess = AuditCodeBase + 3;
        public const int AuditFileAuthorizationSuccess = AuditCodeBase + 4;
        
        // failure codes
        public const int AuditFormsAuthenticationFailure = AuditCodeBase +5;
        public const int AuditMembershipAuthenticationFailure = AuditCodeBase + 6;
        public const int AuditUrlAuthorizationFailure = AuditCodeBase + 7;
        public const int AuditFileAuthorizationFailure = AuditCodeBase + 8;
        public const int AuditInvalidViewStateFailure = AuditCodeBase + 9;
        public const int AuditUnhandledSecurityException = AuditCodeBase + 10;
        public const int AuditUnhandledAccessException = AuditCodeBase + 11;
        
        internal const int AuditCodeBaseLast = AuditCodeBase + 11;

        // Misc events
        public const int MiscCodeBase = 6000;
        
        public const int WebEventProviderInformation = MiscCodeBase + 1;

        internal const int MiscCodeBaseLast = MiscCodeBase + 1;

        // Last code base
        internal const int LastCodeBase = 6000;

    
        /////////////////////////////////////////////////////
        // Detail Codes
        /////////////////////////////////////////////////////
        public const int ApplicationDetailCodeBase = 50000;
        public const int ApplicationShutdownUnknown = ApplicationDetailCodeBase + 1;
        public const int ApplicationShutdownHostingEnvironment = ApplicationDetailCodeBase + 2;
        public const int ApplicationShutdownChangeInGlobalAsax = ApplicationDetailCodeBase + 3;
        public const int ApplicationShutdownConfigurationChange = ApplicationDetailCodeBase + 4;
        public const int ApplicationShutdownUnloadAppDomainCalled = ApplicationDetailCodeBase + 5;
        public const int ApplicationShutdownChangeInSecurityPolicyFile = ApplicationDetailCodeBase + 6;
        public const int ApplicationShutdownBinDirChangeOrDirectoryRename = ApplicationDetailCodeBase + 7;
        public const int ApplicationShutdownBrowsersDirChangeOrDirectoryRename = ApplicationDetailCodeBase + 8;
        public const int ApplicationShutdownCodeDirChangeOrDirectoryRename = ApplicationDetailCodeBase + 9;
        public const int ApplicationShutdownResourcesDirChangeOrDirectoryRename = ApplicationDetailCodeBase + 10;
        public const int ApplicationShutdownIdleTimeout = ApplicationDetailCodeBase + 11; 
        public const int ApplicationShutdownPhysicalApplicationPathChanged = ApplicationDetailCodeBase + 12;
        public const int ApplicationShutdownHttpRuntimeClose = ApplicationDetailCodeBase + 13;
        public const int ApplicationShutdownInitializationError = ApplicationDetailCodeBase + 14;
        public const int ApplicationShutdownMaxRecompilationsReached = ApplicationDetailCodeBase + 15;
        public const int StateServerConnectionError = ApplicationDetailCodeBase + 16;
        public const int ApplicationShutdownBuildManagerChange = ApplicationDetailCodeBase + 17;
                
        // Audit detail codes
        public const int AuditDetailCodeBase = 50200;
        public const int InvalidTicketFailure = AuditDetailCodeBase + 1;
        public const int ExpiredTicketFailure = AuditDetailCodeBase + 2;
        public const int InvalidViewStateMac = AuditDetailCodeBase + 3;
        public const int InvalidViewState = AuditDetailCodeBase + 4;

        // Web Event provider detail codes
        public const int WebEventDetailCodeBase = 50300;
        public const int SqlProviderEventsDropped = WebEventDetailCodeBase + 1;

        // Application extensions should start from here
        public const int WebExtendedBase = 100000;

        internal static string MessageFromEventCode(int eventCode, int eventDetailCode) {
            string  msg = null;
            string  detailMsg = null;

            if (eventDetailCode != 0) {
                switch(eventDetailCode) {
                case ApplicationShutdownUnknown:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownUnknown);
                    break;

                case ApplicationShutdownHostingEnvironment:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownHostingEnvironment);
                    break;

                case ApplicationShutdownChangeInGlobalAsax:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownChangeInGlobalAsax);
                    break;

                case ApplicationShutdownConfigurationChange:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownConfigurationChange);
                    break;

                case ApplicationShutdownUnloadAppDomainCalled:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownUnloadAppDomainCalled);
                    break;

                case ApplicationShutdownChangeInSecurityPolicyFile:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownChangeInSecurityPolicyFile);
                    break;

                case ApplicationShutdownBinDirChangeOrDirectoryRename:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownBinDirChangeOrDirectoryRename);
                    break;

                case ApplicationShutdownBrowsersDirChangeOrDirectoryRename:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownBrowsersDirChangeOrDirectoryRename);
                    break;

                case ApplicationShutdownCodeDirChangeOrDirectoryRename:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownCodeDirChangeOrDirectoryRename);
                    break;

                case ApplicationShutdownResourcesDirChangeOrDirectoryRename:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownResourcesDirChangeOrDirectoryRename);
                    break;

                case ApplicationShutdownIdleTimeout:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownIdleTimeout);
                    break;

                case ApplicationShutdownPhysicalApplicationPathChanged:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownPhysicalApplicationPathChanged);
                    break;

                case ApplicationShutdownHttpRuntimeClose:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownHttpRuntimeClose);
                    break;

                case ApplicationShutdownInitializationError:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownInitializationError);
                    break;

                case ApplicationShutdownMaxRecompilationsReached:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownMaxRecompilationsReached);
                    break;

                case ApplicationShutdownBuildManagerChange:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ApplicationShutdownBuildManagerChange);
                    break;

                case StateServerConnectionError:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_StateServerConnectionError);
                    break;

                case InvalidTicketFailure:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_InvalidTicketFailure);
                    break;
                    
                case ExpiredTicketFailure:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_ExpiredTicketFailure);
                    break;
                    
                case InvalidViewStateMac:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_InvalidViewStateMac);
                    break;
                    
                case InvalidViewState:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_InvalidViewState);
                    break;
                    
                case SqlProviderEventsDropped:
                    detailMsg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_detail_SqlProviderEventsDropped);
                    break;
                    
                default:
                    break;
                }
            }
            
            switch(eventCode) {
            case ApplicationStart:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_ApplicationStart);
                break;

            case ApplicationShutdown:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_ApplicationShutdown);
                break;
            
            case ApplicationCompilationStart:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_ApplicationCompilationStart);
                break;
            
            case ApplicationCompilationEnd:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_ApplicationCompilationEnd);
                break;
            
            case ApplicationHeartbeat:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_ApplicationHeartbeat);
                break;
                
            case RequestTransactionComplete:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_RequestTransactionComplete);
                break;
            
            case RequestTransactionAbort:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_RequestTransactionAbort);
                break;
            
            case RuntimeErrorRequestAbort:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_RuntimeErrorRequestAbort);
                break;
            
            case RuntimeErrorViewStateFailure:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_RuntimeErrorViewStateFailure);
                break;
            
            case RuntimeErrorValidationFailure:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_RuntimeErrorValidationFailure);
                break;
            
            case RuntimeErrorPostTooLarge:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_RuntimeErrorPostTooLarge);
                break;
            
            case RuntimeErrorUnhandledException:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_RuntimeErrorUnhandledException);
                break;
            
            case WebErrorParserError:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_WebErrorParserError);
                break;
            
            case WebErrorCompilationError:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_WebErrorCompilationError);
                break;
            
            case WebErrorConfigurationError:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_WebErrorConfigurationError);
                break;
            
            case AuditUnhandledSecurityException:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_AuditUnhandledSecurityException);
                break;

            case AuditInvalidViewStateFailure:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_AuditInvalidViewStateFailure);
                break;

            case AuditFormsAuthenticationSuccess:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_AuditFormsAuthenticationSuccess);
                break;

            case AuditUrlAuthorizationSuccess:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_AuditUrlAuthorizationSuccess);
                break;

            case AuditFileAuthorizationFailure:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_AuditFileAuthorizationFailure);
                break;

            case AuditFormsAuthenticationFailure:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_AuditFormsAuthenticationFailure);
                break;

            case AuditFileAuthorizationSuccess:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_AuditFileAuthorizationSuccess);
                break;

            case AuditMembershipAuthenticationSuccess:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_AuditMembershipAuthenticationSuccess);
                break;

            case AuditMembershipAuthenticationFailure:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_AuditMembershipAuthenticationFailure);
                break;

            case AuditUrlAuthorizationFailure:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_AuditUrlAuthorizationFailure);
                break;

            case AuditUnhandledAccessException:
                msg = WebBaseEvent.FormatResourceStringWithCache(SR.Webevent_msg_AuditUnhandledAccessException);
                break;
                
            default:
                Debug.Assert(false, "ASP.NET event code " + eventCode.ToString(CultureInfo.InvariantCulture) + " doesn't have message string mapped to it");
                return String.Empty;
            }

            if (detailMsg != null) {
                msg += " " + detailMsg;
            }

            return msg;
        }

        // Both WebBaseEvents and HealthMonitoringSectionHelper has to store information per {event type, event code}.
        // But for system event type, eventCode and event type has a N:1 relationship.  Meaning every event
        // code can be mapped to one and only one event type.  So instead of using {event type, event code} as
        // the key, we can use just the event code as the key.

        // The simplest way is to use a hashtable.  But in order to boost performance, we store those
        // information using an array with event code as the key.  However, because the event code range is not 
        // continuous, and has large gap between categories, instead we use an NxM array, when N is number 
        // of major event code categories (e.g. ApplicationCodeBase and RequestCodeBase), and M is the 
        // max number of per category event code among all the catogories.

        // WebBaseEvents and HealthMonitoringSectionHelper will each maintain its own NxM arrays, and it
        // depends on the following functions to calculate the sizes of the array, and to convert an event
        // code into a (x,y) coordinate.
        
        internal static int[] s_eventArrayDimensionSizes = new int[2];

        internal static int GetEventArrayDimensionSize(int dim) {
            Debug.Assert(dim == 0 || dim == 1, "dim == 0 || dim == 1");

            return s_eventArrayDimensionSizes[dim];
        }

        // Convert an event code into a (x,y) coordinate.
        internal static void GetEventArrayIndexsFromEventCode(int eventCode, out int index0, out int index1) {
            index0 = eventCode/1000 - 1;
            index1 = eventCode - (eventCode/1000)*1000 - 1;

            Debug.Assert(index0 >= 0 && index0 < GetEventArrayDimensionSize(0), "Index0 of system eventCode out of expected range: " + eventCode);
            Debug.Assert(index1 >= 0 && index1 < GetEventArrayDimensionSize(1), "Index1 of system eventCode out of expected range: " + eventCode);
        }
        
        static void InitEventArrayDimensions()
        {
            int sizeOf2ndDim = 0;
            int size;

            // Below is the manual way to figure out the size of the 2nd dimension.

            size = WebEventCodes.ApplicationCodeBaseLast - WebEventCodes.ApplicationCodeBase;
            if (size > sizeOf2ndDim) {
                sizeOf2ndDim = size;
            }

            size = WebEventCodes.RequestCodeBaseLast - WebEventCodes.RequestCodeBase;
            if (size > sizeOf2ndDim) {
                sizeOf2ndDim = size;
            }

            size = WebEventCodes.ErrorCodeBaseLast - WebEventCodes.ErrorCodeBase;
            if (size > sizeOf2ndDim) {
                sizeOf2ndDim = size;
            }

            size = WebEventCodes.AuditCodeBaseLast - WebEventCodes.AuditCodeBase;
            if (size > sizeOf2ndDim) {
                sizeOf2ndDim = size;
            }

            size = WebEventCodes.MiscCodeBaseLast - WebEventCodes.MiscCodeBase;
            if (size > sizeOf2ndDim) {
                sizeOf2ndDim = size;
            }

            s_eventArrayDimensionSizes[0] = WebEventCodes.LastCodeBase/1000;
            s_eventArrayDimensionSizes[1] = sizeOf2ndDim;
        }
    }
}

