//------------------------------------------------------------------------------
// <copyright file="XmlSchemaForm.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Xml.Serialization;

    // if change the enum, have to change xsdbuilder as well.
    /// <include file='doc\XmlSchemaForm.uex' path='docs/doc[@for="XmlSchemaForm"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public enum XmlSchemaForm {
        /// <include file='doc\XmlSchemaForm.uex' path='docs/doc[@for="XmlSchemaForm.XmlEnum"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        None,
        /// <include file='doc\XmlSchemaForm.uex' path='docs/doc[@for="XmlSchemaForm.XmlEnum1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlEnum("qualified")]
        Qualified,
        /// <include file='doc\XmlSchemaForm.uex' path='docs/doc[@for="XmlSchemaForm.XmlEnum2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlEnum("unqualified")]
        Unqualified,
    }
}
