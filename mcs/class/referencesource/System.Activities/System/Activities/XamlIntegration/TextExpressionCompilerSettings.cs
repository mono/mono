//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;

    public class TextExpressionCompilerSettings
    {
        public TextExpressionCompilerSettings()
        {
            this.GenerateAsPartialClass = true;
            this.AlwaysGenerateSource = true;
            this.ForImplementation = true;
        }

        public Activity Activity
        {
            get;
            set;
        }
        
        public string ActivityName
        {
            get;
            set;
        }
        
        public string ActivityNamespace
        {
            get;
            set;
        }

        public bool AlwaysGenerateSource
        {
            get;
            set;
        }

        public bool ForImplementation
        {
            get;
            set;
        }

        public string Language
        {
            get;
            set;
        }
        
        public string RootNamespace
        {
            get;
            set;
        }
                        
        public Action<string> LogSourceGenerationMessage
        {
            get;
            set;
        }

        public bool GenerateAsPartialClass
        {
            get;
            set;
        }
    }
}
