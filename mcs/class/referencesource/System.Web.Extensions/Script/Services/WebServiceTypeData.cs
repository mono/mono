//------------------------------------------------------------------------------
// <copyright file="WebServiceTypeData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Services {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Schema;


    internal class WebServiceTypeData {
        // Actual type is needed for ResolveType in WebServiceData (not relevant in Indigo)
        private Type _actualType;
        // The custom string reprensenation used for WCF case
        private string _stringRepresentation;
        private string _typeName;
        private string _typeNamespace;
        static Dictionary<XmlQualifiedName, Type> _nameToType = new Dictionary<XmlQualifiedName, Type>();

        // constants carried over for internal System.Runtime.Seriliazation.Globals class.
        private const string SerializationNamespace = "http://schemas.microsoft.com/2003/10/Serialization/";
        private const string StringLocalName = "string";
        private const string SchemaNamespace = XmlSchema.Namespace;
        private const string ActualTypeLocalName = "ActualType";
        private const string ActualTypeNameAttribute = "Name";
        private const string ActualTypeNamespaceAttribute = "Namespace";
        private const string EnumerationValueLocalName = "EnumerationValue";
        private const string OccursUnbounded = "unbounded";

        static WebServiceTypeData() {
            Add(typeof(sbyte), "byte");
            Add(typeof(byte), "unsignedByte");
            Add(typeof(short), "short");
            Add(typeof(ushort), "unsignedShort");
            Add(typeof(int), "int");
            Add(typeof(uint), "unsignedInt");
            Add(typeof(long), "long");
            Add(typeof(ulong), "unsignedLong");
        }


        // type is null for WCF
        internal WebServiceTypeData(string name, string ns, Type type) {
            if (String.IsNullOrEmpty(ns)) {
                _typeName = name;
                if (type == null) { // for WCF known types
                    _stringRepresentation = name;
                }
            }
            else {
                _typeName = ns + "." + name;
                if (type == null) { // for WCF known types
                    _stringRepresentation = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", name, ns);
                }
            }
            _typeNamespace = ns;
            _actualType = type;
        }

        internal WebServiceTypeData(string name, string ns)
            : this(name, ns, null) {
        }

        private static XmlQualifiedName actualTypeAnnotationName;
        private static XmlQualifiedName ActualTypeAnnotationName {
            get {
                if (actualTypeAnnotationName == null)
                    actualTypeAnnotationName = new XmlQualifiedName(ActualTypeLocalName, SerializationNamespace);
                return actualTypeAnnotationName;
            }
        }

        static XmlQualifiedName enumerationValueAnnotationName;
        private static XmlQualifiedName EnumerationValueAnnotationName {
            get {
                if (enumerationValueAnnotationName == null)
                    enumerationValueAnnotationName = new XmlQualifiedName(EnumerationValueLocalName, SerializationNamespace);
                return enumerationValueAnnotationName;
            }
        }


        internal string StringRepresentation {
            get { return _stringRepresentation; }
        }

        internal string TypeName {
            get { return _typeName; }
        }

        internal string TypeNamespace {
            get { return _typeNamespace; }
        }

        internal Type Type {
            get { return _actualType; }
        }



        private static void Add(Type type, string localName) {
            XmlQualifiedName stableName = new XmlQualifiedName(localName, XmlSchema.Namespace);
            _nameToType.Add(stableName, type);
        }

        private static bool CheckIfCollection(XmlSchemaComplexType type) {
            if (type == null) {
                return false;
            }
            bool isCollection = false;
            if (type.ContentModel == null) {
                isCollection = CheckIfCollectionSequence(type.Particle as XmlSchemaSequence);
            }
            return isCollection;
        }

        private static bool CheckIfCollectionSequence(XmlSchemaSequence rootSequence) {
            // No support for versioning since schema is not persisted; unknown serialization elements are not removed
            if (rootSequence.Items == null || rootSequence.Items.Count == 0)
                return false;
            if (rootSequence.Items.Count != 1)
                return false;

            XmlSchemaObject o = rootSequence.Items[0];
            if (!(o is XmlSchemaElement))
                return false;

            XmlSchemaElement localElement = (XmlSchemaElement)o;
            return (localElement.MaxOccursString == OccursUnbounded || localElement.MaxOccurs > 1);
        }

        private static bool CheckIfEnum(XmlSchemaSimpleType simpleType, out XmlSchemaSimpleTypeRestriction simpleTypeRestriction) {
            simpleTypeRestriction = null;
            if (simpleType == null) {
                return false;
            }

            // check enum
            XmlSchemaSimpleTypeRestriction restriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
            if (restriction != null) {
                simpleTypeRestriction = restriction;
                return CheckIfEnumRestriction(restriction);
            }

            // check flags enum
            XmlSchemaSimpleTypeList list = simpleType.Content as XmlSchemaSimpleTypeList;
            XmlSchemaSimpleType anonymousType = list.ItemType;
            if (anonymousType != null) {
                restriction = anonymousType.Content as XmlSchemaSimpleTypeRestriction;
                if (restriction != null) {
                    simpleTypeRestriction = restriction;
                    return CheckIfEnumRestriction(restriction);
                }
            }

            return false;
        }

        static bool CheckIfEnumRestriction(XmlSchemaSimpleTypeRestriction restriction) {
            foreach (XmlSchemaFacet facet in restriction.Facets) {
                if (!(facet is XmlSchemaEnumerationFacet)) {
                    return false;
                }
            }

            // Does not check for non-string name or anonymous base type 
            // because these are exported types and do not use those constructs.
            if (restriction.BaseTypeName != XmlQualifiedName.Empty) {
                return (restriction.BaseTypeName.Name == StringLocalName
                    && restriction.BaseTypeName.Namespace == SchemaNamespace
                    && restriction.Facets.Count > 0);
            }

            return false;
        }

        private static string GetInnerText(XmlQualifiedName typeName, XmlElement xmlElement) {
            if (xmlElement != null) {
                XmlNode child = xmlElement.FirstChild;
                while (child != null) {
                    if (child.NodeType == XmlNodeType.Element) {
                        throw new InvalidOperationException();
                        //
                    }
                    child = child.NextSibling;
                }
                return xmlElement.InnerText;
            }
            return null;
        }

        internal static List<WebServiceTypeData> GetKnownTypes(Type type, WebServiceTypeData typeData) {
            List<WebServiceTypeData> knownTypes = new List<WebServiceTypeData>();
            XsdDataContractExporter exporter = new XsdDataContractExporter();
            exporter.Export(type);
            ICollection schemas = exporter.Schemas.Schemas();
            foreach (XmlSchema schema in schemas) {
                // DataContractSerializer always exports built-in types into a fixed schema that can be ignored.
                if (schema.TargetNamespace == SerializationNamespace) {
                    continue;
                }

                foreach (XmlSchemaObject schemaObj in schema.Items) {
                    XmlSchemaType schemaType = schemaObj as XmlSchemaType;
                    string schemaTargetNamespace = XmlConvert.DecodeName(schema.TargetNamespace);
                    if (schemaType != null
                        && !(schemaType.Name == typeData.TypeName && schemaTargetNamespace == typeData.TypeNamespace)
                        && !String.IsNullOrEmpty(schemaType.Name)) {
                        WebServiceTypeData knownTypeData = null;
                        XmlSchemaSimpleTypeRestriction simpleTypeRestriction;
                        if (CheckIfEnum(schemaType as XmlSchemaSimpleType, out simpleTypeRestriction)) {
                            knownTypeData = ImportEnum(XmlConvert.DecodeName(schemaType.Name), schemaTargetNamespace, schemaType.QualifiedName, simpleTypeRestriction, schemaType.Annotation);
                        }
                        else if (CheckIfCollection(schemaType as XmlSchemaComplexType)) {
                            continue;
                        }
                        else if (!(schemaType is XmlSchemaSimpleType)) {
                            knownTypeData = new WebServiceTypeData(XmlConvert.DecodeName(schemaType.Name), schemaTargetNamespace);
                        }
                        if (knownTypeData != null) {
                            knownTypes.Add(knownTypeData);
                        }
                    }
                }
            }
            return knownTypes;
        }

        //used for WCF known types
        internal static WebServiceTypeData GetWebServiceTypeData(Type type) {

            WebServiceTypeData typeData = null;       
            XsdDataContractExporter exporter = new XsdDataContractExporter();
            XmlQualifiedName qname = exporter.GetSchemaTypeName(type);
            if (!qname.IsEmpty) {
                if (type.IsEnum) {
                    bool isUlong = (Enum.GetUnderlyingType(type) == typeof(ulong));
                    typeData = new WebServiceEnumData(XmlConvert.DecodeName(qname.Name), XmlConvert.DecodeName(qname.Namespace), Enum.GetNames(type), Enum.GetValues(type), isUlong);
                }
                else {
                    typeData = new WebServiceTypeData(XmlConvert.DecodeName(qname.Name), XmlConvert.DecodeName(qname.Namespace));
                }
            }
            return typeData;
        }

        internal static XmlQualifiedName ImportActualType(XmlSchemaAnnotation annotation, XmlQualifiedName defaultTypeName, XmlQualifiedName typeName) {
            XmlElement actualTypeElement = ImportAnnotation(annotation, ActualTypeAnnotationName);
            if (actualTypeElement == null) {
                return defaultTypeName;
            }

            XmlNode nameAttribute = actualTypeElement.Attributes.GetNamedItem(ActualTypeNameAttribute);
            Debug.Assert(nameAttribute != null);
            Debug.Assert(nameAttribute.Value != null);
            string name = nameAttribute.Value;

            XmlNode nsAttribute = actualTypeElement.Attributes.GetNamedItem(ActualTypeNamespaceAttribute);
            Debug.Assert(nsAttribute != null);
            Debug.Assert(nsAttribute.Value != null);
            string ns = nsAttribute.Value;

            return new XmlQualifiedName(name, ns);
        }


        static XmlElement ImportAnnotation(XmlSchemaAnnotation annotation, XmlQualifiedName annotationQualifiedName) {
            if (annotation != null && annotation.Items != null && annotation.Items.Count > 0 && annotation.Items[0] is XmlSchemaAppInfo) {
                XmlSchemaAppInfo appInfo = (XmlSchemaAppInfo)annotation.Items[0];
                XmlNode[] markup = appInfo.Markup;
                if (markup != null) {
                    for (int i = 0; i < markup.Length; i++) {
                        XmlElement annotationElement = markup[i] as XmlElement;
                        if (annotationElement != null && annotationElement.LocalName == annotationQualifiedName.Name && annotationElement.NamespaceURI == annotationQualifiedName.Namespace) {
                            return annotationElement;
                        }
                    }
                }
            }
            return null;
        }


        static WebServiceEnumData ImportEnum(string typeName, string typeNamespace, XmlQualifiedName typeQualifiedName, XmlSchemaSimpleTypeRestriction restriction, XmlSchemaAnnotation annotation) {
            // CheckIfEnum has already checked if baseType of restriction is string 
            XmlQualifiedName baseTypeName = ImportActualType(annotation, new XmlQualifiedName("int", XmlSchema.Namespace), typeQualifiedName);
            Type baseEnumType = _nameToType[baseTypeName];
            bool isULong = (baseEnumType == typeof(ulong));
            List<string> enumNames = new List<string>();
            List<long> enumValues = new List<long>();
            foreach (XmlSchemaFacet facet in restriction.Facets) {
                XmlSchemaEnumerationFacet enumFacet = facet as XmlSchemaEnumerationFacet;
                Debug.Assert(enumFacet != null);
                Debug.Assert(enumFacet.Value != null);

                string valueInnerText = GetInnerText(typeQualifiedName, ImportAnnotation(enumFacet.Annotation, EnumerationValueAnnotationName));
                long value;
                if (valueInnerText == null) {
                    // ASP .NET AJAX doesn't honor the Flags nature of Flags enums
                    // If it were to, we would assign Math.Pow(2, nameValues.Count) for Flags enums instead.
                    value = enumNames.Count;
                }
                else {
                    if (isULong) {
                        value = (long)ulong.Parse(valueInnerText, NumberFormatInfo.InvariantInfo);
                    }
                    else {
                        value = long.Parse(valueInnerText, NumberFormatInfo.InvariantInfo);
                    }
                }

                enumNames.Add(enumFacet.Value);
                enumValues.Add(value);
            }

            return new WebServiceEnumData(typeName, typeNamespace, enumNames.ToArray(), enumValues.ToArray(), isULong);
        }

    }
}
