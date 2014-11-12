//------------------------------------------------------------------------------
// <copyright file="IHasXmlNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml {
    /// <include file='doc\IHasXmlNode.uex' path='docs/doc[@for="IHasXmlNode"]/*' />
    public interface IHasXmlNode {
        /// <include file='doc\IHasXmlNode.uex' path='docs/doc[@for="IHasXmlNode.GetNode"]/*' />
        XmlNode GetNode();
    }
}
