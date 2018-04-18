//------------------------------------------------------------------------------
// <copyright file="Operand.cs" company="Microsoft">
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

    internal class Operand : AstNode {
        private XPathResultType type;
        private object val;

        public Operand(string val) {
            this.type = XPathResultType.String;
            this.val = val;
        }

        public Operand(double val) {
            this.type = XPathResultType.Number;
            this.val = val;
        }

        public Operand(bool val) {
            this.type = XPathResultType.Boolean;
            this.val = val;
        }

        public override AstType         Type       { get { return AstType.ConstantOperand; } }
        public override XPathResultType ReturnType { get { return type;                    } }

        public object OperandValue { get { return val; } }
    }
}
