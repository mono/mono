//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities;
 
    [Serializable]
    public class TextExpressionCompilerError
    {
        internal TextExpressionCompilerError()
        {
        }

        public bool IsWarning
        {
            get;
            internal set;
        }

        public int SourceLineNumber
        {
            get;
            internal set;
        }
        
        public string Message
        {
            get;
            internal set;
        }
        
        public string Number
        {
            get;
            internal set;
        }
    }
}
