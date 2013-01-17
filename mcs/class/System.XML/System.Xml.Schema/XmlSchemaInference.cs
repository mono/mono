//
// XmlSchemaInference.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C)2004 Novell Inc.
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

#if NET_2_0

using System;
using System.Collections;
using System.Xml;
using System.Xml.Schema;

using QName = System.Xml.XmlQualifiedName;
using Form = System.Xml.Schema.XmlSchemaForm;
using Use = System.Xml.Schema.XmlSchemaUse;
using SOMList = System.Xml.Schema.XmlSchemaObjectCollection;
using SOMObject = System.Xml.Schema.XmlSchemaObject;
using Import = System.Xml.Schema.XmlSchemaImport;
using Element = System.Xml.Schema.XmlSchemaElement;
using Attr = System.Xml.Schema.XmlSchemaAttribute;
using AttrGroup = System.Xml.Schema.XmlSchemaAttributeGroup;
using AttrGroupRef = System.Xml.Schema.XmlSchemaAttributeGroupRef;
using SimpleType = System.Xml.Schema.XmlSchemaSimpleType;
using ComplexType = System.Xml.Schema.XmlSchemaComplexType;
using SimpleModel = System.Xml.Schema.XmlSchemaSimpleContent;
using SimpleExt = System.Xml.Schema.XmlSchemaSimpleContentExtension;
using SimpleRst = System.Xml.Schema.XmlSchemaSimpleContentRestriction;
using ComplexModel = System.Xml.Schema.XmlSchemaComplexContent;
using ComplexExt = System.Xml.Schema.XmlSchemaComplexContentExtension;
using ComplexRst = System.Xml.Schema.XmlSchemaComplexContentRestriction;
using SimpleTypeRst = System.Xml.Schema.XmlSchemaSimpleTypeRestriction;
using SimpleList = System.Xml.Schema.XmlSchemaSimpleTypeList;
using SimpleUnion = System.Xml.Schema.XmlSchemaSimpleTypeUnion;
using SchemaFacet = System.Xml.Schema.XmlSchemaFacet;
using LengthFacet = System.Xml.Schema.XmlSchemaLengthFacet;
using MinLengthFacet = System.Xml.Schema.XmlSchemaMinLengthFacet;
using Particle = System.Xml.Schema.XmlSchemaParticle;
using Sequence = System.Xml.Schema.XmlSchemaSequence;
using Choice = System.Xml.Schema.XmlSchemaChoice;


namespace System.Xml.Schema
{
	[MonoTODO]
	// FIXME:
	// - merge primitive types
	// - infer gYearMonth too
	// - in some cases sequence should contain element whose minOccurs=0
	//    (no obvious rules right now)
	// - reject some non-supported schema components
	public sealed class XmlSchemaInference
	{
		public enum InferenceOption {
			Restricted,
			Relaxed,
		}

		InferenceOption occurrence = InferenceOption.Restricted;
		InferenceOption typeInference = InferenceOption.Restricted;

		public XmlSchemaInference ()
		{
		}

		public InferenceOption Occurrence {
			get { return occurrence; }
			set { occurrence = value; }
		}

		public InferenceOption TypeInference {
			get { return typeInference; }
			set { typeInference = value; }
		}

		public XmlSchemaSet InferSchema (XmlReader instanceDocument)
		{
			return InferSchema (instanceDocument, new XmlSchemaSet ());
		}

		public XmlSchemaSet InferSchema (XmlReader instanceDocument,
			XmlSchemaSet schemas)
		{
			return XsdInference.Process (instanceDocument, schemas,
				occurrence == InferenceOption.Relaxed,
				typeInference == InferenceOption.Relaxed);
		}
	}

	class XsdInference
	{
		public static XmlSchemaSet Process (XmlReader xmlReader, 
			XmlSchemaSet schemas,
			bool laxOccurrence,
			bool laxTypeInference)
		{
			XsdInference impl = new XsdInference (xmlReader,
				schemas, laxOccurrence, laxTypeInference);
			impl.Run ();
			return impl.schemas;
		}

		public const string NamespaceXml =
			"http://www.w3.org/XML/1998/namespace";

