//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Serialization.Diagnostics
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;

    static class TraceUtility
    {
        static Dictionary<int, string> traceCodes = new Dictionary<int, string>(18)
        {
            { TraceCode.WriteObjectBegin, "WriteObjectBegin" },
            { TraceCode.WriteObjectEnd, "WriteObjectEnd" },
            { TraceCode.WriteObjectContentBegin, "WriteObjectContentBegin" },
            { TraceCode.WriteObjectContentEnd, "WriteObjectContentEnd" },
            { TraceCode.ReadObjectBegin, "ReadObjectBegin" },
            { TraceCode.ReadObjectEnd, "ReadObjectEnd" },
            { TraceCode.ElementIgnored, "ElementIgnored" },
            { TraceCode.XsdExportBegin, "XsdExportBegin" },
            { TraceCode.XsdExportEnd, "XsdExportEnd" },
            { TraceCode.XsdImportBegin, "XsdImportBegin" },
            { TraceCode.XsdImportEnd, "XsdImportEnd" },
            { TraceCode.XsdExportError, "XsdExportError" },
            { TraceCode.XsdImportError, "XsdImportError" },
            { TraceCode.XsdExportAnnotationFailed, "XsdExportAnnotationFailed" },
            { TraceCode.XsdImportAnnotationFailed, "XsdImportAnnotationFailed" },
            { TraceCode.XsdExportDupItems, "XsdExportDupItems" },
            { TraceCode.FactoryTypeNotFound, "FactoryTypeNotFound" },
            { TraceCode.ObjectWithLargeDepth, "ObjectWithLargeDepth" },
        };

        internal static void Trace(TraceEventType severity, int traceCode, string traceDescription)
        {
            Trace(severity, traceCode, traceDescription, null);
        }

        internal static void Trace(TraceEventType severity, int traceCode, string traceDescription, TraceRecord record)
        {
            Trace(severity, traceCode, traceDescription, record, null);
        }

        internal static void Trace(TraceEventType severity, int traceCode, string traceDescription, TraceRecord record, Exception exception)
        {
            Fx.Assert(traceCodes.ContainsKey(traceCode), 
                string.Format(CultureInfo.InvariantCulture, "Unsupported trace code: Please add trace code 0x{0} to the dictionary TraceUtility.traceCodes in {1}", 
                traceCode.ToString("X", CultureInfo.InvariantCulture), typeof(TraceUtility)));
            string msdnTraceCode = System.ServiceModel.Diagnostics.LegacyDiagnosticTrace.GenerateMsdnTraceCode("System.Runtime.Serialization", traceCodes[traceCode]);
            DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, msdnTraceCode, traceDescription, record, exception, null);
        }
    }
}
