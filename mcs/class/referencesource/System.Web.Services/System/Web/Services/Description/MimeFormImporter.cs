//------------------------------------------------------------------------------
// <copyright file="MimeFormImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.Services.Description {
    using System.Web.Services.Protocols;
    using System.Globalization;
    
    internal class MimeFormImporter : MimeImporter {

        internal override MimeParameterCollection ImportParameters() {
            MimeContentBinding mimeContentBinding = (MimeContentBinding)ImportContext.OperationBinding.Input.Extensions.Find(typeof(MimeContentBinding));
            if (mimeContentBinding == null) return null;
            if (string.Compare(mimeContentBinding.Type, HtmlFormParameterReader.MimeType, StringComparison.OrdinalIgnoreCase) != 0) return null;
            MimeParameterCollection parameters = ImportContext.ImportStringParametersMessage();
            if (parameters == null) return null;
            parameters.WriterType = typeof(HtmlFormParameterWriter);
            return parameters;
        }

        internal override MimeReturn ImportReturn() {
            return null;
        }
    }
}