		public const string NamespaceXmlns =
			"http://www.w3.org/2000/xmlns/";

		public const string XdtNamespace =
			"http://www.w3.org/2003/11/xpath-datatypes";

		static readonly QName QNameString = new QName (
			"string", XmlSchema.Namespace);

		static readonly QName QNameBoolean = new QName (
			"boolean", XmlSchema.Namespace);

		static readonly QName QNameAnyType = new QName (
			"anyType", XmlSchema.Namespace);

		static readonly QName QNameByte = new QName (
			"byte", XmlSchema.Namespace);

		static readonly QName QNameUByte = new QName (
			"unsignedByte", XmlSchema.Namespace);

		static readonly QName QNameShort = new QName (
			"short", XmlSchema.Namespace);

		static readonly QName QNameUShort = new QName (
			"unsignedShort", XmlSchema.Namespace);

		static readonly QName QNameInt = new QName (
			"int", XmlSchema.Namespace);

		static readonly QName QNameUInt = new QName (
			"unsignedInt", XmlSchema.Namespace);

		static readonly QName QNameLong = new QName (
			"long", XmlSchema.Namespace);

		static readonly QName QNameULong = new QName (
			"unsignedLong", XmlSchema.Namespace);

		static readonly QName QNameDecimal = new QName (
			"decimal", XmlSchema.Namespace);

		static readonly QName QNameDouble = new QName (
			"double", XmlSchema.Namespace);

		static readonly QName QNameFloat = new QName (
			"float", XmlSchema.Namespace);

		static readonly QName QNameDateTime = new QName (
			"dateTime", XmlSchema.Namespace);

		static readonly QName QNameDuration = new QName (
			"duration", XmlSchema.Namespace);

		XmlReader source;
		XmlSchemaSet schemas;
		bool laxOccurrence;
		bool laxTypeInference;

		Hashtable newElements = new Hashtable ();
		Hashtable newAttributes = new Hashtable ();

		private XsdInference (XmlReader xmlReader, 
			XmlSchemaSet schemas, 
			bool laxOccurrence, 
			bool laxTypeInference)
		{
			this.source = xmlReader;
			this.schemas = schemas;
			this.laxOccurrence = laxOccurrence;
			this.laxTypeInference = laxTypeInference;
		}

		private void Run ()
		{
			// XmlSchemaSet need to be compiled.
			schemas.Compile ();

			// move to top-level element
			source.MoveToContent ();
			if (source.NodeType != XmlNodeType.Element)
				throw new ArgumentException ("Argument XmlReader content is expected to be an element.");

			QName qname = new QName (source.LocalName,
				source.NamespaceURI);
			Element el = GetGlobalElement (qname);
			if (el == null) {
				el = CreateGlobalElement (qname);
				InferElement (el, qname.Namespace, true);
			}
			else
				InferElement (el, qname.Namespace, false);

			// FIXME: compile again.
//			foreach (XmlSchema schema in schemas.Schemas ())
//				schemas.Reprocess (schema);
		}

		private void AddImport (string current, string import)
		{
			foreach (XmlSchema schema in schemas.Schemas (current)) {
				bool exists = false;
				foreach (XmlSchemaExternal e in schema.Includes) {
					Import imp = e as Import;
					if (imp != null &&
						imp.Namespace == import)
						exists = true;
				}
				if (exists)
					continue;
				Import newimp = new Import ();
				newimp.Namespace = import;
				schema.Includes.Add (newimp);
			}
		}

		private void IncludeXmlAttributes ()
		{
			if (schemas.Schemas (NamespaceXml).Count == 0)
				// FIXME: do it from resources.
				schemas.Add (NamespaceXml, 
					"http://www.w3.org/2001/xml.xsd");
		}

