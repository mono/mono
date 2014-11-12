//------------------------------------------------------------------------------
// <copyright file="XmlArrayAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System;
    using System.Xml.Schema;
    
    /// <include file='doc\XmlArrayAttribute.uex' path='docs/doc[@for="XmlArrayAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple=false)]
    public class XmlArrayAttribute : System.Attribute {
        string elementName;
        string ns;
        bool nullable;
        XmlSchemaForm form = XmlSchemaForm.None;
        int order = -1;
        
        /// <include file='doc\XmlArrayAttribute.uex' path='docs/doc[@for="XmlArrayAttribute.XmlArrayAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlArrayAttribute() {
        }
        
        /// <include file='doc\XmlArrayAttribute.uex' path='docs/doc[@for="XmlArrayAttribute.XmlArrayAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlArrayAttribute(string elementName) {
            this.elementName = elementName;
        }
        
        /// <include file='doc\XmlArrayAttribute.uex' path='docs/doc[@for="XmlArrayAttribute.ElementName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ElementName {
            get { return elementName == null ? string.Empty : elementName; }
            set { elementName = value; }
        }
    
        /// <include file='doc\XmlArrayAttribute.uex' path='docs/doc[@for="XmlArrayAttribute.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return ns; }
            set { ns = value; }
        }

        /// <include file='doc\XmlArrayAttribute.uex' path='docs/doc[@for="XmlArrayAttribute.IsNullable"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsNullable {
            get { return nullable; }
            set { nullable = value; }
        }

        /// <include file='doc\XmlArrayAttribute.uex' path='docs/doc[@for="XmlArrayAttribute.Form"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSchemaForm Form {
            get { return form; }
            set { form = value; }
        }

        /// <include file='doc\XmlArrayAttribute.uex' path='docs/doc[@for="XmlArrayAttribute.Order"]/*' />
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
    }
 }
