//------------------------------------------------------------------------------
// <copyright file="XmlAnyAttributeAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;
    using System.Xml.Schema;

    /// <include file='doc\XmlAnyAttributeAttribute.uex' path='docs/doc[@for="XmlAnyAttributeAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple=false)]
    public class XmlAnyAttributeAttribute : System.Attribute {

        /// <include file='doc\XmlAnyAttributeAttribute.uex' path='docs/doc[@for="XmlAnyAttributeAttribute.XmlAnyAttributeAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlAnyAttributeAttribute() {
        }
       
    }
    
}
