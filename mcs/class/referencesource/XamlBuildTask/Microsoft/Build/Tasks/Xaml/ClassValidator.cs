//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xaml;
    using System.Runtime;
    using System.Reflection;
    using System;
    using System.Xaml.Schema;
    using Microsoft.Build.Utilities;
    using XamlBuildTask;

    class ClassValidator
    {
        string xamlFileName;
        IList<LogData> eventArgs;
        Assembly localAssembly;
        string rootNamespace;

        public ClassValidator(string xamlFileName, Assembly localAssembly, string rootNamespace)
        {
            this.xamlFileName = xamlFileName;
            this.localAssembly = localAssembly;
            this.eventArgs = null;
            this.rootNamespace = rootNamespace;
        }

        public bool ValidateXaml(XamlReader xamlReader, bool failOnFirstError, string assemblyName, out IList<LogData> validationErrors)
        {
            if (xamlReader == null)
            {
                throw FxTrace.Exception.ArgumentNull("xamlReader");
            }
            validationErrors = null;

            // We loop through the provided XAML using a XamlValidatingReader to ensure that:
            //  1. XAML is valid.
            //  2. All types referenced in XAML are validate-able. At this point, any types defined in the local 
            //     assembly should be referenced, so this should be possible.
            XamlValidatingReader reader = new XamlValidatingReader(xamlReader, this.localAssembly, rootNamespace, assemblyName);
            reader.OnValidationError += new EventHandler<ValidationEventArgs>(reader_OnValidationError);
            while (reader.Read())
            {
                if (this.eventArgs != null && failOnFirstError)
                {
                    validationErrors = this.eventArgs;
                    return false;
                }
            }

            validationErrors = this.eventArgs;
            if (validationErrors != null && validationErrors.Count > 0)
            {
                return false;
            }
            return true;
        }

        void reader_OnValidationError(object sender, ValidationEventArgs e)
        {
            if (this.eventArgs == null)
            {
                this.eventArgs = new List<LogData>();
            }

            this.eventArgs.Add(new LogData()
            {
                FileName = this.xamlFileName,
                LineNumber = e.LineNumber,
                LinePosition = e.LinePosition,
                Message = e.Message
            });
        }
    }
}
