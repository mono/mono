//------------------------------------------------------------------------------
// <copyright file="XPathAxis.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">sdub</owner>
//------------------------------------------------------------------------------

namespace System.Xml.Xsl.XPath {

    // Order is important - we use them as an index in QilAxis & AxisMask arrays
    internal enum XPathAxis {
        Unknown      = 0,
        Ancestor        ,
        AncestorOrSelf  ,
        Attribute       ,
        Child           ,
        Descendant      ,
        DescendantOrSelf,
        Following       ,
        FollowingSibling,
        Namespace       ,
        Parent          ,
        Preceding       ,
        PrecedingSibling,
        Self            ,
        Root            ,
    }
}
