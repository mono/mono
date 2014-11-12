//------------------------------------------------------------------------------
// <copyright file="AstNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace MS.Internal.Xml.XPath {
    using System;
    using System.Xml.XPath;

    internal abstract class AstNode {
        public enum AstType {
            Axis            ,
            Operator        ,
            Filter          ,
            ConstantOperand ,
            Function        ,
            Group           ,
            Root            ,
            Variable        ,        
            Error           
        };

        public abstract AstType Type { get; }
        public abstract XPathResultType ReturnType { get; }
    }
}
