//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Runtime;

    [Serializable]
    [Fx.Tag.XamlVisible(false)]
    public class XamlLoadErrorInfo
    {
        public XamlLoadErrorInfo(string message, int lineNumber, int linePosition)
        {
            this.Message = message;
            this.LineNumber = lineNumber;
            this.LinePosition = linePosition;
        }

        public int LineNumber { get; private set; }

        public int LinePosition { get; private set; }

        public string Message { get; private set; }

        public string FileName { get; set; }
    }
}
