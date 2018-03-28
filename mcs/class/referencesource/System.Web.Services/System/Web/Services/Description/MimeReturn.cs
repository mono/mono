//------------------------------------------------------------------------------
// <copyright file="MimeReturn.cs" company="Microsoft">
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

    internal class MimeReturn {
        string typeName;
        Type readerType;
        CodeAttributeDeclarationCollection attrs;

        internal string TypeName {
            get { return typeName == null ? string.Empty : typeName; }
            set { typeName = value; }
        }

        internal Type ReaderType {
            get { return readerType; }
            set { readerType = value; }
        }

        internal CodeAttributeDeclarationCollection Attributes {
            get { 
                if (attrs == null)
                    attrs = new CodeAttributeDeclarationCollection();
                return attrs; 
            }
        }
    }
}
