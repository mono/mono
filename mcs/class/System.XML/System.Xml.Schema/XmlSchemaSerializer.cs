using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace System.Xml.Schema
{

	internal class XmlSchemaSerializationWriter : XmlSerializationWriter
	{
		const string xmlNamespace = "http://www.w3.org/2000/xmlns/";
		public void WriteRoot_XmlSchema (object o)
		{
			WriteStartDocument ();
			System.Xml.Schema.XmlSchema ob = (System.Xml.Schema.XmlSchema) o;
			TopLevelElement ();
			WriteObject_XmlSchema (ob, "schema", "http://www.w3.org/2001/XMLSchema", true, false, true);
		}

		void WriteObject_XmlSchema (System.Xml.Schema.XmlSchema ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchema))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchema", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o0 = ob.@UnhandledAttributes;
			if (o0 != null) {
				foreach (XmlAttribute o1 in o0)
					if (o1.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o1, ob);
			}

			if (ob.@AttributeFormDefault != ((System.Xml.Schema.XmlSchemaForm) System.Xml.Schema.XmlSchemaForm.None)) {
				WriteAttribute ("attributeFormDefault", "", GetEnumValue_XmlSchemaForm (ob.@AttributeFormDefault));
			}
			if (ob.@BlockDefault != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("blockDefault", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@BlockDefault));
			}
			if (ob.@FinalDefault != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("finalDefault", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@FinalDefault));
			}
			if (ob.@ElementFormDefault != ((System.Xml.Schema.XmlSchemaForm) System.Xml.Schema.XmlSchemaForm.None)) {
				WriteAttribute ("elementFormDefault", "", GetEnumValue_XmlSchemaForm (ob.@ElementFormDefault));
			}
			WriteAttribute ("targetNamespace", "", ob.@TargetNamespace);
			WriteAttribute ("version", "", ob.@Version);
			WriteAttribute ("id", "", ob.@Id);

			if (ob.@Includes != null) {
				for (int n2 = 0; n2 < ob.@Includes.Count; n2++) {
					if (ob.@Includes[n2] == null) { }
					else if (ob.@Includes[n2].GetType() == typeof(System.Xml.Schema.XmlSchemaInclude)) {
						WriteObject_XmlSchemaInclude (((System.Xml.Schema.XmlSchemaInclude) ob.@Includes[n2]), "include", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Includes[n2].GetType() == typeof(System.Xml.Schema.XmlSchemaImport)) {
						WriteObject_XmlSchemaImport (((System.Xml.Schema.XmlSchemaImport) ob.@Includes[n2]), "import", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Includes[n2].GetType() == typeof(System.Xml.Schema.XmlSchemaRedefine)) {
						WriteObject_XmlSchemaRedefine (((System.Xml.Schema.XmlSchemaRedefine) ob.@Includes[n2]), "redefine", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Includes[n2]);
				}
			}
			if (ob.@Items != null) {
				for (int n3 = 0; n3 < ob.@Items.Count; n3++) {
					if (ob.@Items[n3] == null) { }
					else if (ob.@Items[n3].GetType() == typeof(System.Xml.Schema.XmlSchemaElement)) {
						WriteObject_XmlSchemaElement (((System.Xml.Schema.XmlSchemaElement) ob.@Items[n3]), "element", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n3].GetType() == typeof(System.Xml.Schema.XmlSchemaSimpleType)) {
						WriteObject_XmlSchemaSimpleType (((System.Xml.Schema.XmlSchemaSimpleType) ob.@Items[n3]), "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n3].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Items[n3]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n3].GetType() == typeof(System.Xml.Schema.XmlSchemaAnnotation)) {
						WriteObject_XmlSchemaAnnotation (((System.Xml.Schema.XmlSchemaAnnotation) ob.@Items[n3]), "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n3].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroup)) {
						WriteObject_XmlSchemaAttributeGroup (((System.Xml.Schema.XmlSchemaAttributeGroup) ob.@Items[n3]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n3].GetType() == typeof(System.Xml.Schema.XmlSchemaGroup)) {
						WriteObject_XmlSchemaGroup (((System.Xml.Schema.XmlSchemaGroup) ob.@Items[n3]), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n3].GetType() == typeof(System.Xml.Schema.XmlSchemaComplexType)) {
						WriteObject_XmlSchemaComplexType (((System.Xml.Schema.XmlSchemaComplexType) ob.@Items[n3]), "complexType", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n3].GetType() == typeof(System.Xml.Schema.XmlSchemaNotation)) {
						WriteObject_XmlSchemaNotation (((System.Xml.Schema.XmlSchemaNotation) ob.@Items[n3]), "notation", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Items[n3]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}
/*
		void WriteObject_XmlSchemaForm (System.Xml.Schema.XmlSchemaForm ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaForm))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaForm", "");

			Writer.WriteString (GetEnumValue_XmlSchemaForm (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}
*/
		string GetEnumValue_XmlSchemaForm (System.Xml.Schema.XmlSchemaForm val)
		{
			switch (val)
			{
				case System.Xml.Schema.XmlSchemaForm.Qualified: return "qualified";
				case System.Xml.Schema.XmlSchemaForm.Unqualified: return "unqualified";
				default: return ((long)val).ToString(CultureInfo.InvariantCulture);
			}
		}
/*
		void WriteObject_XmlSchemaDerivationMethod (System.Xml.Schema.XmlSchemaDerivationMethod ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaDerivationMethod))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaDerivationMethod", "");

			Writer.WriteString (GetEnumValue_XmlSchemaDerivationMethod (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}
*/
		string GetEnumValue_XmlSchemaDerivationMethod (System.Xml.Schema.XmlSchemaDerivationMethod val)
		{
			switch (val)
			{
				case System.Xml.Schema.XmlSchemaDerivationMethod.Empty: return "";
				case System.Xml.Schema.XmlSchemaDerivationMethod.Substitution: return "substitution";
				case System.Xml.Schema.XmlSchemaDerivationMethod.Extension: return "extension";
				case System.Xml.Schema.XmlSchemaDerivationMethod.Restriction: return "restriction";
				case System.Xml.Schema.XmlSchemaDerivationMethod.List: return "list";
				case System.Xml.Schema.XmlSchemaDerivationMethod.Union: return "union";
				case System.Xml.Schema.XmlSchemaDerivationMethod.All: return "#all";
				default:
					System.Text.StringBuilder sb = new System.Text.StringBuilder ();
					string[] enumNames = val.ToString().Split (',');
					foreach (string name in enumNames)
					{
						switch (name.Trim())
						{
							case "Empty": sb.Append ("").Append (' '); break; 
							case "Substitution": sb.Append ("substitution").Append (' '); break; 
							case "Extension": sb.Append ("extension").Append (' '); break; 
							case "Restriction": sb.Append ("restriction").Append (' '); break; 
							case "List": sb.Append ("list").Append (' '); break; 
							case "Union": sb.Append ("union").Append (' '); break; 
							case "All": sb.Append ("#all").Append (' '); break; 
							default: sb.Append (name.Trim()).Append (' '); break; 
						}
					}
					return sb.ToString ().Trim();
			}
		}

		void WriteObject_XmlSchemaInclude (System.Xml.Schema.XmlSchemaInclude ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaInclude))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaInclude", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o4 = ob.@UnhandledAttributes;
			if (o4 != null) {
				foreach (XmlAttribute o5 in o4)
					if (o5.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o5, ob);
			}

			WriteAttribute ("schemaLocation", "", ob.@SchemaLocation);
			WriteAttribute ("id", "", ob.@Id);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaImport (System.Xml.Schema.XmlSchemaImport ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaImport))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaImport", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o6 = ob.@UnhandledAttributes;
			if (o6 != null) {
				foreach (XmlAttribute o7 in o6)
					if (o7.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o7, ob);
			}

			WriteAttribute ("schemaLocation", "", ob.@SchemaLocation);
			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("namespace", "", ob.@Namespace);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaRedefine (System.Xml.Schema.XmlSchemaRedefine ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaRedefine))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaRedefine", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o8 = ob.@UnhandledAttributes;
			if (o8 != null) {
				foreach (XmlAttribute o9 in o8)
					if (o9.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o9, ob);
			}

			WriteAttribute ("schemaLocation", "", ob.@SchemaLocation);
			WriteAttribute ("id", "", ob.@Id);

			if (ob.@Items != null) {
				for (int n10 = 0; n10 < ob.@Items.Count; n10++) {
					if (ob.@Items[n10] == null) { }
					else if (ob.@Items[n10].GetType() == typeof(System.Xml.Schema.XmlSchemaGroup)) {
						WriteObject_XmlSchemaGroup (((System.Xml.Schema.XmlSchemaGroup) ob.@Items[n10]), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n10].GetType() == typeof(System.Xml.Schema.XmlSchemaComplexType)) {
						WriteObject_XmlSchemaComplexType (((System.Xml.Schema.XmlSchemaComplexType) ob.@Items[n10]), "complexType", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n10].GetType() == typeof(System.Xml.Schema.XmlSchemaSimpleType)) {
						WriteObject_XmlSchemaSimpleType (((System.Xml.Schema.XmlSchemaSimpleType) ob.@Items[n10]), "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n10].GetType() == typeof(System.Xml.Schema.XmlSchemaAnnotation)) {
						WriteObject_XmlSchemaAnnotation (((System.Xml.Schema.XmlSchemaAnnotation) ob.@Items[n10]), "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n10].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroup)) {
						WriteObject_XmlSchemaAttributeGroup (((System.Xml.Schema.XmlSchemaAttributeGroup) ob.@Items[n10]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Items[n10]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaElement (System.Xml.Schema.XmlSchemaElement ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaElement))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaElement", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o11 = ob.@UnhandledAttributes;
			if (o11 != null) {
				foreach (XmlAttribute o12 in o11)
					if (o12.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o12, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);
			if (ob.@IsAbstract != false) {
				WriteAttribute ("abstract", "", (ob.@IsAbstract?"true":"false"));
			}
			if (ob.@Block != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("block", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@Block));
			}
			if (ob.@DefaultValue != null) {
				WriteAttribute ("default", "", ob.@DefaultValue);
			}
			if (ob.@Final != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("final", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@Final));
			}
			if (ob.@FixedValue != null) {
				WriteAttribute ("fixed", "", ob.@FixedValue);
			}
			if (ob.@Form != ((System.Xml.Schema.XmlSchemaForm) System.Xml.Schema.XmlSchemaForm.None)) {
				WriteAttribute ("form", "", GetEnumValue_XmlSchemaForm (ob.@Form));
			}
			if (ob.@Name != null) {
				WriteAttribute ("name", "", ob.@Name);
			}
			if (ob.@IsNillable != false) {
				WriteAttribute ("nillable", "", (ob.@IsNillable?"true":"false"));
			}
			WriteAttribute ("ref", "", FromXmlQualifiedName (ob.@RefName));
			WriteAttribute ("substitutionGroup", "", FromXmlQualifiedName (ob.@SubstitutionGroup));
			WriteAttribute ("type", "", FromXmlQualifiedName (ob.@SchemaTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@SchemaType is System.Xml.Schema.XmlSchemaSimpleType) {
				WriteObject_XmlSchemaSimpleType (((System.Xml.Schema.XmlSchemaSimpleType) ob.@SchemaType), "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@SchemaType is System.Xml.Schema.XmlSchemaComplexType) {
				WriteObject_XmlSchemaComplexType (((System.Xml.Schema.XmlSchemaComplexType) ob.@SchemaType), "complexType", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (ob.@Constraints != null) {
				for (int n13 = 0; n13 < ob.@Constraints.Count; n13++) {
					if (ob.@Constraints[n13] == null) { }
					else if (ob.@Constraints[n13].GetType() == typeof(System.Xml.Schema.XmlSchemaKeyref)) {
						WriteObject_XmlSchemaKeyref (((System.Xml.Schema.XmlSchemaKeyref) ob.@Constraints[n13]), "keyref", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Constraints[n13].GetType() == typeof(System.Xml.Schema.XmlSchemaKey)) {
						WriteObject_XmlSchemaKey (((System.Xml.Schema.XmlSchemaKey) ob.@Constraints[n13]), "key", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Constraints[n13].GetType() == typeof(System.Xml.Schema.XmlSchemaUnique)) {
						WriteObject_XmlSchemaUnique (((System.Xml.Schema.XmlSchemaUnique) ob.@Constraints[n13]), "unique", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Constraints[n13]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleType (System.Xml.Schema.XmlSchemaSimpleType ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleType))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleType", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o14 = ob.@UnhandledAttributes;
			if (o14 != null) {
				foreach (XmlAttribute o15 in o14)
					if (o15.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o15, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);
			if (ob.@Final != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("final", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@Final));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Content is System.Xml.Schema.XmlSchemaSimpleTypeUnion) {
				WriteObject_XmlSchemaSimpleTypeUnion (((System.Xml.Schema.XmlSchemaSimpleTypeUnion) ob.@Content), "union", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Content is System.Xml.Schema.XmlSchemaSimpleTypeList) {
				WriteObject_XmlSchemaSimpleTypeList (((System.Xml.Schema.XmlSchemaSimpleTypeList) ob.@Content), "list", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Content is System.Xml.Schema.XmlSchemaSimpleTypeRestriction) {
				WriteObject_XmlSchemaSimpleTypeRestriction (((System.Xml.Schema.XmlSchemaSimpleTypeRestriction) ob.@Content), "restriction", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAttribute (System.Xml.Schema.XmlSchemaAttribute ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAttribute))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAttribute", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o16 = ob.@UnhandledAttributes;
			if (o16 != null) {
				foreach (XmlAttribute o17 in o16)
					if (o17.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o17, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			if (ob.@DefaultValue != null) {
				WriteAttribute ("default", "", ob.@DefaultValue);
			}
			if (ob.@FixedValue != null) {
				WriteAttribute ("fixed", "", ob.@FixedValue);
			}
			if (ob.@Form != ((System.Xml.Schema.XmlSchemaForm) System.Xml.Schema.XmlSchemaForm.None)) {
				WriteAttribute ("form", "", GetEnumValue_XmlSchemaForm (ob.@Form));
			}
			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("ref", "", FromXmlQualifiedName (ob.@RefName));
			WriteAttribute ("type", "", FromXmlQualifiedName (ob.@SchemaTypeName));
			if (ob.@Use != ((System.Xml.Schema.XmlSchemaUse) System.Xml.Schema.XmlSchemaUse.None)) {
				WriteAttribute ("use", "", GetEnumValue_XmlSchemaUse (ob.@Use));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaSimpleType (ob.@SchemaType, "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAnnotation (System.Xml.Schema.XmlSchemaAnnotation ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAnnotation))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAnnotation", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o18 = ob.@UnhandledAttributes;
			if (o18 != null) {
				foreach (XmlAttribute o19 in o18)
					if (o19.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o19, ob);
			}

			WriteAttribute ("id", "", ob.@Id);

			if (ob.@Items != null) {
				for (int n20 = 0; n20 < ob.@Items.Count; n20++) {
					if (ob.@Items[n20] == null) { }
					else if (ob.@Items[n20].GetType() == typeof(System.Xml.Schema.XmlSchemaAppInfo)) {
						WriteObject_XmlSchemaAppInfo (((System.Xml.Schema.XmlSchemaAppInfo) ob.@Items[n20]), "appinfo", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n20].GetType() == typeof(System.Xml.Schema.XmlSchemaDocumentation)) {
						WriteObject_XmlSchemaDocumentation (((System.Xml.Schema.XmlSchemaDocumentation) ob.@Items[n20]), "documentation", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Items[n20]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAttributeGroup (System.Xml.Schema.XmlSchemaAttributeGroup ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAttributeGroup))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAttributeGroup", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o21 = ob.@UnhandledAttributes;
			if (o21 != null) {
				foreach (XmlAttribute o22 in o21)
					if (o22.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o22, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Attributes != null) {
				for (int n23 = 0; n23 < ob.@Attributes.Count; n23++) {
					if (ob.@Attributes[n23] == null) { }
					else if (ob.@Attributes[n23].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Attributes[n23]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Attributes[n23].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
						WriteObject_XmlSchemaAttributeGroupRef (((System.Xml.Schema.XmlSchemaAttributeGroupRef) ob.@Attributes[n23]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Attributes[n23]);
				}
			}
			WriteObject_XmlSchemaAnyAttribute (ob.@AnyAttribute, "anyAttribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaGroup (System.Xml.Schema.XmlSchemaGroup ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaGroup))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaGroup", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o24 = ob.@UnhandledAttributes;
			if (o24 != null) {
				foreach (XmlAttribute o25 in o24)
					if (o25.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o25, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Particle is System.Xml.Schema.XmlSchemaSequence) {
				WriteObject_XmlSchemaSequence (((System.Xml.Schema.XmlSchemaSequence) ob.@Particle), "sequence", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaChoice) {
				WriteObject_XmlSchemaChoice (((System.Xml.Schema.XmlSchemaChoice) ob.@Particle), "choice", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaAll) {
				WriteObject_XmlSchemaAll (((System.Xml.Schema.XmlSchemaAll) ob.@Particle), "all", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaComplexType (System.Xml.Schema.XmlSchemaComplexType ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaComplexType))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaComplexType", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o26 = ob.@UnhandledAttributes;
			if (o26 != null) {
				foreach (XmlAttribute o27 in o26)
					if (o27.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o27, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);
			if (ob.@Final != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("final", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@Final));
			}
			if (ob.@IsAbstract != false) {
				WriteAttribute ("abstract", "", (ob.@IsAbstract?"true":"false"));
			}
			if (ob.@Block != ((System.Xml.Schema.XmlSchemaDerivationMethod) System.Xml.Schema.XmlSchemaDerivationMethod.None)) {
				WriteAttribute ("block", "", GetEnumValue_XmlSchemaDerivationMethod (ob.@Block));
			}
			if (ob.@IsMixed != false) {
				WriteAttribute ("mixed", "", (ob.@IsMixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@ContentModel is System.Xml.Schema.XmlSchemaComplexContent) {
				WriteObject_XmlSchemaComplexContent (((System.Xml.Schema.XmlSchemaComplexContent) ob.@ContentModel), "complexContent", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@ContentModel is System.Xml.Schema.XmlSchemaSimpleContent) {
				WriteObject_XmlSchemaSimpleContent (((System.Xml.Schema.XmlSchemaSimpleContent) ob.@ContentModel), "simpleContent", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (ob.@Particle is System.Xml.Schema.XmlSchemaAll) {
				WriteObject_XmlSchemaAll (((System.Xml.Schema.XmlSchemaAll) ob.@Particle), "all", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaGroupRef) {
				WriteObject_XmlSchemaGroupRef (((System.Xml.Schema.XmlSchemaGroupRef) ob.@Particle), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaSequence) {
				WriteObject_XmlSchemaSequence (((System.Xml.Schema.XmlSchemaSequence) ob.@Particle), "sequence", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaChoice) {
				WriteObject_XmlSchemaChoice (((System.Xml.Schema.XmlSchemaChoice) ob.@Particle), "choice", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (ob.@Attributes != null) {
				for (int n28 = 0; n28 < ob.@Attributes.Count; n28++) {
					if (ob.@Attributes[n28] == null) { }
					else if (ob.@Attributes[n28].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
						WriteObject_XmlSchemaAttributeGroupRef (((System.Xml.Schema.XmlSchemaAttributeGroupRef) ob.@Attributes[n28]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Attributes[n28].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Attributes[n28]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Attributes[n28]);
				}
			}
			WriteObject_XmlSchemaAnyAttribute (ob.@AnyAttribute, "anyAttribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaNotation (System.Xml.Schema.XmlSchemaNotation ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaNotation))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaNotation", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o29 = ob.@UnhandledAttributes;
			if (o29 != null) {
				foreach (XmlAttribute o30 in o29)
					if (o30.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o30, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("public", "", ob.@Public);
			WriteAttribute ("system", "", ob.@System);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaKeyref (System.Xml.Schema.XmlSchemaKeyref ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaKeyref))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaKeyref", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o31 = ob.@UnhandledAttributes;
			if (o31 != null) {
				foreach (XmlAttribute o32 in o31)
					if (o32.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o32, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);
			WriteAttribute ("refer", "", FromXmlQualifiedName (ob.@Refer));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaXPath (ob.@Selector, "selector", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Fields != null) {
				for (int n33 = 0; n33 < ob.@Fields.Count; n33++) {
					WriteObject_XmlSchemaXPath (((System.Xml.Schema.XmlSchemaXPath) ob.@Fields[n33]), "field", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaKey (System.Xml.Schema.XmlSchemaKey ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaKey))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaKey", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o34 = ob.@UnhandledAttributes;
			if (o34 != null) {
				foreach (XmlAttribute o35 in o34)
					if (o35.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o35, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaXPath (ob.@Selector, "selector", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Fields != null) {
				for (int n36 = 0; n36 < ob.@Fields.Count; n36++) {
					WriteObject_XmlSchemaXPath (((System.Xml.Schema.XmlSchemaXPath) ob.@Fields[n36]), "field", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaUnique (System.Xml.Schema.XmlSchemaUnique ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaUnique))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaUnique", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o37 = ob.@UnhandledAttributes;
			if (o37 != null) {
				foreach (XmlAttribute o38 in o37)
					if (o38.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o38, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("name", "", ob.@Name);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaXPath (ob.@Selector, "selector", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Fields != null) {
				for (int n39 = 0; n39 < ob.@Fields.Count; n39++) {
					WriteObject_XmlSchemaXPath (((System.Xml.Schema.XmlSchemaXPath) ob.@Fields[n39]), "field", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleTypeUnion (System.Xml.Schema.XmlSchemaSimpleTypeUnion ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeUnion))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleTypeUnion", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o40 = ob.@UnhandledAttributes;
			if (o40 != null) {
				foreach (XmlAttribute o41 in o40)
					if (o41.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o41, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			string s42 = null;
			if (ob.@MemberTypes != null) {
				System.Text.StringBuilder s43 = new System.Text.StringBuilder();
				for (int n44 = 0; n44 < ob.@MemberTypes.Length; n44++) {
					s43.Append (FromXmlQualifiedName (ob.@MemberTypes[n44])).Append (" ");
				}
				s42 = s43.ToString ().Trim ();
			}
			WriteAttribute ("memberTypes", "", s42);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@BaseTypes != null) {
				for (int n45 = 0; n45 < ob.@BaseTypes.Count; n45++) {
					WriteObject_XmlSchemaSimpleType (((System.Xml.Schema.XmlSchemaSimpleType) ob.@BaseTypes[n45]), "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleTypeList (System.Xml.Schema.XmlSchemaSimpleTypeList ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeList))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleTypeList", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o46 = ob.@UnhandledAttributes;
			if (o46 != null) {
				foreach (XmlAttribute o47 in o46)
					if (o47.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o47, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("itemType", "", FromXmlQualifiedName (ob.@ItemTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaSimpleType (ob.@ItemType, "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleTypeRestriction (System.Xml.Schema.XmlSchemaSimpleTypeRestriction ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleTypeRestriction))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleTypeRestriction", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o48 = ob.@UnhandledAttributes;
			if (o48 != null) {
				foreach (XmlAttribute o49 in o48)
					if (o49.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o49, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("base", "", FromXmlQualifiedName (ob.@BaseTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaSimpleType (ob.@BaseType, "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Facets != null) {
				for (int n50 = 0; n50 < ob.@Facets.Count; n50++) {
					if (ob.@Facets[n50] == null) { }
					else if (ob.@Facets[n50].GetType() == typeof(System.Xml.Schema.XmlSchemaMaxLengthFacet)) {
						WriteObject_XmlSchemaMaxLengthFacet (((System.Xml.Schema.XmlSchemaMaxLengthFacet) ob.@Facets[n50]), "maxLength", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n50].GetType() == typeof(System.Xml.Schema.XmlSchemaMinLengthFacet)) {
						WriteObject_XmlSchemaMinLengthFacet (((System.Xml.Schema.XmlSchemaMinLengthFacet) ob.@Facets[n50]), "minLength", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n50].GetType() == typeof(System.Xml.Schema.XmlSchemaLengthFacet)) {
						WriteObject_XmlSchemaLengthFacet (((System.Xml.Schema.XmlSchemaLengthFacet) ob.@Facets[n50]), "length", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n50].GetType() == typeof(System.Xml.Schema.XmlSchemaFractionDigitsFacet)) {
						WriteObject_XmlSchemaFractionDigitsFacet (((System.Xml.Schema.XmlSchemaFractionDigitsFacet) ob.@Facets[n50]), "fractionDigits", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n50].GetType() == typeof(System.Xml.Schema.XmlSchemaMaxInclusiveFacet)) {
						WriteObject_XmlSchemaMaxInclusiveFacet (((System.Xml.Schema.XmlSchemaMaxInclusiveFacet) ob.@Facets[n50]), "maxInclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n50].GetType() == typeof(System.Xml.Schema.XmlSchemaMaxExclusiveFacet)) {
						WriteObject_XmlSchemaMaxExclusiveFacet (((System.Xml.Schema.XmlSchemaMaxExclusiveFacet) ob.@Facets[n50]), "maxExclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n50].GetType() == typeof(System.Xml.Schema.XmlSchemaMinExclusiveFacet)) {
						WriteObject_XmlSchemaMinExclusiveFacet (((System.Xml.Schema.XmlSchemaMinExclusiveFacet) ob.@Facets[n50]), "minExclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n50].GetType() == typeof(System.Xml.Schema.XmlSchemaEnumerationFacet)) {
						WriteObject_XmlSchemaEnumerationFacet (((System.Xml.Schema.XmlSchemaEnumerationFacet) ob.@Facets[n50]), "enumeration", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n50].GetType() == typeof(System.Xml.Schema.XmlSchemaTotalDigitsFacet)) {
						WriteObject_XmlSchemaTotalDigitsFacet (((System.Xml.Schema.XmlSchemaTotalDigitsFacet) ob.@Facets[n50]), "totalDigits", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n50].GetType() == typeof(System.Xml.Schema.XmlSchemaMinInclusiveFacet)) {
						WriteObject_XmlSchemaMinInclusiveFacet (((System.Xml.Schema.XmlSchemaMinInclusiveFacet) ob.@Facets[n50]), "minInclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n50].GetType() == typeof(System.Xml.Schema.XmlSchemaWhiteSpaceFacet)) {
						WriteObject_XmlSchemaWhiteSpaceFacet (((System.Xml.Schema.XmlSchemaWhiteSpaceFacet) ob.@Facets[n50]), "whiteSpace", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n50].GetType() == typeof(System.Xml.Schema.XmlSchemaPatternFacet)) {
						WriteObject_XmlSchemaPatternFacet (((System.Xml.Schema.XmlSchemaPatternFacet) ob.@Facets[n50]), "pattern", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Facets[n50]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}
/*
		void WriteObject_XmlSchemaUse (System.Xml.Schema.XmlSchemaUse ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaUse))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaUse", "");

			Writer.WriteString (GetEnumValue_XmlSchemaUse (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}
*/
		string GetEnumValue_XmlSchemaUse (System.Xml.Schema.XmlSchemaUse val)
		{
			switch (val)
			{
				case System.Xml.Schema.XmlSchemaUse.Optional: return "optional";
				case System.Xml.Schema.XmlSchemaUse.Prohibited: return "prohibited";
				case System.Xml.Schema.XmlSchemaUse.Required: return "required";
				default: return ((long)val).ToString(CultureInfo.InvariantCulture);
			}
		}

		void WriteObject_XmlSchemaAppInfo (System.Xml.Schema.XmlSchemaAppInfo ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAppInfo))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAppInfo", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			WriteAttribute ("source", "", ob.@Source);

			if (ob.@Markup != null) {
				foreach (XmlNode o51 in ob.@Markup) {
					XmlNode o52 = o51;
					if (o52 is XmlElement) {
						WriteElementLiteral (o52, "", "", false, true);
					} else o52.WriteTo (Writer);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaDocumentation (System.Xml.Schema.XmlSchemaDocumentation ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaDocumentation))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaDocumentation", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			WriteAttribute ("source", "", ob.@Source);
			WriteAttribute ("xml:lang", "", ob.@Language);

			if (ob.@Markup != null) {
				foreach (XmlNode o53 in ob.@Markup) {
					XmlNode o54 = o53;
					if (o54 is XmlElement) {
						WriteElementLiteral (o54, "", "", false, true);
					} else o54.WriteTo (Writer);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAttributeGroupRef (System.Xml.Schema.XmlSchemaAttributeGroupRef ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAttributeGroupRef", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o55 = ob.@UnhandledAttributes;
			if (o55 != null) {
				foreach (XmlAttribute o56 in o55)
					if (o56.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o56, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("ref", "", FromXmlQualifiedName (ob.@RefName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAnyAttribute (System.Xml.Schema.XmlSchemaAnyAttribute ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAnyAttribute))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAnyAttribute", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o57 = ob.@UnhandledAttributes;
			if (o57 != null) {
				foreach (XmlAttribute o58 in o57)
					if (o58.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o58, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("namespace", "", ob.@Namespace);
			if (ob.@ProcessContents != ((System.Xml.Schema.XmlSchemaContentProcessing) System.Xml.Schema.XmlSchemaContentProcessing.None)) {
				WriteAttribute ("processContents", "", GetEnumValue_XmlSchemaContentProcessing (ob.@ProcessContents));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSequence (System.Xml.Schema.XmlSchemaSequence ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSequence))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSequence", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o59 = ob.@UnhandledAttributes;
			if (o59 != null) {
				foreach (XmlAttribute o60 in o59)
					if (o60.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o60, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Items != null) {
				for (int n61 = 0; n61 < ob.@Items.Count; n61++) {
					if (ob.@Items[n61] == null) { }
					else if (ob.@Items[n61].GetType() == typeof(System.Xml.Schema.XmlSchemaSequence)) {
						WriteObject_XmlSchemaSequence (((System.Xml.Schema.XmlSchemaSequence) ob.@Items[n61]), "sequence", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n61].GetType() == typeof(System.Xml.Schema.XmlSchemaChoice)) {
						WriteObject_XmlSchemaChoice (((System.Xml.Schema.XmlSchemaChoice) ob.@Items[n61]), "choice", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n61].GetType() == typeof(System.Xml.Schema.XmlSchemaGroupRef)) {
						WriteObject_XmlSchemaGroupRef (((System.Xml.Schema.XmlSchemaGroupRef) ob.@Items[n61]), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n61].GetType() == typeof(System.Xml.Schema.XmlSchemaElement)) {
						WriteObject_XmlSchemaElement (((System.Xml.Schema.XmlSchemaElement) ob.@Items[n61]), "element", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n61].GetType() == typeof(System.Xml.Schema.XmlSchemaAny)) {
						WriteObject_XmlSchemaAny (((System.Xml.Schema.XmlSchemaAny) ob.@Items[n61]), "any", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Items[n61]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaChoice (System.Xml.Schema.XmlSchemaChoice ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaChoice))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaChoice", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o62 = ob.@UnhandledAttributes;
			if (o62 != null) {
				foreach (XmlAttribute o63 in o62)
					if (o63.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o63, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Items != null) {
				for (int n64 = 0; n64 < ob.@Items.Count; n64++) {
					if (ob.@Items[n64] == null) { }
					else if (ob.@Items[n64].GetType() == typeof(System.Xml.Schema.XmlSchemaGroupRef)) {
						WriteObject_XmlSchemaGroupRef (((System.Xml.Schema.XmlSchemaGroupRef) ob.@Items[n64]), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n64].GetType() == typeof(System.Xml.Schema.XmlSchemaElement)) {
						WriteObject_XmlSchemaElement (((System.Xml.Schema.XmlSchemaElement) ob.@Items[n64]), "element", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n64].GetType() == typeof(System.Xml.Schema.XmlSchemaAny)) {
						WriteObject_XmlSchemaAny (((System.Xml.Schema.XmlSchemaAny) ob.@Items[n64]), "any", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n64].GetType() == typeof(System.Xml.Schema.XmlSchemaSequence)) {
						WriteObject_XmlSchemaSequence (((System.Xml.Schema.XmlSchemaSequence) ob.@Items[n64]), "sequence", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Items[n64].GetType() == typeof(System.Xml.Schema.XmlSchemaChoice)) {
						WriteObject_XmlSchemaChoice (((System.Xml.Schema.XmlSchemaChoice) ob.@Items[n64]), "choice", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Items[n64]);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaAll (System.Xml.Schema.XmlSchemaAll ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAll))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAll", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o65 = ob.@UnhandledAttributes;
			if (o65 != null) {
				foreach (XmlAttribute o66 in o65)
					if (o66.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o66, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Items != null) {
				for (int n67 = 0; n67 < ob.@Items.Count; n67++) {
					WriteObject_XmlSchemaElement (((System.Xml.Schema.XmlSchemaElement) ob.@Items[n67]), "element", "http://www.w3.org/2001/XMLSchema", false, false, true);
				}
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaComplexContent (System.Xml.Schema.XmlSchemaComplexContent ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaComplexContent))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaComplexContent", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o68 = ob.@UnhandledAttributes;
			if (o68 != null) {
				foreach (XmlAttribute o69 in o68)
					if (o69.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o69, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("mixed", "", (ob.@IsMixed?"true":"false"));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Content is System.Xml.Schema.XmlSchemaComplexContentExtension) {
				WriteObject_XmlSchemaComplexContentExtension (((System.Xml.Schema.XmlSchemaComplexContentExtension) ob.@Content), "extension", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Content is System.Xml.Schema.XmlSchemaComplexContentRestriction) {
				WriteObject_XmlSchemaComplexContentRestriction (((System.Xml.Schema.XmlSchemaComplexContentRestriction) ob.@Content), "restriction", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleContent (System.Xml.Schema.XmlSchemaSimpleContent ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContent))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleContent", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o70 = ob.@UnhandledAttributes;
			if (o70 != null) {
				foreach (XmlAttribute o71 in o70)
					if (o71.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o71, ob);
			}

			WriteAttribute ("id", "", ob.@Id);

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Content is System.Xml.Schema.XmlSchemaSimpleContentExtension) {
				WriteObject_XmlSchemaSimpleContentExtension (((System.Xml.Schema.XmlSchemaSimpleContentExtension) ob.@Content), "extension", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Content is System.Xml.Schema.XmlSchemaSimpleContentRestriction) {
				WriteObject_XmlSchemaSimpleContentRestriction (((System.Xml.Schema.XmlSchemaSimpleContentRestriction) ob.@Content), "restriction", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaGroupRef (System.Xml.Schema.XmlSchemaGroupRef ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaGroupRef))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaGroupRef", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o72 = ob.@UnhandledAttributes;
			if (o72 != null) {
				foreach (XmlAttribute o73 in o72)
					if (o73.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o73, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);
			WriteAttribute ("ref", "", FromXmlQualifiedName (ob.@RefName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaXPath (System.Xml.Schema.XmlSchemaXPath ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaXPath))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaXPath", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o74 = ob.@UnhandledAttributes;
			if (o74 != null) {
				foreach (XmlAttribute o75 in o74)
					if (o75.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o75, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			if (ob.@XPath != null) {
				WriteAttribute ("xpath", "", ob.@XPath);
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaMaxLengthFacet (System.Xml.Schema.XmlSchemaMaxLengthFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaMaxLengthFacet))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaMaxLengthFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o76 = ob.@UnhandledAttributes;
			if (o76 != null) {
				foreach (XmlAttribute o77 in o76)
					if (o77.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o77, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaMinLengthFacet (System.Xml.Schema.XmlSchemaMinLengthFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaMinLengthFacet))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaMinLengthFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o78 = ob.@UnhandledAttributes;
			if (o78 != null) {
				foreach (XmlAttribute o79 in o78)
					if (o79.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o79, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaLengthFacet (System.Xml.Schema.XmlSchemaLengthFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaLengthFacet))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaLengthFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o80 = ob.@UnhandledAttributes;
			if (o80 != null) {
				foreach (XmlAttribute o81 in o80)
					if (o81.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o81, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaFractionDigitsFacet (System.Xml.Schema.XmlSchemaFractionDigitsFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaFractionDigitsFacet))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaFractionDigitsFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o82 = ob.@UnhandledAttributes;
			if (o82 != null) {
				foreach (XmlAttribute o83 in o82)
					if (o83.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o83, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaMaxInclusiveFacet (System.Xml.Schema.XmlSchemaMaxInclusiveFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaMaxInclusiveFacet))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaMaxInclusiveFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o84 = ob.@UnhandledAttributes;
			if (o84 != null) {
				foreach (XmlAttribute o85 in o84)
					if (o85.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o85, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaMaxExclusiveFacet (System.Xml.Schema.XmlSchemaMaxExclusiveFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaMaxExclusiveFacet))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaMaxExclusiveFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o86 = ob.@UnhandledAttributes;
			if (o86 != null) {
				foreach (XmlAttribute o87 in o86)
					if (o87.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o87, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaMinExclusiveFacet (System.Xml.Schema.XmlSchemaMinExclusiveFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaMinExclusiveFacet))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaMinExclusiveFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o88 = ob.@UnhandledAttributes;
			if (o88 != null) {
				foreach (XmlAttribute o89 in o88)
					if (o89.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o89, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaEnumerationFacet (System.Xml.Schema.XmlSchemaEnumerationFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaEnumerationFacet))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaEnumerationFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o90 = ob.@UnhandledAttributes;
			if (o90 != null) {
				foreach (XmlAttribute o91 in o90)
					if (o91.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o91, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaTotalDigitsFacet (System.Xml.Schema.XmlSchemaTotalDigitsFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaTotalDigitsFacet))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaTotalDigitsFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o92 = ob.@UnhandledAttributes;
			if (o92 != null) {
				foreach (XmlAttribute o93 in o92)
					if (o93.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o93, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaMinInclusiveFacet (System.Xml.Schema.XmlSchemaMinInclusiveFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaMinInclusiveFacet))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaMinInclusiveFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o94 = ob.@UnhandledAttributes;
			if (o94 != null) {
				foreach (XmlAttribute o95 in o94)
					if (o95.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o95, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaWhiteSpaceFacet (System.Xml.Schema.XmlSchemaWhiteSpaceFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaWhiteSpaceFacet))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaWhiteSpaceFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o96 = ob.@UnhandledAttributes;
			if (o96 != null) {
				foreach (XmlAttribute o97 in o96)
					if (o97.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o97, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaPatternFacet (System.Xml.Schema.XmlSchemaPatternFacet ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaPatternFacet))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaPatternFacet", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o98 = ob.@UnhandledAttributes;
			if (o98 != null) {
				foreach (XmlAttribute o99 in o98)
					if (o99.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o99, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("value", "", ob.@Value);
			if (ob.@IsFixed != false) {
				WriteAttribute ("fixed", "", (ob.@IsFixed?"true":"false"));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}
/*
		void WriteObject_XmlSchemaContentProcessing (System.Xml.Schema.XmlSchemaContentProcessing ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaContentProcessing))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaContentProcessing", "");

			Writer.WriteString (GetEnumValue_XmlSchemaContentProcessing (ob));
			if (writeWrappingElem) WriteEndElement (ob);
		}
*/
		string GetEnumValue_XmlSchemaContentProcessing (System.Xml.Schema.XmlSchemaContentProcessing val)
		{
			switch (val)
			{
				case System.Xml.Schema.XmlSchemaContentProcessing.Skip: return "skip";
				case System.Xml.Schema.XmlSchemaContentProcessing.Lax: return "lax";
				case System.Xml.Schema.XmlSchemaContentProcessing.Strict: return "strict";
				default: return ((long)val).ToString(CultureInfo.InvariantCulture);
			}
		}

		void WriteObject_XmlSchemaAny (System.Xml.Schema.XmlSchemaAny ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaAny))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaAny", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o100 = ob.@UnhandledAttributes;
			if (o100 != null) {
				foreach (XmlAttribute o101 in o100)
					if (o101.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o101, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("minOccurs", "", ob.@MinOccursString);
			WriteAttribute ("maxOccurs", "", ob.@MaxOccursString);
			WriteAttribute ("namespace", "", ob.@Namespace);
			if (ob.@ProcessContents != ((System.Xml.Schema.XmlSchemaContentProcessing) System.Xml.Schema.XmlSchemaContentProcessing.None)) {
				WriteAttribute ("processContents", "", GetEnumValue_XmlSchemaContentProcessing (ob.@ProcessContents));
			}

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaComplexContentExtension (System.Xml.Schema.XmlSchemaComplexContentExtension ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaComplexContentExtension))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaComplexContentExtension", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o102 = ob.@UnhandledAttributes;
			if (o102 != null) {
				foreach (XmlAttribute o103 in o102)
					if (o103.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o103, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("base", "", FromXmlQualifiedName (ob.@BaseTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Particle is System.Xml.Schema.XmlSchemaGroupRef) {
				WriteObject_XmlSchemaGroupRef (((System.Xml.Schema.XmlSchemaGroupRef) ob.@Particle), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaSequence) {
				WriteObject_XmlSchemaSequence (((System.Xml.Schema.XmlSchemaSequence) ob.@Particle), "sequence", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaChoice) {
				WriteObject_XmlSchemaChoice (((System.Xml.Schema.XmlSchemaChoice) ob.@Particle), "choice", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaAll) {
				WriteObject_XmlSchemaAll (((System.Xml.Schema.XmlSchemaAll) ob.@Particle), "all", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (ob.@Attributes != null) {
				for (int n104 = 0; n104 < ob.@Attributes.Count; n104++) {
					if (ob.@Attributes[n104] == null) { }
					else if (ob.@Attributes[n104].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Attributes[n104]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Attributes[n104].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
						WriteObject_XmlSchemaAttributeGroupRef (((System.Xml.Schema.XmlSchemaAttributeGroupRef) ob.@Attributes[n104]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Attributes[n104]);
				}
			}
			WriteObject_XmlSchemaAnyAttribute (ob.@AnyAttribute, "anyAttribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaComplexContentRestriction (System.Xml.Schema.XmlSchemaComplexContentRestriction ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaComplexContentRestriction))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaComplexContentRestriction", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o105 = ob.@UnhandledAttributes;
			if (o105 != null) {
				foreach (XmlAttribute o106 in o105)
					if (o106.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o106, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("base", "", FromXmlQualifiedName (ob.@BaseTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Particle is System.Xml.Schema.XmlSchemaSequence) {
				WriteObject_XmlSchemaSequence (((System.Xml.Schema.XmlSchemaSequence) ob.@Particle), "sequence", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaChoice) {
				WriteObject_XmlSchemaChoice (((System.Xml.Schema.XmlSchemaChoice) ob.@Particle), "choice", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaGroupRef) {
				WriteObject_XmlSchemaGroupRef (((System.Xml.Schema.XmlSchemaGroupRef) ob.@Particle), "group", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			else if (ob.@Particle is System.Xml.Schema.XmlSchemaAll) {
				WriteObject_XmlSchemaAll (((System.Xml.Schema.XmlSchemaAll) ob.@Particle), "all", "http://www.w3.org/2001/XMLSchema", false, false, true);
			}
			if (ob.@Attributes != null) {
				for (int n107 = 0; n107 < ob.@Attributes.Count; n107++) {
					if (ob.@Attributes[n107] == null) { }
					else if (ob.@Attributes[n107].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
						WriteObject_XmlSchemaAttributeGroupRef (((System.Xml.Schema.XmlSchemaAttributeGroupRef) ob.@Attributes[n107]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Attributes[n107].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Attributes[n107]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Attributes[n107]);
				}
			}
			WriteObject_XmlSchemaAnyAttribute (ob.@AnyAttribute, "anyAttribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleContentExtension (System.Xml.Schema.XmlSchemaSimpleContentExtension ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContentExtension))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleContentExtension", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o108 = ob.@UnhandledAttributes;
			if (o108 != null) {
				foreach (XmlAttribute o109 in o108)
					if (o109.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o109, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("base", "", FromXmlQualifiedName (ob.@BaseTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Attributes != null) {
				for (int n110 = 0; n110 < ob.@Attributes.Count; n110++) {
					if (ob.@Attributes[n110] == null) { }
					else if (ob.@Attributes[n110].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
						WriteObject_XmlSchemaAttributeGroupRef (((System.Xml.Schema.XmlSchemaAttributeGroupRef) ob.@Attributes[n110]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Attributes[n110].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Attributes[n110]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Attributes[n110]);
				}
			}
			WriteObject_XmlSchemaAnyAttribute (ob.@AnyAttribute, "anyAttribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		void WriteObject_XmlSchemaSimpleContentRestriction (System.Xml.Schema.XmlSchemaSimpleContentRestriction ob, string element, string namesp, bool isNullable, bool needType, bool writeWrappingElem)
		{
			if (ob == null)
			{
				if (isNullable)
					WriteNullTagLiteral(element, namesp);
				return;
			}

			System.Type type = ob.GetType ();
			if (type == typeof(System.Xml.Schema.XmlSchemaSimpleContentRestriction))
				;
			else {
				throw CreateUnknownTypeException (ob);
			}

			if (writeWrappingElem) {
				WriteStartElement (element, namesp, ob);
			}

			if (needType) WriteXsiType("XmlSchemaSimpleContentRestriction", "http://www.w3.org/2001/XMLSchema");

			WriteNamespaceDeclarations ((XmlSerializerNamespaces) ob.@Namespaces);

			ICollection o111 = ob.@UnhandledAttributes;
			if (o111 != null) {
				foreach (XmlAttribute o112 in o111)
					if (o112.NamespaceURI != xmlNamespace)
						WriteXmlAttribute (o112, ob);
			}

			WriteAttribute ("id", "", ob.@Id);
			WriteAttribute ("base", "", FromXmlQualifiedName (ob.@BaseTypeName));

			WriteObject_XmlSchemaAnnotation (ob.@Annotation, "annotation", "http://www.w3.org/2001/XMLSchema", false, false, true);
			WriteObject_XmlSchemaSimpleType (ob.@BaseType, "simpleType", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (ob.@Facets != null) {
				for (int n113 = 0; n113 < ob.@Facets.Count; n113++) {
					if (ob.@Facets[n113] == null) { }
					else if (ob.@Facets[n113].GetType() == typeof(System.Xml.Schema.XmlSchemaEnumerationFacet)) {
						WriteObject_XmlSchemaEnumerationFacet (((System.Xml.Schema.XmlSchemaEnumerationFacet) ob.@Facets[n113]), "enumeration", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n113].GetType() == typeof(System.Xml.Schema.XmlSchemaMaxLengthFacet)) {
						WriteObject_XmlSchemaMaxLengthFacet (((System.Xml.Schema.XmlSchemaMaxLengthFacet) ob.@Facets[n113]), "maxLength", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n113].GetType() == typeof(System.Xml.Schema.XmlSchemaMinLengthFacet)) {
						WriteObject_XmlSchemaMinLengthFacet (((System.Xml.Schema.XmlSchemaMinLengthFacet) ob.@Facets[n113]), "minLength", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n113].GetType() == typeof(System.Xml.Schema.XmlSchemaLengthFacet)) {
						WriteObject_XmlSchemaLengthFacet (((System.Xml.Schema.XmlSchemaLengthFacet) ob.@Facets[n113]), "length", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n113].GetType() == typeof(System.Xml.Schema.XmlSchemaFractionDigitsFacet)) {
						WriteObject_XmlSchemaFractionDigitsFacet (((System.Xml.Schema.XmlSchemaFractionDigitsFacet) ob.@Facets[n113]), "fractionDigits", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n113].GetType() == typeof(System.Xml.Schema.XmlSchemaTotalDigitsFacet)) {
						WriteObject_XmlSchemaTotalDigitsFacet (((System.Xml.Schema.XmlSchemaTotalDigitsFacet) ob.@Facets[n113]), "totalDigits", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n113].GetType() == typeof(System.Xml.Schema.XmlSchemaMaxInclusiveFacet)) {
						WriteObject_XmlSchemaMaxInclusiveFacet (((System.Xml.Schema.XmlSchemaMaxInclusiveFacet) ob.@Facets[n113]), "maxInclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n113].GetType() == typeof(System.Xml.Schema.XmlSchemaMaxExclusiveFacet)) {
						WriteObject_XmlSchemaMaxExclusiveFacet (((System.Xml.Schema.XmlSchemaMaxExclusiveFacet) ob.@Facets[n113]), "maxExclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n113].GetType() == typeof(System.Xml.Schema.XmlSchemaMinInclusiveFacet)) {
						WriteObject_XmlSchemaMinInclusiveFacet (((System.Xml.Schema.XmlSchemaMinInclusiveFacet) ob.@Facets[n113]), "minInclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n113].GetType() == typeof(System.Xml.Schema.XmlSchemaMinExclusiveFacet)) {
						WriteObject_XmlSchemaMinExclusiveFacet (((System.Xml.Schema.XmlSchemaMinExclusiveFacet) ob.@Facets[n113]), "minExclusive", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n113].GetType() == typeof(System.Xml.Schema.XmlSchemaWhiteSpaceFacet)) {
						WriteObject_XmlSchemaWhiteSpaceFacet (((System.Xml.Schema.XmlSchemaWhiteSpaceFacet) ob.@Facets[n113]), "whiteSpace", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Facets[n113].GetType() == typeof(System.Xml.Schema.XmlSchemaPatternFacet)) {
						WriteObject_XmlSchemaPatternFacet (((System.Xml.Schema.XmlSchemaPatternFacet) ob.@Facets[n113]), "pattern", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Facets[n113]);
				}
			}
			if (ob.@Attributes != null) {
				for (int n114 = 0; n114 < ob.@Attributes.Count; n114++) {
					if (ob.@Attributes[n114] == null) { }
					else if (ob.@Attributes[n114].GetType() == typeof(System.Xml.Schema.XmlSchemaAttributeGroupRef)) {
						WriteObject_XmlSchemaAttributeGroupRef (((System.Xml.Schema.XmlSchemaAttributeGroupRef) ob.@Attributes[n114]), "attributeGroup", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else if (ob.@Attributes[n114].GetType() == typeof(System.Xml.Schema.XmlSchemaAttribute)) {
						WriteObject_XmlSchemaAttribute (((System.Xml.Schema.XmlSchemaAttribute) ob.@Attributes[n114]), "attribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
					}
					else throw CreateUnknownTypeException (ob.@Attributes[n114]);
				}
			}
			WriteObject_XmlSchemaAnyAttribute (ob.@AnyAttribute, "anyAttribute", "http://www.w3.org/2001/XMLSchema", false, false, true);
			if (writeWrappingElem) WriteEndElement (ob);
		}

		protected override void InitCallbacks ()
		{
		}

	}

}