		private void InferElement (Element el, string ns, bool isNew)
		{
			// Quick check for reference to another definition
			// (i.e. element ref='...' that should be redirected)
			if (el.RefName != QName.Empty) {
				Element body = GetGlobalElement (el.RefName);
				if (body == null) {
					body = CreateElement (el.RefName);
					InferElement (body, ns, true);
				}
				else
					InferElement (body, ns, isNew);
				return;
			}

			// Attributes
			if (source.MoveToFirstAttribute ()) {
				InferAttributes (el, ns, isNew);
				source.MoveToElement ();
			}

			// Content
			if (source.IsEmptyElement) {
				InferAsEmptyElement (el, ns, isNew);
				source.Read ();
				source.MoveToContent ();
			}
			else {
				InferContent (el, ns, isNew);
				source.ReadEndElement ();
			}
			if (el.SchemaType == null &&
				el.SchemaTypeName == QName.Empty)
				el.SchemaTypeName = QNameString;
		}

		#region Attribute Inference

		private Hashtable CollectAttrTable (SOMList attList)
		{
			// get attribute definition table.
			Hashtable table = new Hashtable ();
			foreach (XmlSchemaObject obj in attList) {
				Attr attr = obj as Attr;
				if (attr == null)
					throw Error (obj, String.Format ("Attribute inference only supports direct attribute definition. {0} is not supported.", obj.GetType ()));
				if (attr.RefName != QName.Empty)
					table.Add (attr.RefName, attr);
				else
					table.Add (new QName (attr.Name, ""),
						attr);
			}
			return table;
		}

		private void InferAttributes (Element el, string ns, bool isNew)
		{
			// Now this element is going to have complexType.
			// It currently not, then we have to replace it.
			ComplexType ct = null;
			SOMList attList = null;
			Hashtable table = null;

			do {
				switch (source.NamespaceURI) {
				case NamespaceXml:
					if (schemas.Schemas (
						NamespaceXml) .Count == 0)
						IncludeXmlAttributes ();
					break;
				case XmlSchema.InstanceNamespace:
					if (source.LocalName == "nil")
						el.IsNillable = true;
					// all other xsi:* atts are ignored
					continue;
				case NamespaceXmlns:
					continue;
				}
				if (ct == null) {
					ct = ToComplexType (el);
					attList = GetAttributes (ct);
					table = CollectAttrTable (attList);
				}
				QName attrName = new QName (
					source.LocalName, source.NamespaceURI);
				Attr attr = table [attrName] as Attr;
				if (attr == null) {
					attList.Add (InferNewAttribute (
						attrName, isNew, ns));
				} else {
					table.Remove (attrName);
					if (attr.RefName != null &&
						attr.RefName != QName.Empty)
						continue; // just a reference
					InferMergedAttribute (attr);
				}
			} while (source.MoveToNextAttribute ());

			// mark all attr definitions that did not appear
			// as optional.
			if (table != null)
				foreach (Attr attr in table.Values)
					attr.Use = Use.Optional;
		}

		private XmlSchemaAttribute InferNewAttribute (
			QName attrName, bool isNewTypeDefinition, string ns)
		{
			Attr attr = null;
			bool mergedRequired = false;
			if (attrName.Namespace.Length > 0) {
				// global attribute; might be already defined.
				attr = GetGlobalAttribute (attrName) as Attr;
				if (attr == null) {
					attr = CreateGlobalAttribute (attrName);
					attr.SchemaTypeName =
						InferSimpleType (source.Value);
				} else {
					InferMergedAttribute (attr);
					mergedRequired =
						attr.Use == Use.Required;
				}
				attr = new Attr ();
				attr.RefName = attrName;
				AddImport (ns, attrName.Namespace);
			} else {
				// local attribute
				attr = new Attr ();
				attr.Name = attrName.Name;
				attr.SchemaTypeName =
					InferSimpleType (source.Value);
			}
			if (!laxOccurrence &&
				(isNewTypeDefinition || mergedRequired))
				attr.Use = Use.Required;
			else
				attr.Use = Use.Optional;

			return attr;
		}

		// validate string value agains attr and 
		// if invalid, then relax the type.
		private void InferMergedAttribute (Attr attr)
		{
			attr.SchemaTypeName = InferMergedType (source.Value,
				attr.SchemaTypeName);
			attr.SchemaType = null;
		}

