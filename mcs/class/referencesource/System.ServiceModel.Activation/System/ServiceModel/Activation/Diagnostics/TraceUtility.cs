//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Activation.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    
    static class TraceUtility
    {
        static Dictionary<int, string> traceCodes = new Dictionary<int, string>(7)
        {
            { TraceCode.WebHostFailedToCompile, "WebHostFailedToCompile" },
            { TraceCode.WebHostServiceActivated, "WebHostServiceActivated" },
            { TraceCode.WebHostFailedToActivateService, "WebHostFailedToActivateService" },
            { TraceCode.WebHostCompilation, "WebHostCompilation" },
            { TraceCode.WebHostDebugRequest, "WebHostDebugRequest" },
            { TraceCode.WebHostProtocolMisconfigured, "WebHostProtocolMisconfigured" },
            { TraceCode.WebHostServiceCloseFailed, "WebHostServiceCloseFailed" }, 
            { TraceCode.WebHostNoCBTSupport, "WebHostNoCBTSupport" },   
        };

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, object source)
        {
            TraceEvent(severity, traceCode, traceDescription, null, source, null);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, object source, Exception exception)
        {
            TraceEvent(severity, traceCode, traceDescription, null, source, exception);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, TraceRecord record, object source, Exception exception)
        {
            Fx.Assert(traceCodes.ContainsKey(traceCode), 
                string.Format(CultureInfo.InvariantCulture, "Unsupported trace code: Please add trace code 0x{0} to the dictionary TraceUtility.traceCodes in {1}", 
                traceCode.ToString("X", CultureInfo.InvariantCulture), typeof(TraceUtility)));
            string msdnTraceCode = System.ServiceModel.Diagnostics.LegacyDiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Activation", traceCodes[traceCode]);
            DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, msdnTraceCode, traceDescription, record, exception, source);
        }

        internal static string CreateSourceString(object source)
        {
            return source.GetType().ToString() + "/" + source.GetHashCode().ToString(CultureInfo.CurrentCulture);
        }
    }
}
