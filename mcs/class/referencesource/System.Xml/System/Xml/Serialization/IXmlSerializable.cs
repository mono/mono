//------------------------------------------------------------------------------
// <copyright file="IXmlSerializable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System.Xml.Schema;

    /// <include file='doc\IXmlSerializable.uex' path='docs/doc[@for="IXmlSerializable"]/*' />
    ///<internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public interface IXmlSerializable {
        /// <include file='doc\IXmlSerializable.uex' path='docs/doc[@for="IXmlSerializable.GetSchema"]/*' />
        XmlSchema GetSchema();
        /// <include file='doc\IXmlSerializable.uex' path='docs/doc[@for="IXmlSerializable.ReadXml"]/*' />
        void ReadXml(XmlReader reader);
        /// <include file='doc\IXmlSerializable.uex' path='docs/doc[@for="IXmlSerializable.WriteXml"]/*' />
        void WriteXml(XmlWriter writer);
    }

}
