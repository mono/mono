//------------------------------------------------------------------------------
// <copyright file="EndEvent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XsltOld {
    using Res = System.Xml.Utils.Res;
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.XPath;

    internal class EndEvent : Event {
        private XPathNodeType nodeType;
        
        internal EndEvent(XPathNodeType nodeType) {
            Debug.Assert(nodeType != XPathNodeType.Namespace);
            this.nodeType = nodeType;
        }

        public override bool Output(Processor processor, ActionFrame frame) {
            return processor.EndEvent(this.nodeType);
        }
    }
}
