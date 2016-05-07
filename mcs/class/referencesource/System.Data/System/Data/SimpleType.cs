//------------------------------------------------------------------------------
// <copyright file="SimpleType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Xml;
    using System.Xml.Schema;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Globalization;
    using System.Collections;
    using System.Data.Common;

    /// <devdoc>
    /// </devdoc>
    [Serializable]
    internal  sealed class SimpleType : ISerializable {
        string baseType = null;                 // base type name
        SimpleType baseSimpleType = null;
//        object  xmlBaseType = null;           // Qualified name of Basetype
        XmlQualifiedName xmlBaseType = null;    // Qualified name of Basetype
        string name = "";
        int length = -1;
        int minLength = -1;
        int maxLength = -1;
        string pattern = "";
        string ns = "";                  // my ns
        
        // 
        string maxExclusive = "";
        string maxInclusive = "";
        string minExclusive = "";
        string minInclusive = "";
        //REMOVED: encoding due to [....] 2001 XDS changes

        // 
        internal string enumeration = "";

        internal SimpleType (string baseType) { // anonymous simpletype
            this.baseType = baseType;
        }

        internal  SimpleType (XmlSchemaSimpleType node) { // named simpletype
            name = node.Name;
            ns = (node.QualifiedName != null) ? node.QualifiedName.Namespace : "";
            LoadTypeValues(node);
        }

        private SimpleType(SerializationInfo info, StreamingContext context) {
            this.baseType = info.GetString("SimpleType.BaseType");
            this.baseSimpleType = (SimpleType)info.GetValue("SimpleType.BaseSimpleType", typeof(SimpleType));

            if (info.GetBoolean("SimpleType.XmlBaseType.XmlQualifiedNameExists")) {
                string xmlQNName = info.GetString("SimpleType.XmlBaseType.Name");
                string xmlQNNamespace = info.GetString("SimpleType.XmlBaseType.Namespace");
                this.xmlBaseType = new XmlQualifiedName(xmlQNName, xmlQNNamespace);            
            }
            else {
                this.xmlBaseType = null;
            }
            this.name = info.GetString("SimpleType.Name");
            this.ns = info.GetString("SimpleType.NS");
            this.maxLength = info.GetInt32("SimpleType.MaxLength");
            this.length = info.GetInt32("SimpleType.Length");
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("SimpleType.BaseType", this.baseType);
            info.AddValue("SimpleType.BaseSimpleType", this.baseSimpleType);
            XmlQualifiedName xmlQN = (xmlBaseType as XmlQualifiedName);
            info.AddValue("SimpleType.XmlBaseType.XmlQualifiedNameExists", xmlQN != null ? true : false);
            info.AddValue("SimpleType.XmlBaseType.Name", xmlQN != null ? xmlQN.Name : null);
            info.AddValue("SimpleType.XmlBaseType.Namespace", xmlQN != null ? xmlQN.Namespace : null);
            info.AddValue("SimpleType.Name", this.name);
            info.AddValue("SimpleType.NS", this.ns);
            info.AddValue("SimpleType.MaxLength", this.maxLength);
            info.AddValue("SimpleType.Length", this.length);
        }        

        internal void LoadTypeValues (XmlSchemaSimpleType node) {
            if ((node.Content is XmlSchemaSimpleTypeList) || 
                (node.Content is XmlSchemaSimpleTypeUnion))
                throw ExceptionBuilder.SimpleTypeNotSupported();

            if (node.Content is XmlSchemaSimpleTypeRestriction) {
                XmlSchemaSimpleTypeRestriction content = (XmlSchemaSimpleTypeRestriction) node.Content;

                XmlSchemaSimpleType ancestor = node.BaseXmlSchemaType as XmlSchemaSimpleType;
                if ((ancestor != null) && (ancestor.QualifiedName.Namespace != Keywords.XSDNS)) { // I'm assuming that built-in types don't have a name!
//                    Console.WriteLine("In simpleNode, ancestor.Name = '{0}'", ancestor.Name);
                    baseSimpleType = new SimpleType(node.BaseXmlSchemaType as XmlSchemaSimpleType);
//                    baseSimpleType = new SimpleType(node);
                } 

// do we need to put qualified name?                
// for user defined simpletype, always go with qname
                if (content.BaseTypeName.Namespace == Keywords.XSDNS)
                    baseType = content.BaseTypeName.Name;
                else
                    baseType = content.BaseTypeName.ToString();


                if (baseSimpleType != null && baseSimpleType.Name != null && baseSimpleType.Name.Length > 0) {
                    xmlBaseType = baseSimpleType.XmlBaseType;//  SimpleTypeQualifiedName;
                }
                else {
                    xmlBaseType = content.BaseTypeName;
                }

                if (baseType == null || baseType.Length == 0) {
//                    Console.WriteLine("baseType == null, setting it to ", content.BaseType.Name);
                    baseType = content.BaseType.Name;
                    xmlBaseType = null;
                }

                if (baseType == "NOTATION")
                    baseType = "string";
               

                foreach(XmlSchemaFacet facet in content.Facets) {

                    if (facet is XmlSchemaLengthFacet)
                        length = Convert.ToInt32(facet.Value, null);
                        
                    if (facet is XmlSchemaMinLengthFacet)
                        minLength = Convert.ToInt32(facet.Value, null);
                        
                    if (facet is XmlSchemaMaxLengthFacet)
                        maxLength = Convert.ToInt32(facet.Value, null);
                        
                    if (facet is XmlSchemaPatternFacet)
                        pattern = facet.Value;
                        
                    if (facet is XmlSchemaEnumerationFacet)
                        enumeration = !Common.ADP.IsEmpty(enumeration) ? enumeration + " " + facet.Value : facet.Value;
                        
                    if (facet is XmlSchemaMinExclusiveFacet)
                        minExclusive = facet.Value;
                        
                    if (facet is XmlSchemaMinInclusiveFacet)
                        minInclusive = facet.Value;
                       
                    if (facet is XmlSchemaMaxExclusiveFacet)
                        maxExclusive = facet.Value;
                        
                    if (facet is XmlSchemaMaxInclusiveFacet)
                        maxInclusive = facet.Value;

                    }
                }

                string tempStr = XSDSchema.GetMsdataAttribute(node, Keywords.TARGETNAMESPACE);
                if (tempStr != null)
                    ns = tempStr;
            }

        internal bool IsPlainString() {
            return (
                XSDSchema.QualifiedName(this.baseType)    == XSDSchema.QualifiedName("string")     &&
                Common.ADP.IsEmpty(this.name)         &&
                this.length       == -1               &&
                this.minLength    == -1               &&
                this.maxLength    == -1               &&
                Common.ADP.IsEmpty(this.pattern)      &&
                Common.ADP.IsEmpty(this.maxExclusive) &&
                Common.ADP.IsEmpty(this.maxInclusive) &&
                Common.ADP.IsEmpty(this.minExclusive) &&
                Common.ADP.IsEmpty(this.minInclusive) &&
                Common.ADP.IsEmpty(this.enumeration)
            );
        }

        internal string BaseType {
            get {
                return baseType;
            }
        }

        internal XmlQualifiedName XmlBaseType {
            get {
                return (XmlQualifiedName)xmlBaseType;
            }
        }

        internal string Name {
            get {
                    return name;
            }
        }

        internal string Namespace {
            get {
                return ns;
            }
        }

        internal int Length {
            get {
                return length;
            }
        }

        internal int MaxLength {
            get {
                return maxLength;
            }
            set {
                maxLength = value;
            }
        }

        internal SimpleType BaseSimpleType {
            get {
                return baseSimpleType;
            }
        }
// return  qualified name of this simple type
        public string SimpleTypeQualifiedName {
          get {
            if (ns.Length == 0)
                return name;
            return (ns + ":" + name);
          }
        }

        internal string QualifiedName(string name) {
            int iStart = name.IndexOf(':');
            if (iStart == -1)
                return Keywords.XSD_PREFIXCOLON + name;
            else
                return name;
        }

/*
        internal XmlNode ToNode(XmlDocument dc) {
            return ToNode(dc, null, false);
        }
*/
        
        internal XmlNode ToNode(XmlDocument dc, Hashtable prefixes, bool inRemoting) {
            XmlElement typeNode = dc.CreateElement(Keywords.XSD_PREFIX, Keywords.XSD_SIMPLETYPE, Keywords.XSDNS);

            if (name != null && name.Length != 0) {
                // this is a global type : 
                typeNode.SetAttribute(Keywords.NAME, name);
                if (inRemoting) {
                    typeNode.SetAttribute(Keywords.TARGETNAMESPACE, Keywords.MSDNS, this.Namespace);
                }
            }
            XmlElement type = dc.CreateElement(Keywords.XSD_PREFIX, Keywords.XSD_RESTRICTION, Keywords.XSDNS);

            if (!inRemoting) {
                if (baseSimpleType != null) {
                    if (baseSimpleType.Namespace != null && baseSimpleType.Namespace.Length > 0) {
                        string prefix = (prefixes!=null)?(string) prefixes[baseSimpleType.Namespace]:null;
                        if (prefix != null) {
                            type.SetAttribute(Keywords.BASE, (prefix +":"+ baseSimpleType.Name));
                        }
                        else {
                            type.SetAttribute(Keywords.BASE, baseSimpleType.Name);
                        }
                    }
                    else { // [....]
                        type.SetAttribute(Keywords.BASE, baseSimpleType.Name);
                    }
                }
                else {
                    type.SetAttribute(Keywords.BASE, QualifiedName(baseType)); // has to be xs:SomePrimitiveType
                }
            }
            else{
                type.SetAttribute(Keywords.BASE, (baseSimpleType != null) ? baseSimpleType.Name : QualifiedName(baseType));
            }

            XmlElement constraint;
            if (length >= 0) {
                constraint = dc.CreateElement(Keywords.XSD_PREFIX, Keywords.XSD_LENGTH, Keywords.XSDNS);
                constraint.SetAttribute(Keywords.VALUE, length.ToString(CultureInfo.InvariantCulture));
                type.AppendChild(constraint);
            }
            if (maxLength >= 0) {
                constraint = dc.CreateElement(Keywords.XSD_PREFIX, Keywords.XSD_MAXLENGTH, Keywords.XSDNS);
                constraint.SetAttribute(Keywords.VALUE, maxLength.ToString(CultureInfo.InvariantCulture));
                type.AppendChild(constraint);
            }
/*        // removed due to MDAC bug 83892
            // will be reactivated in whidbey with the proper handling
            if (pattern != null && pattern.Length > 0) {
                constraint = dc.CreateElement(Keywords.XSD_PREFIX, Keywords.XSD_PATTERN, Keywords.XSDNS);
                constraint.SetAttribute(Keywords.VALUE, pattern);
                type.AppendChild(constraint);
            }
            if (minLength >= 0) {
                constraint = dc.CreateElement(Keywords.XSD_PREFIX, Keywords.XSD_MINLENGTH, Keywords.XSDNS);
                constraint.SetAttribute(Keywords.VALUE, minLength.ToString());
                type.AppendChild(constraint);
            }
            if (minInclusive != null && minInclusive.Length > 0) {
                constraint = dc.CreateElement(Keywords.XSD_PREFIX, Keywords.XSD_MININCLUSIVE, Keywords.XSDNS);
                constraint.SetAttribute(Keywords.VALUE, minInclusive);
                type.AppendChild(constraint);
            }
            if (minExclusive != null && minExclusive.Length > 0) {
                constraint =dc.CreateElement(Keywords.XSD_PREFIX, Keywords.XSD_MINEXCLUSIVE, Keywords.XSDNS);
                constraint.SetAttribute(Keywords.VALUE, minExclusive);
                type.AppendChild(constraint);
            }
            if (maxInclusive != null && maxInclusive.Length > 0) {
                constraint =dc.CreateElement(Keywords.XSD_PREFIX, Keywords.XSD_MAXINCLUSIVE, Keywords.XSDNS);
                constraint.SetAttribute(Keywords.VALUE, maxInclusive);
                type.AppendChild(constraint);
            }
            if (maxExclusive != null && maxExclusive.Length > 0) {
                constraint = dc.CreateElement(Keywords.XSD_PREFIX, Keywords.XSD_MAXEXCLUSIVE, Keywords.XSDNS);
                constraint.SetAttribute(Keywords.VALUE, maxExclusive);
                type.AppendChild(constraint);
            }
            if (enumeration.Length > 0) {
                string[] list = enumeration.TrimEnd(null).Split(null);

                for (int i = 0; i < list.Length; i++) {
                    constraint = dc.CreateElement(Keywords.XSD_PREFIX, Keywords.XSD_ENUMERATION, Keywords.XSDNS);
                    constraint.SetAttribute(Keywords.VALUE, list[i]);
                    type.AppendChild(constraint);
                }
            }
            */
            typeNode.AppendChild(type);
            return typeNode;
        }


        // 
        internal static SimpleType CreateEnumeratedType(string values) {
            SimpleType enumType = new SimpleType("string");
            enumType.enumeration = values;
            return enumType;
        }

        internal static SimpleType CreateByteArrayType(string encoding) {
            SimpleType byteArrayType = new SimpleType("base64Binary");
            return byteArrayType;
        }
        
        internal static SimpleType CreateLimitedStringType(int length) {
            SimpleType limitedString = new SimpleType("string");
            limitedString.maxLength = length;
            return limitedString;
        }

        internal static SimpleType CreateSimpleType(StorageType typeCode, Type type) {
            if ((typeCode == StorageType.Char) && (type == typeof(Char))) {
                return new SimpleType("string") { length = 1 };
            }
            return null;
        }

// Assumption is otherSimpleType and current ST name and NS matches.
// if existing simpletype is being redefined with different facets, then it will return no-empty string defining the error
        internal string HasConflictingDefinition(SimpleType otherSimpleType) {
            if (otherSimpleType == null)
                return "otherSimpleType";
            if (this.MaxLength != otherSimpleType.MaxLength)
                return ("MaxLength");

            if (string.Compare(this.BaseType, otherSimpleType.BaseType, StringComparison.Ordinal) != 0)
                return ("BaseType");
            if ((this.BaseSimpleType == null && otherSimpleType.BaseSimpleType != null) &&
                (this.BaseSimpleType.HasConflictingDefinition(otherSimpleType.BaseSimpleType)).Length != 0)
                return ("BaseSimpleType");
             return string.Empty;
                
        }
// only string types can have MaxLength
        internal bool CanHaveMaxLength() {
            SimpleType rootType = this;
            while (rootType.BaseSimpleType != null) {
                rootType = rootType.BaseSimpleType;
            }
            if (string.Compare(rootType.BaseType, "string", StringComparison.OrdinalIgnoreCase) == 0)
                return true;
            return false;
        }
       internal void ConvertToAnnonymousSimpleType() {
           this.name = null;
           this.ns = string.Empty;
           SimpleType tmpSimpleType = this;

           while (tmpSimpleType.baseSimpleType !=  null) {
               tmpSimpleType = tmpSimpleType.baseSimpleType;
           }
           baseType = tmpSimpleType.baseType;
           baseSimpleType = tmpSimpleType.baseSimpleType;
           xmlBaseType = tmpSimpleType.xmlBaseType;          
       }
    }
}
