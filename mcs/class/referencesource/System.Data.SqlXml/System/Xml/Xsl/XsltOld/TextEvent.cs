//------------------------------------------------------------------------------
// <copyright file="TextEvent.cs" company="Microsoft">
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

    internal class TextEvent : Event {
        private string text;
        
        protected TextEvent() {}

        public TextEvent(string text) {
            Debug.Assert(text != null);
            this.text = text;
        }

        public TextEvent(Compiler compiler) {
            NavigatorInput input = compiler.Input;
            Debug.Assert(input.NodeType == XPathNodeType.Text || input.NodeType == XPathNodeType.SignificantWhitespace);
            this.text = input.Value;
        }

        public override bool Output(Processor processor, ActionFrame frame) {
            return processor.TextEvent(this.text);
        }

        public virtual string Evaluate(Processor processor, ActionFrame frame) {
            return this.text;
        }
    }
}
