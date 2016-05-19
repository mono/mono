//------------------------------------------------------------------------------
// <copyright file="NameSpaceEvent.cs" company="Microsoft">
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

    internal class NamespaceEvent : Event {
        private string namespaceUri;
        private string name;
        
        public NamespaceEvent(NavigatorInput input) {
            Debug.Assert(input != null);
            Debug.Assert(input.NodeType == XPathNodeType.Namespace);
            this.namespaceUri = input.Value;
            this.name         = input.LocalName;
        }

        public override void ReplaceNamespaceAlias(Compiler compiler){
            if (this.namespaceUri.Length != 0) { // Do we need to check this for namespace?
                NamespaceInfo ResultURIInfo = compiler.FindNamespaceAlias(this.namespaceUri);
                if (ResultURIInfo != null) {
                    this.namespaceUri = ResultURIInfo.nameSpace;
                    if (ResultURIInfo.prefix != null) {
                        this.name = ResultURIInfo.prefix;
                    }
                }
            }
        }
        
        public override bool Output(Processor processor, ActionFrame frame) {
            bool res;
            res = processor.BeginEvent(XPathNodeType.Namespace, /*prefix:*/null, this.name, this.namespaceUri, /*empty:*/false);
            Debug.Assert(res); // Namespace node as any other attribute can't fail because it doesn't signal record change
            res = processor.EndEvent(XPathNodeType.Namespace);
            Debug.Assert(res);
            return true;
        }
    }
}