		private QName InferMergedType (string value, QName typeName)
		{
			// examine value against specified type and
			// if unacceptable, then return a relaxed type.

			SimpleType st = XmlSchemaType.GetBuiltInSimpleType (
				typeName);
			if (st == null) // non-primitive type => see above.
				return QNameString;
			do {
				try {
					st.Datatype.ParseValue (value,
						source.NameTable,
						source as IXmlNamespaceResolver);
					return typeName;
				} catch {
					st = st.BaseXmlSchemaType as XmlSchemaSimpleType;
					typeName = st != null ? st.QualifiedName : QName.Empty;
				}
			} while (typeName != QName.Empty);
			return QNameString;
		}

		private SOMList GetAttributes (ComplexType ct)
		{
			if (ct.ContentModel == null)
				return ct.Attributes;

			SimpleModel sc = ct.ContentModel as SimpleModel;
			if (sc != null) {
				SimpleExt sce = sc.Content as SimpleExt;
				if (sce != null)
					return sce.Attributes;
				SimpleRst scr = sc.Content as SimpleRst;
				if (scr != null)
					return scr.Attributes;
				else
					throw Error (sc, "Invalid simple content model.");
			}
			ComplexModel cc = ct.ContentModel as ComplexModel;
			if (cc != null) {
				ComplexExt cce = cc.Content as ComplexExt;
				if (cce != null)
					return cce.Attributes;
				ComplexRst ccr = cc.Content as ComplexRst;
				if (ccr != null)
					return ccr.Attributes;
				else
					throw Error (cc, "Invalid simple content model.");
			}
			throw Error (cc, "Invalid complexType. Should not happen.");
		}

		private ComplexType ToComplexType (Element el)
		{
			QName name = el.SchemaTypeName;
			XmlSchemaType type = el.SchemaType;

			// 1. element type is complex.
			ComplexType ct = type as ComplexType;
			if (ct != null)
				return ct;

			// 2. reference to global complexType.
			XmlSchemaType globalType = schemas.GlobalTypes [name]
				as XmlSchemaType;
			ct = globalType as ComplexType;
			if (ct != null)
				return ct;

			ct = new ComplexType ();
			el.SchemaType = ct;
			el.SchemaTypeName = QName.Empty;

			// 3. base type name is xs:anyType or no specification.
			// <xs:complexType />
			if (name == QNameAnyType)
				return ct;
			else if (type == null && name == QName.Empty)
				return ct;

			SimpleModel sc = new SimpleModel ();
			ct.ContentModel = sc;

			// 4. type is simpleType
			//    -> extension of existing simple type.
			SimpleType st = type as SimpleType;
			if (st != null) {
				SimpleRst scr = new SimpleRst ();
				scr.BaseType = st;
				sc.Content = scr;
				return ct;
			}

			SimpleExt sce = new SimpleExt ();
			sc.Content = sce;

			// 5. type name points to primitive type
			//    -> simple extension of a primitive type
			st = XmlSchemaType.GetBuiltInSimpleType (name);
			if (st != null) {
				sce.BaseTypeName = name;
				return ct;
			}

			// 6. type name points to global simpleType.
			st = globalType as SimpleType;
			if (st != null) {
				sce.BaseTypeName = name;
				return ct;
			}

			throw Error (el, "Unexpected schema component that contains simpleTypeName that could not be resolved.");
		}

		#endregion

		#region Element Type

		private void InferAsEmptyElement (Element el, string ns,
			bool isNew)
		{
			ComplexType ct = el.SchemaType as ComplexType;
			if (ct != null) {
				SimpleModel sm =
					ct.ContentModel as SimpleModel;
				if (sm != null) {
					ToEmptiableSimpleContent (sm, isNew);
					return;
				}

				ComplexModel cm = ct.ContentModel
					as ComplexModel;
				if (cm != null) {
					ToEmptiableComplexContent (cm, isNew);
					return;
				}

				if (ct.Particle != null)
					ct.Particle.MinOccurs = 0;
				return;
			}
			SimpleType st = el.SchemaType as SimpleType;
			if (st != null) {
				st = MakeBaseTypeAsEmptiable (st);
				switch (st.QualifiedName.Namespace) {
				case XmlSchema.Namespace:
				case XdtNamespace:
					el.SchemaTypeName = st.QualifiedName;
					break;
				default:
					el.SchemaType =st;
					break;
				}
			}
		}

