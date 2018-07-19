//------------------------------------------------------------------------------
// <copyright file="XmlArrayItemAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System;
    using System.Xml.Schema;

    /// <include file='doc\XmlArrayItemAttribute.uex' path='docs/doc[@for="XmlArrayItemAttribute"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple=true)]
    public class XmlArrayItemAttribute : System.Attribute {
        string elementName;
        Type type;
        string ns;
        string dataType;
        bool nullable;
        bool nullableSpecified = false;
        XmlSchemaForm form = XmlSchemaForm.None;
        int nestingLevel;
        
        /// <include file='doc\XmlArrayItemAttribute.uex' path='docs/doc[@for="XmlArrayItemAttribute.XmlArrayItemAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlArrayItemAttribute() {
        }
        
        /// <include file='doc\XmlArrayItemAttribute.uex' path='docs/doc[@for="XmlArrayItemAttribute.XmlArrayItemAttribute1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlArrayItemAttribute(string elementName) {
            this.elementName = elementName;
        }
        
        /// <include file='doc\XmlArrayItemAttribute.uex' path='docs/doc[@for="XmlArrayItemAttribute.XmlArrayItemAttribute2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlArrayItemAttribute(Type type) {
            this.type = type;
        }
        
        /// <include file='doc\XmlArrayItemAttribute.uex' path='docs/doc[@for="XmlArrayItemAttribute.XmlArrayItemAttribute3"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlArrayItemAttribute(string elementName, Type type) {
            this.elementName = elementName;
            this.type = type;
        }
        
        /// <include file='doc\XmlArrayItemAttribute.uex' path='docs/doc[@for="XmlArrayItemAttribute.Type"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Type Type {
            get { return type; }
            set { type = value; }
        }
        
        /// <include file='doc\XmlArrayItemAttribute.uex' path='docs/doc[@for="XmlArrayItemAttribute.ElementName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ElementName {
            get { return elementName == null ? string.Empty : elementName; }
            set { elementName = value; }
        }

        /// <include file='doc\XmlArrayItemAttribute.uex' path='docs/doc[@for="XmlArrayItemAttribute.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Namespace {
            get { return ns; }
            set { ns = value; }
        }

        /// <include file='doc\XmlArrayItemAttribute.uex' path='docs/doc[@for="XmlArrayItemAttribute.NestingLevel"]/*' />
        public int NestingLevel {
            get { return nestingLevel; }
            set { nestingLevel = value; }
        }

        /// <include file='doc\XmlArrayItemAttribute.uex' path='docs/doc[@for="XmlArrayItemAttribute.DataType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string DataType {
            get { return dataType == null ? string.Empty : dataType; }
            set { dataType = value; }
        }

        /// <include file='doc\XmlArrayItemAttribute.uex' path='docs/doc[@for="XmlArrayItemAttribute.IsNullable"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsNullable {
            get { return nullable; }
            set { nullable = value; nullableSpecified = true; }
        }

        internal bool IsNullableSpecified {
            get { return nullableSpecified; }
        }

        /// <include file='doc\XmlArrayItemAttribute.uex' path='docs/doc[@for="XmlArrayItemAttribute.Form"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlSchemaForm Form {
            get { return form; }
            set { form = value; }
        }
    }
 }
