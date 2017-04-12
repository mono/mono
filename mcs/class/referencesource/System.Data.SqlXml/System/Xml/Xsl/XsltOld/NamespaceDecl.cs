//------------------------------------------------------------------------------
// <copyright file="NamespaceDecl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Xml;

    internal class NamespaceDecl {
        private string        prefix;
        private string        nsUri;
        private string        prevDefaultNsUri;
        private NamespaceDecl next;

        internal string Prefix {
            get { return this.prefix; }
        }

        internal string Uri {
            get { return this.nsUri; }
        }

        internal string PrevDefaultNsUri {
            get { return this.prevDefaultNsUri; }
        }

        internal NamespaceDecl Next {
            get { return this.next; }
        }

        internal NamespaceDecl(string prefix, string nsUri, string prevDefaultNsUri, NamespaceDecl next) {
            Init(prefix, nsUri, prevDefaultNsUri, next);
        }

        internal void Init(string prefix, string nsUri, string prevDefaultNsUri, NamespaceDecl next) {
            this.prefix           = prefix;
            this.nsUri            = nsUri;
            this.prevDefaultNsUri = prevDefaultNsUri;
            this.next             = next;
        }
    }
}
