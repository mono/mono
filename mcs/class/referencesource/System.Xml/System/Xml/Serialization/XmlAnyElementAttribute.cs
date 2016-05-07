//------------------------------------------------------------------------------
// <copyright file="XmlAnyElementAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;
    using System.Xml.Schema;

    /// <include file='doc\XmlAnyElementAttribute.uex' path='docs/doc[@for="XmlAnyElementAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple=true)]
    public class XmlAnyElementAttribute : System.Attribute {
        string name;
        string ns;
        int order = -1;
        bool nsSpecified = false;

        /// <include file='doc\XmlAnyElementAttribute.uex' path='docs/doc[@for="XmlAnyElementAttribute.XmlAnyElementAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlAnyElementAttribute() {
        }
        
        /// <include file='doc\XmlAnyElementAttribute.uex' path='docs/doc[@for="XmlAnyElementAttribute.XmlAnyElementAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlAnyElementAttribute(string name) {
            this.name = name;
        }

        /// <include file='doc\XmlAnyElementAttribute.uex' path='docs/doc[@for="XmlAnyElementAttribute.XmlAnyElementAttribute2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlAnyElementAttribute(string name, string ns) {
            this.name = name;
            this.ns = ns;
            nsSpecified = true;
        }
        
        /// <include file='doc\XmlAnyElementAttribute.uex' path='docs/doc[@for="XmlAnyElementAttribute.Name"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Name {
            get { return name == null ? string.Empty : name; }
            set { name = value; }
        }
        
        /// <include file='doc\XmlAnyElementAttribute.uex' path='docs/doc[@for="XmlAnyElementAttribute.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return ns; }
            set { 
                ns = value; 
                nsSpecified = true;
            }
        }

        /// <include file='doc\XmlAnyElementAttribute.uex' path='docs/doc[@for="XmlAnyElementAttribute.Order"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Order {
            get { return order; }
            set {
                if (value < 0)
                    throw new ArgumentException(Res.GetString(Res.XmlDisallowNegativeValues), "Order");
                order = value;
            }
        }

        internal bool NamespaceSpecified {
            get { return nsSpecified; }
        }
    }
}
