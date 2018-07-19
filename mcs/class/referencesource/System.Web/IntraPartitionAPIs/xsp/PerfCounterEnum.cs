//------------------------------------------------------------------------------
// <copyright from='1997' to='2001' company='Microsoft Corporation'>           
//    Copyright (c) Microsoft Corporation. All Rights Reserved.                
//    Information Contained Herein is Proprietary and Confidential.            
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * PerfCounters class
 */
namespace System.Web {

    // Global enums
    internal enum GlobalPerfCounter {

        APPLICATION_RESTARTS                               = 0,
        APPLICATIONS_RUNNING                               = 1,
        REQUESTS_DISCONNECTED                              = 2,
        REQUEST_EXECUTION_TIME                             = 3,
        REQUESTS_REJECTED                                  = 4,
        REQUESTS_QUEUED                                    = 5,
        WPS_RUNNING                                        = 6,
        WPS_RESTARTS                                       = 7,
        REQUEST_WAIT_TIME                                  = 8,
        STATE_SERVER_SESSIONS_ACTIVE                       = 9,
        STATE_SERVER_SESSIONS_ABANDONED                    = 10,
        STATE_SERVER_SESSIONS_TIMED_OUT                    = 11,
        STATE_SERVER_SESSIONS_TOTAL                        = 12,
        REQUESTS_CURRENT                                   = 13,
        GLOBAL_AUDIT_SUCCESS                               = 14,
        GLOBAL_AUDIT_FAIL                                  = 15,
        GLOBAL_EVENTS_ERROR                                = 16,
        GLOBAL_EVENTS_HTTP_REQ_ERROR                       = 17,
        GLOBAL_EVENTS_HTTP_INFRA_ERROR                     = 18,
        REQUESTS_IN_NATIVE_QUEUE                           = 19,

    }

    internal enum AppPerfCounter {

        ANONYMOUS_REQUESTS                                 = 20,
        TOTAL_CACHE_ENTRIES                                = 21,
        TOTAL_CACHE_TURNOVER_RATE                          = 22,
        TOTAL_CACHE_HITS                                   = 23,
        TOTAL_CACHE_MISSES                                 = 24,
        TOTAL_CACHE_RATIO_BASE                             = 25,
        API_CACHE_ENTRIES                                  = 26,
        API_CACHE_TURNOVER_RATE                            = 27,
        API_CACHE_HITS                                     = 28,
        API_CACHE_MISSES                                   = 29,
        API_CACHE_RATIO_BASE                               = 30,
        OUTPUT_CACHE_ENTRIES                               = 31,
        OUTPUT_CACHE_TURNOVER_RATE                         = 32,
        OUTPUT_CACHE_HITS                                  = 33,
        OUTPUT_CACHE_MISSES                                = 34,
        OUTPUT_CACHE_RATIO_BASE                            = 35,
        COMPILATIONS                                       = 36,
        DEBUGGING_REQUESTS                                 = 37,
        ERRORS_PRE_PROCESSING                              = 38,
        ERRORS_COMPILING                                   = 39,
        ERRORS_DURING_REQUEST                              = 40,
        ERRORS_UNHANDLED                                   = 41,
        ERRORS_TOTAL                                       = 42,
        PIPELINES                                          = 43,
        REQUEST_BYTES_IN                                   = 44,
        REQUEST_BYTES_OUT                                  = 45,
        REQUESTS_EXECUTING                                 = 46,
        REQUESTS_FAILED                                    = 47,
        REQUESTS_NOT_FOUND                                 = 48,
        REQUESTS_NOT_AUTHORIZED                            = 49,
        REQUESTS_IN_APPLICATION_QUEUE                      = 50,
        REQUESTS_TIMED_OUT                                 = 51,
        REQUESTS_SUCCEDED                                  = 52,
        REQUESTS_TOTAL                                     = 53,
        SESSIONS_ACTIVE                                    = 54,
        SESSIONS_ABANDONED                                 = 55,
        SESSIONS_TIMED_OUT                                 = 56,
        SESSIONS_TOTAL                                     = 57,
        TRANSACTIONS_ABORTED                               = 58,
        TRANSACTIONS_COMMITTED                             = 59,
        TRANSACTIONS_PENDING                               = 60,
        TRANSACTIONS_TOTAL                                 = 61,
        SESSION_STATE_SERVER_CONNECTIONS                   = 62,
        SESSION_SQL_SERVER_CONNECTIONS                     = 63,
        EVENTS_TOTAL                                       = 64,
        EVENTS_APP                                         = 65,
        EVENTS_ERROR                                       = 66,
        EVENTS_HTTP_REQ_ERROR                              = 67,
        EVENTS_HTTP_INFRA_ERROR                            = 68,
        EVENTS_WEB_REQ                                     = 69,
        AUDIT_SUCCESS                                      = 70,
        AUDIT_FAIL                                         = 71,
        MEMBER_SUCCESS                                     = 72,
        MEMBER_FAIL                                        = 73,
        FORMS_AUTH_SUCCESS                                 = 74,
        FORMS_AUTH_FAIL                                    = 75,
        VIEWSTATE_MAC_FAIL                                 = 76,
        APP_REQUEST_EXEC_TIME                              = 77,
        APP_REQUEST_DISCONNECTED                           = 78,
        APP_REQUESTS_REJECTED                              = 79,
        APP_REQUEST_WAIT_TIME                              = 80,
        CACHE_PERCENT_MACH_MEM_LIMIT_USED                  = 81,
        CACHE_PERCENT_MACH_MEM_LIMIT_USED_BASE             = 82,
        CACHE_PERCENT_PROC_MEM_LIMIT_USED                  = 83,
        CACHE_PERCENT_PROC_MEM_LIMIT_USED_BASE             = 84,
        CACHE_TOTAL_TRIMS                                  = 85,
        CACHE_API_TRIMS                                    = 86,
        CACHE_OUTPUT_TRIMS                                 = 87,
        APP_CPU_USED                                       = 88,
        APP_CPU_USED_BASE                                  = 89,
        APP_MEMORY_USED                                    = 90,
        REQUEST_BYTES_IN_WEBSOCKETS                        = 91,
        REQUEST_BYTES_OUT_WEBSOCKETS                       = 92,
        REQUESTS_EXECUTING_WEBSOCKETS                      = 93,
        REQUESTS_FAILED_WEBSOCKETS                         = 94,
        REQUESTS_SUCCEEDED_WEBSOCKETS                      = 95,
        REQUESTS_TOTAL_WEBSOCKETS                          = 96,

    }

    // StateService enums
    internal enum StateServicePerfCounter {

        STATE_SERVICE_SESSIONS_ACTIVE                      = 97,
        STATE_SERVICE_SESSIONS_ABANDONED                   = 98,
        STATE_SERVICE_SESSIONS_TIMED_OUT                   = 99,
        STATE_SERVICE_SESSIONS_TOTAL                       = 100,

    }
}    

