//------------------------------------------------------------------------------
// <copyright file="DocumentScope.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.XPath;

    internal class DocumentScope {
        protected NamespaceDecl scopes;

        internal NamespaceDecl Scopes {
            get { return this.scopes; }
        }

        internal NamespaceDecl AddNamespace(string prefix, string uri, string prevDefaultNsUri) {
            this.scopes = new NamespaceDecl(prefix, uri, prevDefaultNsUri, this.scopes);
            return this.scopes;
        }

        internal string ResolveAtom(string prefix) {
            Debug.Assert(prefix != null && prefix.Length > 0);

            for (NamespaceDecl scope = this.scopes; scope != null; scope = scope.Next) {
                if (Ref.Equal(scope.Prefix, prefix)) {
                    Debug.Assert(scope.Uri != null);
                    return scope.Uri;
                }
            }

            return null;
        }

        internal string ResolveNonAtom(string prefix) {
            Debug.Assert(prefix != null && prefix.Length > 0);

            for (NamespaceDecl scope = this.scopes; scope != null; scope = scope.Next) {
                if (scope.Prefix == prefix) {
                    Debug.Assert(scope.Uri != null);
                    return scope.Uri;
                }
            }
            return null;
        }
    }
}