		private SimpleType MakeBaseTypeAsEmptiable (SimpleType st)
		{
			switch (st.QualifiedName.Namespace) {
			case XmlSchema.Namespace:
			case XdtNamespace:
				// If a primitive type
				return XmlSchemaType.GetBuiltInSimpleType (
					XmlTypeCode.String);
			}
			SimpleTypeRst str = st.Content as SimpleTypeRst;
			if (str != null) {
				ArrayList al = null;
				foreach (SchemaFacet f in str.Facets) {
					if (f is LengthFacet ||
						f is MinLengthFacet) {
						if (al == null)
							al = new ArrayList ();
						al.Add (f);
					}
				}
				foreach (SchemaFacet f in al)
					str.Facets.Remove (f);
				if (str.BaseType != null)
					str.BaseType =
						MakeBaseTypeAsEmptiable (st);
				else
					// It might have a reference to an
					// external simple type, but there is
					// no assurance that any of those
					// external types allow an empty
					// string. So just set base type as
					// xs:string.
					str.BaseTypeName = QNameString;
			} // union/list can have empty string value.

			return st;
		}

		private void ToEmptiableSimpleContent (
			SimpleModel sm, bool isNew)
		{
			SimpleExt se = sm.Content as SimpleExt;
			if (se != null)
				se.BaseTypeName = QNameString;
			else {
				SimpleRst sr = sm.Content
					as SimpleRst;
				if (sr == null)
					throw Error (sm, "Invalid simple content model was passed.");
				sr.BaseTypeName = QNameString;
				sr.BaseType = null;
			}
		}

		private void ToEmptiableComplexContent (
			ComplexModel cm, bool isNew)
		{
			ComplexExt ce = cm.Content
				as ComplexExt;
			if (ce != null) {
				if (ce.Particle != null)
					ce.Particle.MinOccurs = 0;
				else if (ce.BaseTypeName != null &&
					ce.BaseTypeName != QName.Empty &&
					ce.BaseTypeName != QNameAnyType)
					throw Error (ce, "Complex type content extension has a reference to an external component that is not supported.");
			}
			else {
				ComplexRst cr = cm.Content
					as ComplexRst;
				if (cr == null)
					throw Error (cm, "Invalid complex content model was passed.");
				if (cr.Particle != null)
					cr.Particle.MinOccurs = 0;
				else if (cr.BaseTypeName != null &&
					cr.BaseTypeName != QName.Empty &&
					cr.BaseTypeName != QNameAnyType)
					throw Error (cr, "Complex type content extension has a reference to an external component that is not supported.");
			}
		}

		private void InferContent (Element el, string ns, bool isNew)
		{
			source.Read ();
			source.MoveToContent ();
			switch (source.NodeType) {
			case XmlNodeType.EndElement:
				InferAsEmptyElement (el, ns, isNew);
				break;
			case XmlNodeType.Element:
				InferComplexContent (el, ns, isNew);
				break;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
				InferTextContent (el, isNew);
				source.MoveToContent ();
				if (source.NodeType == XmlNodeType.Element)
					goto case XmlNodeType.Element;
				break;
			case XmlNodeType.Whitespace:
				InferContent (el, ns, isNew); // skip and retry
				break;
			}
		}

		private void InferComplexContent (Element el, string ns,
			bool isNew)
		{
			ComplexType ct = ToComplexType (el);
			ToComplexContentType (ct);

			int position = 0;
			bool consumed = false;

			do {
				switch (source.NodeType) {
				case XmlNodeType.Element:
					Sequence s = PopulateSequence (ct);
					Choice c = s.Items.Count > 0 ?
						s.Items [0] as Choice :
						null;
					if (c != null)
						ProcessLax (c, ns);
					else
						ProcessSequence (ct, s, ns,
							ref position,
							ref consumed,
							isNew);
					source.MoveToContent ();
					break;
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
					MarkAsMixed (ct);
					source.ReadString ();
					source.MoveToContent ();
					break;
				case XmlNodeType.EndElement:
					return; // finished
				case XmlNodeType.None:
					throw new NotImplementedException ("Internal Error: Should not happen.");
				}
			} while (true);
		}

