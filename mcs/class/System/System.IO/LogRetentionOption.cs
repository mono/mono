namespace System.IO
{
    using System;

    internal enum LogRetentionOption
    {
        UnlimitedSequentialFiles,
        LimitedCircularFiles,
        SingleFileUnboundedSize,
        LimitedSequentialFiles,
        SingleFileBoundedSize
    }
}

