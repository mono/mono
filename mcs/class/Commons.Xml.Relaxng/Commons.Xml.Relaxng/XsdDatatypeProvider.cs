//
// XsdDatatypeProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (c) 2004 Novell Inc.
// All rights reserved
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;

using XSchema = System.Xml.Schema.XmlSchema;

namespace Commons.Xml.Relaxng.XmlSchema
{
	public class XsdDatatypeProvider : RelaxngDatatypeProvider
	{
		static XsdDatatypeProvider instance = new XsdDatatypeProvider ();
		static Hashtable table;
		static XsdQNameWrapper qnameType = new XsdQNameWrapper ();

		private XsdDatatypeProvider ()
		{
			if (table != null)
				return;

			table = new Hashtable ();

			// TODO: fill all type names.
			string [] names = new string [] {
				"anySimpleType",
				"string",
				"normalizedString",
				"token",
				"language",
				"NMTOKEN", "NMTOKENS",
				"Name", "NCName",
				"ID", "IDREF", "IDREFS",
				"ENTITY", "ENTITIES", //"NOTATION",
				"decimal", 
				"integer", "long", "int", "short", "byte",
				"nonPositiveInteger", "negativeInteger",
				"nonNegativeInteger", "positiveInteger",
				"unsignedLong", "unsignedInt", 
				"unsignedShort", "unsignedByte",
				"double", "float", 
				"base64Binary", "hexBinary",
				"boolean",
				"anyURI",
				"duration", "dateTime", "date", "time",
//				"QName",
				"gYearMonth", "gMonthDay",
				"gYear", "gMonth", "gDay",
			};

			StringBuilder sb = new StringBuilder ();
			sb.Append ("<xs:schema xmlns:xs='" + XSchema.Namespace + "'>");
			foreach (string name in names)
				sb.Append ("<xs:element name='" + name + "' type='xs:" + name + "'/>");
			sb.Append ("</xs:schema>");
			XSchema schema = XSchema.Read (new XmlTextReader (sb.ToString (), XmlNodeType.Document, null), null);
			schema.Compile (null);
			foreach (XmlSchemaElement el in schema.Elements.Values)
				table.Add (el.Name, new XsdPrimitiveType (el.Name, el.ElementType as XmlSchemaDatatype));
		}

		public static XsdDatatypeProvider Instance {
			get { return instance; }
		}

		public override RelaxngDatatype GetDatatype (string name, string ns, RelaxngParamList parameters)
		{
			RelaxngDatatype dt = GetPrimitiveType (name, ns);
			if (dt == null)
				return null;
			else if (parameters == null || parameters.Count == 0)
				return dt;
			else
				return new XsdSimpleRestrictionType (dt, parameters);
		}

		private RelaxngDatatype GetPrimitiveType (string name, string ns)
		{
			switch (ns) {
			case System.Xml.Schema.XmlSchema.Namespace:
			case "http://www.w3.org/2001/XMLSchema-datatypes":
				break;
			default:
				return null;
			}
			if (name == "QName")
				return qnameType;
			return table [name] as RelaxngDatatype;
		}

	}

	public class XsdSimpleRestrictionType : RelaxngDatatype
	{
		XmlSchemaSimpleType type;
		XSchema schema;

