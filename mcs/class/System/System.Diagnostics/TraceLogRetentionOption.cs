namespace System.Diagnostics
{
    using System;

    public enum TraceLogRetentionOption
    {
        UnlimitedSequentialFiles,
        LimitedCircularFiles,
        SingleFileUnboundedSize,
        LimitedSequentialFiles,
        SingleFileBoundedSize
    }
}

