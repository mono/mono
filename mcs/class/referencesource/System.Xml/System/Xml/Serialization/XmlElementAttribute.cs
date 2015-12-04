//------------------------------------------------------------------------------
// <copyright file="XmlElementAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;
    using System.Xml.Schema;

    /// <include file='doc\XmlElementAttribute.uex' path='docs/doc[@for="XmlElementAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple=true)]
    public class XmlElementAttribute : System.Attribute {
        string elementName;
        Type type;
        string ns;
        string dataType;
        bool nullable;
        bool nullableSpecified;
        XmlSchemaForm form = XmlSchemaForm.None;
        int order = -1;
        
        /// <include file='doc\XmlElementAttribute.uex' path='docs/doc[@for="XmlElementAttribute.XmlElementAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlElementAttribute() {
        }
        
        /// <include file='doc\XmlElementAttribute.uex' path='docs/doc[@for="XmlElementAttribute.XmlElementAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlElementAttribute(string elementName) {
            this.elementName = elementName;
        }

        /// <include file='doc\XmlElementAttribute.uex' path='docs/doc[@for="XmlElementAttribute.XmlElementAttribute2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlElementAttribute(Type type) {
            this.type = type;
        }
        
        /// <include file='doc\XmlElementAttribute.uex' path='docs/doc[@for="XmlElementAttribute.XmlElementAttribute3"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlElementAttribute(string elementName, Type type) {
            this.elementName = elementName;
            this.type = type;
        }

        /// <include file='doc\XmlElementAttribute.uex' path='docs/doc[@for="XmlElementAttribute.Type"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Type Type {
            get { return type; }
            set { type = value; }
        }

        /// <include file='doc\XmlElementAttribute.uex' path='docs/doc[@for="XmlElementAttribute.ElementName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ElementName {
            get { return elementName == null ? string.Empty : elementName; }
            set { elementName = value; }
        }
        
        /// <include file='doc\XmlElementAttribute.uex' path='docs/doc[@for="XmlElementAttribute.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return ns; }
            set { ns = value; }
        }

        /// <include file='doc\XmlElementAttribute.uex' path='docs/doc[@for="XmlElementAttribute.DataType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string DataType {
            get { return dataType == null ? string.Empty : dataType; }
            set { dataType = value; }
        }

        /// <include file='doc\XmlElementAttribute.uex' path='docs/doc[@for="XmlElementAttribute.IsNullable"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsNullable {
            get { return nullable; }
            set { 
                nullable = value; 
                nullableSpecified = true;
            }
        }

        internal bool IsNullableSpecified {
            get { return nullableSpecified; }
        }

        /// <include file='doc\XmlElementAttribute.uex' path='docs/doc[@for="XmlElementAttribute.Form"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSchemaForm Form {
            get { return form; }
            set { form = value; }
        }

        /// <include file='doc\XmlElementAttribute.uex' path='docs/doc[@for="XmlElementAttribute.Order"]/*' />
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
