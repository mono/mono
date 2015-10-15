//------------------------------------------------------------------------------
// <copyright file="ParentQuery.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace MS.Internal.Xml.XPath {
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Diagnostics;
    using System.Globalization;
    using System.Collections.Generic;

    internal sealed class ParentQuery : CacheAxisQuery {

        public ParentQuery(Query  qyInput, string Name, string Prefix, XPathNodeType Type) : base(qyInput, Name, Prefix, Type) {}
        private ParentQuery(ParentQuery other) : base(other) { }

        public override object Evaluate(XPathNodeIterator context) {
            base.Evaluate(context);

            // 
            XPathNavigator input;
            while ((input = qyInput.Advance()) != null) {
                input = input.Clone();

                if (input.MoveToParent()) {
                    if (matches(input)) {
                        Insert(outputBuffer, input);
                    }
                }
            }
            return this;
        }

        public override XPathNodeIterator Clone() { return new ParentQuery(this); }        
    }
}
