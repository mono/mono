//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;

    [Serializable]
    internal struct LogData
    {
        public string Message
        { get; set; }

        public string FileName
        { get; set; }

        public int LineNumber
        { get; set; }

        public int LinePosition
        { get; set; }
    }
}
