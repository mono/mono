//------------------------------------------------------------------------------
// <copyright file="SchemaNotation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>  
// <owner current="true" primary="true">Microsoft</owner>                                                              
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System;
    using System.Diagnostics;

    internal sealed class SchemaNotation {
        internal const int   SYSTEM = 0;
        internal const int   PUBLIC = 1;

        private XmlQualifiedName  name;
        private String systemLiteral;   // System literal
        private String pubid;    // pubid literal

        internal SchemaNotation(XmlQualifiedName name) {
            this.name = name;
        }

        internal XmlQualifiedName Name {
            get { return name;}
        }

        internal String SystemLiteral {
            get { return systemLiteral;}
            set { systemLiteral = value;}
        }

        internal String Pubid {
            get { return pubid;}
            set { pubid = value;}
        }

    };

}
