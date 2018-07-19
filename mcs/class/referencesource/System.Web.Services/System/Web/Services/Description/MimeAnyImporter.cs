//------------------------------------------------------------------------------
// <copyright file="MimeAnyImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.IO;


    internal class MimeAnyImporter : MimeImporter {

        internal override MimeParameterCollection ImportParameters() {
            return null;
        }

        internal override MimeReturn ImportReturn() {
            if (ImportContext.OperationBinding.Output.Extensions.Count == 0) return null;
            MimeReturn importedReturn = new MimeReturn();
            importedReturn.TypeName = typeof(Stream).FullName;
            importedReturn.ReaderType = typeof(AnyReturnReader);
            return importedReturn;
        }
    }
}
