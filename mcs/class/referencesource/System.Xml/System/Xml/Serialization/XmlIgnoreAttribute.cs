//------------------------------------------------------------------------------
// <copyright file="XmlIgnoreAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System;

    /// <include file='doc\XmlIgnoreAttribute.uex' path='docs/doc[@for="XmlIgnoreAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class XmlIgnoreAttribute : System.Attribute {
        /// <include file='doc\XmlIgnoreAttribute.uex' path='docs/doc[@for="XmlIgnoreAttribute.XmlIgnoreAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlIgnoreAttribute() {
        }
    }
}
