//------------------------------------------------------------------------------
// <copyright file="XmlFormatExtensionPointAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Configuration {

    using System;

    /// <include file='doc\XmlFormatExtensionPointAttribute.uex' path='docs/doc[@for="XmlFormatExtensionPointAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class XmlFormatExtensionPointAttribute : Attribute {
        string name;
        bool allowElements = true;

        /// <include file='doc\XmlFormatExtensionPointAttribute.uex' path='docs/doc[@for="XmlFormatExtensionPointAttribute.XmlFormatExtensionPointAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlFormatExtensionPointAttribute(string memberName) {
            this.name = memberName;
        }

        /// <include file='doc\XmlFormatExtensionPointAttribute.uex' path='docs/doc[@for="XmlFormatExtensionPointAttribute.MemberName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string MemberName {
            get { return name == null ? string.Empty : name; }
            set { name = value; }
        }

        /// <include file='doc\XmlFormatExtensionPointAttribute.uex' path='docs/doc[@for="XmlFormatExtensionPointAttribute.AllowElements"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool AllowElements {
            get { return allowElements; }
            set { allowElements = value; }
        }
    }


}
