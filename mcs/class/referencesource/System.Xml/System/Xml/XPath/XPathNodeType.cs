//------------------------------------------------------------------------------
// <copyright file="XPathNodeType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">sdub</owner>
//------------------------------------------------------------------------------

namespace System.Xml.XPath {
    using System;

    public enum XPathNodeType {
        Root,
        Element,
        Attribute,
        Namespace,
        Text,
        SignificantWhitespace,
        Whitespace,
        ProcessingInstruction,
        Comment,
        All,
    }
}