		private void InferTextContent (Element el, bool isNew)
		{
			string value = source.ReadString ();
			if (el.SchemaType == null) {
				if (el.SchemaTypeName == QName.Empty) {
					// no type information -> infer type
					if (isNew)
						el.SchemaTypeName =
							InferSimpleType (
							value);
					else
						el.SchemaTypeName =
							QNameString;
					return;
				}
				switch (el.SchemaTypeName.Namespace) {
				case XmlSchema.Namespace:
				case XdtNamespace:
					// existing primitive type
					el.SchemaTypeName = InferMergedType (
						value, el.SchemaTypeName);
					break;
				default:
					ComplexType ct = schemas.GlobalTypes [
						el.SchemaTypeName]
						as ComplexType;
					// If it is complex, then just set
					// mixed='true' (type cannot be set.)
					// If it is simple, then we cannot
					// make sure that string value is
					// valid. So just set as xs:string.
					if (ct != null)
						MarkAsMixed (ct);
					else
						el.SchemaTypeName = QNameString;
					break;
				}
				return;
			}
			// simpleType
			SimpleType st = el.SchemaType as SimpleType;
			if (st != null) {
				// If simple, then (described above)
				el.SchemaType = null;
				el.SchemaTypeName = QNameString;
				return;
			}

			// complexType
			ComplexType ect = el.SchemaType as ComplexType;

			SimpleModel sm = ect.ContentModel as SimpleModel;
			if (sm == null) {
				// - ComplexContent
				MarkAsMixed (ect);
				return;
			}

			// - SimpleContent
			SimpleExt se = sm.Content as SimpleExt;
			if (se != null)
				se.BaseTypeName = InferMergedType (value,
					se.BaseTypeName);
			SimpleRst sr = sm.Content as SimpleRst;
			if (sr != null) {
				sr.BaseTypeName = InferMergedType (value,
					sr.BaseTypeName);
				sr.BaseType = null;
			}
		}

		private void MarkAsMixed (ComplexType ct)
		{
			ComplexModel cm = ct.ContentModel as ComplexModel;
			if (cm != null)
				cm.IsMixed = true;
			else
				ct.IsMixed = true;
		}

		#endregion

		#region Particles

		private void ProcessLax (Choice c, string ns)
		{
			foreach (Particle p in c.Items) {
				Element el = p as Element;
				if (el == null)
					throw Error (c, String.Format ("Target schema item contains unacceptable particle {0}. Only element is allowed here."));
				if (ElementMatches (el, ns)) {
					InferElement (el, ns, false);
					return;
				}
			}
			// append a new element particle to lax term.
			Element nel = new Element ();
			if (source.NamespaceURI == ns)
				nel.Name = source.LocalName;
			else {
				nel.RefName = new QName (source.LocalName,
					source.NamespaceURI);
				AddImport (ns, source.NamespaceURI);
			}
			InferElement (nel, source.NamespaceURI, true);
			c.Items.Add (nel);
		}

		private bool ElementMatches (Element el, string ns)
		{
			bool matches = false;
			if (el.RefName != QName.Empty) {
				if (el.RefName.Name == source.LocalName &&
					el.RefName.Namespace ==
					source.NamespaceURI)
					matches = true;
			}
			else if (el.Name == source.LocalName &&
				ns == source.NamespaceURI)
					matches = true;
			return matches;
		}

