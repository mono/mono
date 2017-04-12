//------------------------------------------------------------------------------
// <copyright file="SoapSchemaMember.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System;

    /// <include file='doc\SoapSchemaMember.uex' path='docs/doc[@for="SoapSchemaMember"]/*' />
    /// <internalonly/>
    public class SoapSchemaMember {
        string memberName;
        XmlQualifiedName type = XmlQualifiedName.Empty;

        /// <include file='doc\SoapSchemaMember.uex' path='docs/doc[@for="SoapSchemaMember.MemberType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlQualifiedName MemberType {
            get { return type; }
            set { type = value; }
        }

        /// <include file='doc\SoapSchemaMember.uex' path='docs/doc[@for="SoapSchemaMember.MemberName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string MemberName {
            get { return memberName == null ? string.Empty : memberName; }
            set { memberName = value; }
        }
    }


}
