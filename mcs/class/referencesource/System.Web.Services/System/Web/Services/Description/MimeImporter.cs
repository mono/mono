//------------------------------------------------------------------------------
// <copyright file="MimeImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System;
    using System.Reflection;
    using System.CodeDom;

    internal abstract class MimeImporter {
        HttpProtocolImporter protocol;

        internal abstract MimeParameterCollection ImportParameters();
        internal abstract MimeReturn ImportReturn();

        internal virtual void GenerateCode(MimeReturn[] importedReturns, MimeParameterCollection[] importedParameters) {
        }

        internal virtual void AddClassMetadata(CodeTypeDeclaration codeClass) {
        }

        internal HttpProtocolImporter ImportContext {
            get { return protocol; }
            set { protocol = value; }
        }
    }
}