		private void ProcessSequence (ComplexType ct, Sequence s,
			string ns, ref int position, ref bool consumed,
			bool isNew)
		{
			for (int i = 0; i < position; i++) {
				Element iel = s.Items [i] as Element;
				if (ElementMatches (iel, ns)) {
					// Sequence element type violation
					// might happen (might not, but we
					// cannot backtrack here). So switch
					// to sequence of choice* here.
					ProcessLax (ToSequenceOfChoice (s), ns);
					return;
				}
			}

			if (s.Items.Count <= position) {
				QName name = new QName (source.LocalName,
					source.NamespaceURI);
				Element nel = CreateElement (name);
				if (laxOccurrence)
					nel.MinOccurs = 0;
				InferElement (nel, ns, true);
				if (ns == name.Namespace)
					s.Items.Add (nel);
				else {
					Element re = new Element ();
					if (laxOccurrence)
						re.MinOccurs = 0;
					re.RefName = name;
					AddImport (ns, name.Namespace);
					s.Items.Add (re);
				}
				consumed = true;
				return;
			}
			Element el = s.Items [position] as Element;
			if (el == null)
				throw Error (s, String.Format ("Target complex type content sequence has an unacceptable type of particle {0}", s.Items [position]));
			bool matches = ElementMatches (el, ns);
			if (matches) {
				if (consumed)
					el.MaxOccursString = "unbounded";
				InferElement (el, source.NamespaceURI, false);
				source.MoveToContent ();
				switch (source.NodeType) {
				case XmlNodeType.None:
					if (source.NodeType ==
						XmlNodeType.Element)
						goto case XmlNodeType.Element;
					else if (source.NodeType ==
						XmlNodeType.EndElement)
						goto case XmlNodeType.EndElement;
					break;
				case XmlNodeType.Element:
					ProcessSequence (ct, s, ns, ref position,
						ref consumed, isNew);
					break;
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
					MarkAsMixed (ct);
					source.ReadString ();
					goto case XmlNodeType.None;
				case XmlNodeType.Whitespace:
					source.ReadString ();
					goto case XmlNodeType.None;
				case XmlNodeType.EndElement:
					return;
				default:
					source.Read ();
					break;
				}
			}
			else {
				if (consumed) {
					position++;
					consumed = false;
					ProcessSequence (ct, s, ns,
						ref position, ref consumed,
						isNew);
				}
				else
					ProcessLax (ToSequenceOfChoice (s), ns);
			}
		}

		// Note that it does not return the changed sequence.
		private Choice ToSequenceOfChoice (Sequence s)
		{
			Choice c = new Choice ();
			if (laxOccurrence)
				c.MinOccurs = 0;
			c.MaxOccursString = "unbounded";
			foreach (Particle p in s.Items)
				c.Items.Add (p);
			s.Items.Clear ();
			s.Items.Add (c);
			return c;
		}

		// It makes complexType not to have Simple content model.
		private void ToComplexContentType (ComplexType type)
		{
			SimpleModel sm = type.ContentModel as SimpleModel;
			if (sm == null)
				return;

			SOMList atts = GetAttributes (type);
			foreach (SOMObject o in atts)
				type.Attributes.Add (o);
			// FIXME: need to copy AnyAttribute.
			// (though not considered right now)
			type.ContentModel = null;
			type.IsMixed = true;
		}

		private Sequence PopulateSequence (ComplexType ct)
		{
			Particle p = PopulateParticle (ct);
			Sequence s = p as Sequence;
			if (s != null)
				return s;
			else
				throw Error (ct, String.Format ("Target complexType contains unacceptable type of particle {0}", p));
		}

		private Sequence CreateSequence ()
		{
			Sequence s = new Sequence ();
			if (laxOccurrence)
				s.MinOccurs = 0;
			return s;
		}

		private Particle PopulateParticle (ComplexType ct)
		{
			if (ct.ContentModel == null) {
				if (ct.Particle == null)
					ct.Particle = CreateSequence ();
				return ct.Particle;
			}
			ComplexModel cm = ct.ContentModel as ComplexModel;
			if (cm != null) {
				ComplexExt  ce = cm.Content as ComplexExt;
				if (ce != null) {
					if (ce.Particle == null)
						ce.Particle = CreateSequence ();
					return ce.Particle;
				}
				ComplexRst cr = cm.Content as ComplexRst;
				if (cr != null) {
					if (cr.Particle == null)
						cr.Particle = CreateSequence ();
					return cr.Particle;
				}
			}
			throw Error (ct, "Schema inference internal error. The complexType should have been converted to have a complex content.");
		}

		#endregion

		#region String Value

