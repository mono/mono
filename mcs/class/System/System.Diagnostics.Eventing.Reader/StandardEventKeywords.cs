namespace System.Diagnostics.Eventing.Reader
{
    using System;

    [Flags]
    public enum StandardEventKeywords : long
    {
        AuditFailure = 0x10000000000000L,
        AuditSuccess = 0x20000000000000L,
        [Obsolete("Incorrect value: use CorrelationHint2 instead", false)]
        CorrelationHint = 0x10000000000000L,
        CorrelationHint2 = 0x40000000000000L,
        EventLogClassic = 0x80000000000000L,
        None = 0L,
        ResponseTime = 0x1000000000000L,
        Sqm = 0x8000000000000L,
        WdiContext = 0x2000000000000L,
        WdiDiagnostic = 0x4000000000000L
    }
}