		public XsdSimpleRestrictionType (RelaxngDatatype primitive, RelaxngParamList parameters)
		{
			type = new XmlSchemaSimpleType ();
			XmlSchemaSimpleTypeRestriction r =
				new XmlSchemaSimpleTypeRestriction ();
			type.Content = r;
			string ns = primitive.NamespaceURI;
			// Remap XML Schema datatypes namespace -> XML Schema namespace.
			if (ns == "http://www.w3.org/2001/XMLSchema-datatypes")
				ns = XSchema.Namespace;
			r.BaseTypeName = new XmlQualifiedName (primitive.Name, ns);
			foreach (RelaxngParam p in parameters) {
				XmlSchemaFacet f = null;
				string value = p.Value;
				switch (p.Name) {
				case "maxExclusive":
					f = new XmlSchemaMaxExclusiveFacet ();
					break;
				case "maxInclusive":
					f = new XmlSchemaMaxInclusiveFacet ();
					break;
				case "minExclusive":
					f = new XmlSchemaMinExclusiveFacet ();
					break;
				case "minInclusive":
					f = new XmlSchemaMinInclusiveFacet ();
					break;
				case "pattern":
					f = new XmlSchemaPatternFacet ();
					// .NET/Mono Regex has a bug that it does not support "IsLatin-1Supplement"
					// (it somehow breaks at '-').
					value = value.Replace ("\\p{IsLatin-1Supplement}", "[\\x80-\\xFF]");
					break;
				case "whiteSpace":
					f = new XmlSchemaWhiteSpaceFacet ();
					break;
				case "length":
					f = new XmlSchemaLengthFacet ();
					break;
				case "maxLength":
					f = new XmlSchemaMaxLengthFacet ();
					break;
				case "minLength":
					f = new XmlSchemaMinLengthFacet ();
					break;
				case "fractionDigits":
					f = new XmlSchemaFractionDigitsFacet ();
					break;
				case "totalDigits":
					f = new XmlSchemaTotalDigitsFacet ();
					break;
				default:
					throw new RelaxngException (String.Format ("XML Schema facet {0} is not recognized or not supported.", p.Name));
				}
				f.Value = value;
				r.Facets.Add (f);
			}

			// Now we create XmlSchema to handle simple-type
			// based validation (since there is no other way, 
			// because of sucky XmlSchemaSimpleType design).
			schema = new XSchema ();
			XmlSchemaElement el = new XmlSchemaElement ();
			el.Name = "root";
			el.SchemaType = type;
			schema.Items.Add (el);
			schema.Compile (null);
		}

		public override string Name {
			get { return type.QualifiedName.Name; }
		}

		public override string NamespaceURI {
			get { return type.QualifiedName.Namespace; }
		}

		internal override bool IsContextDependent {
			get { return type.Datatype != null && type.Datatype.TokenizedType == XmlTokenizedType.QName; }
		}

		public override object Parse (string value, XmlReader reader)
		{
			// Now we create XmlValidatingReader to handle
			// simple-type based validation (since there is no
			// other way, because of sucky XmlSchemaSimpleType
			// design).
			if (value != null)
				value = value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
			XmlValidatingReader v = new XmlValidatingReader (
				new XmlTextReader (
					String.Concat ("<root>", value, "</root>"),
					XmlNodeType.Document,
					null));
			v.Schemas.Add (schema);
			v.Read (); // <root>
			try {
				return v.ReadTypedValue ();
			} finally {
				v.Read (); // </root>
			}
		}
	}

	public class XsdPrimitiveType : RelaxngDatatype
	{
		XmlSchemaDatatype dt;
		string name;

		public XsdPrimitiveType (string name, XmlSchemaDatatype xstype)
		{
			this.name = name;
			dt = xstype;
		}

		internal override bool IsContextDependent {
			get { return dt.TokenizedType == XmlTokenizedType.QName; }
		}

		public override string Name {
			get { return name; }
		}

		public override string NamespaceURI {
			get { return "http://www.w3.org/2001/XMLSchema-datatypes"; }
		}

		public override object Parse (string text, XmlReader reader) 
		{
			return dt.ParseValue (text,
				reader != null ? reader.NameTable : null,
				null);
		}
	}

	// since QName resolution will fail, it must be implemented differently.
	public class XsdQNameWrapper : RelaxngDatatype
	{
		public XsdQNameWrapper ()
		{
		}

		public override string Name {
			get { return "QName"; }
		}

		public override string NamespaceURI {
			get { return "http://www.w3.org/2001/XMLSchema-datatypes"; }
		}

		internal override bool IsContextDependent {
			get { return true; }
		}

		public override object Parse (string s, XmlReader reader) 
		{
			int colonAt = s.IndexOf (':');
			string localName = colonAt < 0 ? s : s.Substring (colonAt + 1);
//			string localName = nameTable.Add (colonAt < 0 ? s : s.Substring (colonAt + 1));
			return new XmlQualifiedName (localName, reader.LookupNamespace (
				colonAt < 0 ? "" : s.Substring (0, colonAt - 1)));
		}

	}
}
