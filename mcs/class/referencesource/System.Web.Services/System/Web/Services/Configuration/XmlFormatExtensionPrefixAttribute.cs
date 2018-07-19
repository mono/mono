//------------------------------------------------------------------------------
// <copyright file="XmlFormatExtensionPrefixAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Configuration {

    using System;

    /// <include file='doc\XmlFormatExtensionPrefixAttribute.uex' path='docs/doc[@for="XmlFormatExtensionPrefixAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class XmlFormatExtensionPrefixAttribute : Attribute {
        string prefix;
        string ns;

        /// <include file='doc\XmlFormatExtensionPrefixAttribute.uex' path='docs/doc[@for="XmlFormatExtensionPrefixAttribute.XmlFormatExtensionPrefixAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlFormatExtensionPrefixAttribute() {
        }

        /// <include file='doc\XmlFormatExtensionPrefixAttribute.uex' path='docs/doc[@for="XmlFormatExtensionPrefixAttribute.XmlFormatExtensionPrefixAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlFormatExtensionPrefixAttribute(string prefix, string ns) {
            this.prefix = prefix;
            this.ns = ns;
        }

        /// <include file='doc\XmlFormatExtensionPrefixAttribute.uex' path='docs/doc[@for="XmlFormatExtensionPrefixAttribute.Prefix"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Prefix {
            get { return prefix == null ? string.Empty : prefix; }
            set { prefix = value; }
        }

        /// <include file='doc\XmlFormatExtensionPrefixAttribute.uex' path='docs/doc[@for="XmlFormatExtensionPrefixAttribute.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return ns == null ? string.Empty : ns; }
            set { ns = value; }
        }
    }
}
