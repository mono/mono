//------------------------------------------------------------------------------
// <copyright file="SoapAttributes.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    using System;
    using System.Reflection;
    using System.Collections;
    using System.ComponentModel;

    internal enum SoapAttributeFlags {
        Enum = 0x1,
        Type = 0x2,
        Element = 0x4,
        Attribute = 0x8,
    }

    /// <include file='doc\SoapAttributes.uex' path='docs/doc[@for="SoapAttributes"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class SoapAttributes {
        bool soapIgnore;
        SoapTypeAttribute soapType;
        SoapElementAttribute soapElement;
        SoapAttributeAttribute soapAttribute;
        SoapEnumAttribute soapEnum;
        object soapDefaultValue = null;
        
        /// <include file='doc\SoapAttributes.uex' path='docs/doc[@for="SoapAttributes.SoapAttributes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapAttributes() {
        }

        /// <include file='doc\SoapAttributes.uex' path='docs/doc[@for="SoapAttributes.SoapAttributes1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapAttributes(ICustomAttributeProvider provider) {
            object[] attrs = provider.GetCustomAttributes(false);
            for (int i = 0; i < attrs.Length; i++) {
                if (attrs[i] is SoapIgnoreAttribute || attrs[i] is ObsoleteAttribute) {
                    this.soapIgnore = true;
                    break;
                }
                else if (attrs[i] is SoapElementAttribute) {
                    this.soapElement = (SoapElementAttribute)attrs[i];
                }
                else if (attrs[i] is SoapAttributeAttribute) {
                    this.soapAttribute = (SoapAttributeAttribute)attrs[i];
                }
                else if (attrs[i] is SoapTypeAttribute) {
                    this.soapType = (SoapTypeAttribute)attrs[i];
                }
                else if (attrs[i] is SoapEnumAttribute) {
                    this.soapEnum = (SoapEnumAttribute)attrs[i];
                }
                else if (attrs[i] is DefaultValueAttribute) {
                    this.soapDefaultValue = ((DefaultValueAttribute)attrs[i]).Value;
                }
            }
            if (soapIgnore) {
                this.soapElement = null;
                this.soapAttribute = null;
                this.soapType = null;
                this.soapEnum = null;
                this.soapDefaultValue = null;
            }
        }
        
        internal SoapAttributeFlags SoapFlags {
            get { 
                SoapAttributeFlags flags = 0;
                if (soapElement != null) flags |= SoapAttributeFlags.Element;
                if (soapAttribute != null) flags |= SoapAttributeFlags.Attribute;
                if (soapEnum != null) flags |= SoapAttributeFlags.Enum;
                if (soapType != null) flags |= SoapAttributeFlags.Type;
                return flags;
            }
        }

        /// <include file='doc\SoapAttributes.uex' path='docs/doc[@for="SoapAttributes.SoapType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapTypeAttribute SoapType {
            get { return soapType; }
            set { soapType = value; }
        }
        
        /// <include file='doc\SoapAttributes.uex' path='docs/doc[@for="SoapAttributes.SoapEnum"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapEnumAttribute SoapEnum {
            get { return soapEnum; }
            set { soapEnum = value; }
        }
        
        /// <include file='doc\SoapAttributes.uex' path='docs/doc[@for="SoapAttributes.SoapIgnore"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool SoapIgnore {
            get { return soapIgnore; }
            set { soapIgnore = value; }
        }
        
        /// <include file='doc\SoapAttributes.uex' path='docs/doc[@for="SoapAttributes.SoapElement"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapElementAttribute SoapElement {
            get { return soapElement; }
            set { soapElement = value; }
        }
        
        /// <include file='doc\SoapAttributes.uex' path='docs/doc[@for="SoapAttributes.SoapAttribute"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapAttributeAttribute SoapAttribute {
            get { return soapAttribute; }
            set { soapAttribute = value; }
        }

        /// <include file='doc\SoapAttributes.uex' path='docs/doc[@for="SoapAttributes.SoapDefaultValue"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object SoapDefaultValue {
            get { return soapDefaultValue; }
            set { soapDefaultValue = value; }
        }
    }
}
