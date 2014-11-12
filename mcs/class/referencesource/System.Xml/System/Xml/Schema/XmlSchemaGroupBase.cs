//------------------------------------------------------------------------------
// <copyright file="XmlSchemaGroupbase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                 
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Xml.Serialization;

    /// <include file='doc\XmlSchemaGroupbase.uex' path='docs/doc[@for="XmlSchemaGroupBase"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class XmlSchemaGroupBase : XmlSchemaParticle {
        /// <include file='doc\XmlSchemaGroupbase.uex' path='docs/doc[@for="XmlSchemaGroupBase.Items"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public abstract XmlSchemaObjectCollection Items { get; }

        internal abstract void SetItems(XmlSchemaObjectCollection newItems);
    }
}
