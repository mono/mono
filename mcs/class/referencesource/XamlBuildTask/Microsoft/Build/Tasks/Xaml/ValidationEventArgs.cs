//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;

    internal class ValidationEventArgs : EventArgs
    {
        public ValidationEventArgs(string message, int lineNumber, int linePosition)
        {
            Message = message;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        public string Message 
        { get; private set; }
        
        public int LineNumber 
        { get; private set; }
        
        public int LinePosition 
        { get; private set; }
    }
}
