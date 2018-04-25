//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    static class DiagnosticsTraceCode 
    {
        // Diagnostic trace codes
        public const int Diagnostics = 0X20000;
        public const int AppDomainUnload = DiagnosticsTraceCode.Diagnostics | 0X0001; //
        public const int EventLog = DiagnosticsTraceCode.Diagnostics | 0X0002; //
        public const int ThrowingException = DiagnosticsTraceCode.Diagnostics | 0X0003; //
        public const int TraceHandledException = DiagnosticsTraceCode.Diagnostics | 0X0004; //
        public const int UnhandledException = DiagnosticsTraceCode.Diagnostics | 0X0005; //
        public const int TraceTruncatedQuotaExceeded = DiagnosticsTraceCode.Diagnostics | 0X000C; //
    }
}
