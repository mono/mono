//------------------------------------------------------------------------------
// <copyright file="XmlSchemaDerivationMethod.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>  
// <owner current="true" primary="true">[....]</owner>                                                              
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    public sealed class XmlSchemaCompilationSettings {

        bool enableUpaCheck;

        public XmlSchemaCompilationSettings() {
            enableUpaCheck = true;
        }

        public bool EnableUpaCheck {
            get {
                return enableUpaCheck;
            }
            set {
                enableUpaCheck = value;
            }
        }
    }
    
}
