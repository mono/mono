//------------------------------------------------------------------------------
// <copyright file="IXPathNavigable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml.XPath {
    using System;

    public interface IXPathNavigable {
        XPathNavigator CreateNavigator();
    }
}
