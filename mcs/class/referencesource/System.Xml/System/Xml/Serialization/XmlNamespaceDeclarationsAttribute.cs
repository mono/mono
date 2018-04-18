//------------------------------------------------------------------------------
// <copyright file="XmlNamespaceDeclarationsAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;
    using System.Xml.Schema;

    /// <include file='doc\XmlNamespaceDeclarationsAttribute.uex' path='docs/doc[@for="XmlNamespaceDeclarationsAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple=false)]
    public class XmlNamespaceDeclarationsAttribute : System.Attribute {

        /// <include file='doc\XmlNamespaceDeclarationsAttribute.uex' path='docs/doc[@for="XmlNamespaceDeclarationsAttribute.XmlNamespaceDeclarationsAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlNamespaceDeclarationsAttribute() {
        }
       
    }
    
}
