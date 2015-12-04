//------------------------------------------------------------------------------
// <copyright file="XmlSchemaSimpleTypeUnion.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Xml.Serialization;

    /// <include file='doc\XmlSchemaSimpleTypeUnion.uex' path='docs/doc[@for="XmlSchemaSimpleTypeUnion"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class XmlSchemaSimpleTypeUnion : XmlSchemaSimpleTypeContent {
        XmlSchemaObjectCollection baseTypes = new XmlSchemaObjectCollection();
        XmlQualifiedName[] memberTypes;
        XmlSchemaSimpleType[] baseMemberTypes; // Compiled

        /// <include file='doc\XmlSchemaSimpleTypeUnion.uex' path='docs/doc[@for="XmlSchemaSimpleTypeUnion.BaseTypes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
        public XmlSchemaObjectCollection BaseTypes {
            get { return baseTypes; }
        }

        /// <include file='doc\XmlSchemaSimpleTypeUnion.uex' path='docs/doc[@for="XmlSchemaSimpleTypeUnion.MemberTypes"]/*' />
        [XmlAttribute("memberTypes")]
        public XmlQualifiedName[] MemberTypes {
            get { return memberTypes; }
            set { memberTypes = value; }
        }
        
        //Compiled Information
        /// <include file='doc\XmlSchemaSimpleTypeUnion.uex' path='docs/doc[@for="XmlSchemaSimpleTypeUnion.BaseMemberTypes"]/*' />
        [XmlIgnore]
        public XmlSchemaSimpleType[] BaseMemberTypes {
            get { return baseMemberTypes; }
        }

        internal void SetBaseMemberTypes(XmlSchemaSimpleType[] baseMemberTypes) {
            this.baseMemberTypes = baseMemberTypes;
        }

        internal override XmlSchemaObject Clone() {
            if (memberTypes != null && memberTypes.Length > 0) { //Only if the union has MemberTypes defined
                XmlSchemaSimpleTypeUnion newUnion = (XmlSchemaSimpleTypeUnion)MemberwiseClone();
                XmlQualifiedName[] newQNames = new XmlQualifiedName[memberTypes.Length];
            
                for (int i = 0; i < memberTypes.Length; i++) {
                    newQNames[i] = memberTypes[i].Clone();
                }
                newUnion.MemberTypes = newQNames;
                return newUnion;
            }
            return this;
        }
    }
}

