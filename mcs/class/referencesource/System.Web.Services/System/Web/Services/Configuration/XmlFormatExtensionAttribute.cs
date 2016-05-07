//------------------------------------------------------------------------------
// <copyright file="XmlFormatExtensionAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Configuration {

    using System;

    /// <include file='doc\XmlFormatExtensionAttribute.uex' path='docs/doc[@for="XmlFormatExtensionAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class XmlFormatExtensionAttribute : Attribute {
        Type[] types;
        string name;
        string ns;

        /// <include file='doc\XmlFormatExtensionAttribute.uex' path='docs/doc[@for="XmlFormatExtensionAttribute.XmlFormatExtensionAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlFormatExtensionAttribute() {
        }

        /// <include file='doc\XmlFormatExtensionAttribute.uex' path='docs/doc[@for="XmlFormatExtensionAttribute.XmlFormatExtensionAttribute2"]/*' />
        public XmlFormatExtensionAttribute(string elementName, string ns, Type extensionPoint1) : this(elementName, ns, new Type[] { extensionPoint1 }) {
        }
        /// <include file='doc\XmlFormatExtensionAttribute.uex' path='docs/doc[@for="XmlFormatExtensionAttribute.XmlFormatExtensionAttribute3"]/*' />
        public XmlFormatExtensionAttribute(string elementName, string ns, Type extensionPoint1, Type extensionPoint2) : this(elementName, ns, new Type[] { extensionPoint1, extensionPoint2 }) {
        }
        /// <include file='doc\XmlFormatExtensionAttribute.uex' path='docs/doc[@for="XmlFormatExtensionAttribute.XmlFormatExtensionAttribute4"]/*' />
        public XmlFormatExtensionAttribute(string elementName, string ns, Type extensionPoint1, Type extensionPoint2, Type extensionPoint3) : this(elementName, ns, new Type[] { extensionPoint1, extensionPoint2, extensionPoint3 }) {
        }
        /// <include file='doc\XmlFormatExtensionAttribute.uex' path='docs/doc[@for="XmlFormatExtensionAttribute.XmlFormatExtensionAttribute5"]/*' />
        public XmlFormatExtensionAttribute(string elementName, string ns, Type extensionPoint1, Type extensionPoint2, Type extensionPoint3, Type extensionPoint4) : this(elementName, ns, new Type[] { extensionPoint1, extensionPoint2, extensionPoint3, extensionPoint4 }) {
        }

        /// <include file='doc\XmlFormatExtensionAttribute.uex' path='docs/doc[@for="XmlFormatExtensionAttribute.XmlFormatExtensionAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlFormatExtensionAttribute(string elementName, string ns, Type[] extensionPoints) {
            this.name = elementName;
            this.ns = ns;
            this.types = extensionPoints;
        }

        /// <include file='doc\XmlFormatExtensionAttribute.uex' path='docs/doc[@for="XmlFormatExtensionAttribute.ExtensionPoints"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Type[] ExtensionPoints {
            get { return types == null ? new Type[0] : types; }
            set { types = value; }
        }

        /// <include file='doc\XmlFormatExtensionAttribute.uex' path='docs/doc[@for="XmlFormatExtensionAttribute.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return ns == null ? string.Empty : ns; }
            set { ns = value; }
        }

        /// <include file='doc\XmlFormatExtensionAttribute.uex' path='docs/doc[@for="XmlFormatExtensionAttribute.ElementName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ElementName {
            get { return name == null ? string.Empty : name; }
            set { name = value; }
        }
    }

}