		// primitive type inference.
		// When running lax type inference, it just returns xs:string.
		private QName InferSimpleType (string value)
		{
			if (laxTypeInference)
				return QNameString;

			switch (value) {
			// 0 and 1 are not inferred as byte unlike MS.XSDInfer
//			case "0":
//			case "1":
			case "true":
			case "false":
				return QNameBoolean;
			}
			try {
				long dec = XmlConvert.ToInt64 (value);
				if (byte.MinValue <= dec && dec <= byte.MaxValue)
					return QNameUByte;
				if (sbyte.MinValue <= dec && dec <= sbyte.MaxValue)
					return QNameByte;
				if (ushort.MinValue <= dec && dec <= ushort.MaxValue)
					return QNameUShort;
				if (short.MinValue <= dec && dec <= short.MaxValue)
					return QNameShort;
				if (uint.MinValue <= dec && dec <= uint.MaxValue)
					return QNameUInt;
				if (int.MinValue <= dec && dec <= int.MaxValue)
					return QNameInt;
				return QNameLong;
			} catch (Exception) {
			}
			try {
				XmlConvert.ToUInt64 (value);
				return QNameULong;
			} catch (Exception) {
			}
			try {
				XmlConvert.ToDecimal (value);
				return QNameDecimal;
			} catch (Exception) {
			}
			try {
				double dbl = XmlConvert.ToDouble (value);
				if (float.MinValue <= dbl &&
					dbl <= float.MaxValue)
					return QNameFloat;
				else
					return QNameDouble;
			} catch (Exception) {
			}
			try {
				// FIXME: also try DateTimeSerializationMode
				// and gYearMonth
				XmlConvert.ToDateTime (value);
				return QNameDateTime;
			} catch (Exception) {
			}
			try {
				XmlConvert.ToTimeSpan (value);
				return QNameDuration;
			} catch (Exception) {
			}

			// xs:string
			return QNameString;
		}

		#endregion

		#region Utilities

		private Element GetGlobalElement (QName name)
		{
			Element el = newElements [name] as Element;
			if (el == null)
				el = schemas.GlobalElements [name] as Element;
			return el;
		}

		private Attr GetGlobalAttribute (QName name)
		{
			Attr a = newElements [name] as Attr;
			if (a == null)
				a = schemas.GlobalAttributes [name] as Attr;
			return a;
		}

		private Element CreateElement (QName name)
		{
			Element el = new Element ();
			el.Name = name.Name;
			return el;
		}

		private Element CreateGlobalElement (QName name)
		{
			Element el = CreateElement (name);
			XmlSchema schema = PopulateSchema (name.Namespace);
			schema.Items.Add (el);
			newElements.Add (name, el);
			return el;
		}

		private Attr CreateGlobalAttribute (QName name)
		{
			Attr attr = new Attr ();
			XmlSchema schema = PopulateSchema (name.Namespace);
			attr.Name = name.Name;
			schema.Items.Add (attr);
			newAttributes.Add (name, attr);
			return attr;
		}

		// Note that the return value never assures that all the
		// components in the parameter ns must reside in it.
		private XmlSchema PopulateSchema (string ns)
		{
			ICollection list = schemas.Schemas (ns);
			if (list.Count > 0) {
				IEnumerator e = list.GetEnumerator ();
				e.MoveNext ();
				return (XmlSchema) e.Current;
			}
			XmlSchema s = new XmlSchema ();
			if (ns != null && ns.Length > 0)
				s.TargetNamespace = ns;
			s.ElementFormDefault = Form.Qualified;
			s.AttributeFormDefault = Form.Unqualified;
			schemas.Add (s);
			return s;
		}

		private XmlSchemaInferenceException Error (
			XmlSchemaObject sourceObj,
			string message)
		{
			// This override is mainly for schema component error.
			return Error (sourceObj, false, message);
		}

		private XmlSchemaInferenceException Error (
			XmlSchemaObject sourceObj,
			bool useReader,
			string message)
		{
			string msg = String.Concat (
				message,
				sourceObj != null ?
					String.Format (". Related schema component is {0}",
						sourceObj.SourceUri,
						sourceObj.LineNumber,
						sourceObj.LinePosition) :
					String.Empty,
				useReader ?
					String.Format (". {0}", source.BaseURI) :
					String.Empty);

			IXmlLineInfo li = source as IXmlLineInfo;
			if (useReader && li != null)
				return new XmlSchemaInferenceException (
					msg, null, li.LineNumber,
					li.LinePosition);
			else
				return new XmlSchemaInferenceException (msg);
		}

		#endregion
	}
}

#endif
