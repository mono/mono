//
// XmlSchemaValidatingReader.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc,
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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
#if !NON_MONO
using Mono.Xml;
#endif

#if NET_2_0

using System.Collections.Generic;
using QName = System.Xml.XmlQualifiedName;
using Form = System.Xml.Schema.XmlSchemaForm;
using Use = System.Xml.Schema.XmlSchemaUse;
using ContentType = System.Xml.Schema.XmlSchemaContentType;
using Validity = System.Xml.Schema.XmlSchemaValidity;
using ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags;
using SOMList = System.Xml.Schema.XmlSchemaObjectCollection;
using SOMObject = System.Xml.Schema.XmlSchemaObject;
using XsElement = System.Xml.Schema.XmlSchemaElement;
using XsAttr = System.Xml.Schema.XmlSchemaAttribute;
using AttrGroup = System.Xml.Schema.XmlSchemaAttributeGroup;
using AttrGroupRef = System.Xml.Schema.XmlSchemaAttributeGroupRef;
using XsDatatype = System.Xml.Schema.XmlSchemaDatatype;
using SchemaType = System.Xml.Schema.XmlSchemaType;
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
using ValException = System.Xml.Schema.XmlSchemaValidationException;

namespace Mono.Xml.Schema
{
	internal class XmlSchemaValidatingReader : XmlReader, IXmlLineInfo,
		IXmlSchemaInfo, IXmlNamespaceResolver, IHasXmlParserContext
	{
		static readonly XsAttr [] emptyAttributeArray =
			new XsAttr [0];

		#region Instance Fields

		XmlReader reader;
		ValidationFlags options;
		XmlSchemaValidator v;
		XmlValueGetter getter;
		XmlSchemaInfo xsinfo;
		IXmlLineInfo readerLineInfo;
		ValidationType validationType;
		IXmlNamespaceResolver nsResolver;

		XsAttr [] defaultAttributes = emptyAttributeArray;
		int currentDefaultAttribute = -1;
		ArrayList defaultAttributesCache = new ArrayList ();
		bool defaultAttributeConsumed;
		XmlSchemaType currentAttrType;
		bool validationDone;

		// Extra for XmlSchemaValidtingReader
		// (not in XsdValidatingReader)
		XsElement element; // ... xsinfo.Element?

		#endregion

		public XmlSchemaValidatingReader (XmlReader reader,
			XmlReaderSettings settings)
		{
			IXmlNamespaceResolver nsResolver = reader as IXmlNamespaceResolver;
			if (nsResolver == null)
			//	throw new ArgumentException ("Argument XmlReader must implement IXmlNamespaceResolver.");
				nsResolver = new XmlNamespaceManager (reader.NameTable);

			XmlSchemaSet schemas = settings.Schemas;
			if (schemas == null)
				schemas = new XmlSchemaSet ();
			options = settings.ValidationFlags;

			this.reader = reader;
			v = new XmlSchemaValidator (
				reader.NameTable,
				schemas,
				nsResolver,
				options);
			if (reader.BaseURI != String.Empty)
				v.SourceUri = new Uri (reader.BaseURI);

			readerLineInfo = reader as IXmlLineInfo;
			getter = delegate () {
				if (v.CurrentAttributeType != null)
					return v.CurrentAttributeType.ParseValue (Value, NameTable, this);
				else
					return Value; 
				};
			xsinfo = new XmlSchemaInfo (); // transition cache
			v.LineInfoProvider = this;
			v.ValidationEventSender = reader;
			this.nsResolver = nsResolver;
#if !NON_MONO
			ValidationEventHandler += delegate (object o, ValidationEventArgs e) {
				settings.OnValidationError (o, e);
			};
			if (settings != null && settings.Schemas != null)
				v.XmlResolver = settings.Schemas.XmlResolver;
			else
				v.XmlResolver = new XmlUrlResolver ();
#else
			v.XmlResolver = new XmlUrlResolver ();
#endif
			v.Initialize ();
		}

		public event ValidationEventHandler ValidationEventHandler {
			add { v.ValidationEventHandler += value; }
			remove { v.ValidationEventHandler -= value; }
		}

		public XmlSchemaType ElementSchemaType {
			get {
				return element != null ? element.ElementSchemaType : null;
			}
		}

		// clear default attributes, MoveTo*Attribute() transitent
		// fields and so on.
		private void ResetStateOnRead ()
		{
			currentDefaultAttribute = -1;
			defaultAttributeConsumed = false;
			currentAttrType = null;
			defaultAttributes = emptyAttributeArray;
			v.CurrentAttributeType = null;
		}

		#region Properties

		public int LineNumber {
			get { return readerLineInfo != null ? readerLineInfo.LineNumber : 0; }
		}

		public int LinePosition {
			get { return readerLineInfo != null ? readerLineInfo.LinePosition : 0; }
		}

		public XmlSchemaType SchemaType {
			get {
				if (ReadState != ReadState.Interactive)
					return null;

				switch (NodeType) {
				case XmlNodeType.Element:
					if (ElementSchemaType != null)
						return ElementSchemaType;
					else
						return null;//SourceReaderSchemaType;
				case XmlNodeType.Attribute:
					if (currentAttrType == null) {
						ComplexType ct = ElementSchemaType as ComplexType;
						if (ct != null) {
							XsAttr attdef = ct.AttributeUses [new XmlQualifiedName (LocalName, NamespaceURI)] as XsAttr;
							if (attdef != null)
								currentAttrType = attdef.AttributeSchemaType;
							return currentAttrType;
						}
//						currentAttrType = SourceReaderSchemaType;
					}
					return currentAttrType;
				default:
					return null;//SourceReaderSchemaType;
				}
			}
		}

		public ValidationType ValidationType {
			get { return validationType; }
			set {
				if (ReadState != ReadState.Initial)
					throw new InvalidOperationException ("ValidationType must be set before reading.");
				validationType = value;
			}
		}

		public IDictionary<string, string> GetNamespacesInScope (XmlNamespaceScope scope)
		{
			IXmlNamespaceResolver resolver = reader as IXmlNamespaceResolver;
			if (resolver == null)
				throw new NotSupportedException ("The input XmlReader does not implement IXmlNamespaceResolver and thus this validating reader cannot collect in-scope namespaces.");
			return resolver.GetNamespacesInScope (scope);
		}

		public string LookupPrefix (string ns)
		{
			return nsResolver.LookupPrefix (ns);
		}

		// Public Overriden Properties

		public override int AttributeCount {
			get {
				return reader.AttributeCount + defaultAttributes.Length;
			}
		}

		public override string BaseURI {
			get { return reader.BaseURI; }
		}

		// If this class is used to implement XmlValidatingReader,
		// it should be left to DTDValidatingReader. In other cases,
		// it depends on the reader's ability.
		public override bool CanResolveEntity {
			get { return reader.CanResolveEntity; }
		}

		public override int Depth {
			get {
				if (currentDefaultAttribute < 0)
					return reader.Depth;
				if (this.defaultAttributeConsumed)
					return reader.Depth + 2;
				return reader.Depth + 1;
			}
		}

		public override bool EOF {
			get { return reader.EOF; }
		}

		public override bool HasValue {
			get {
				if (currentDefaultAttribute < 0)
					return reader.HasValue;
				return true;
			}
		}

		public override bool IsDefault {
			get {
				if (currentDefaultAttribute < 0)
					return reader.IsDefault;
				return true;
			}
		}

		public override bool IsEmptyElement {
			get {
				if (currentDefaultAttribute < 0)
					return reader.IsEmptyElement;
				return false;
			}
		}

		public override string this [int i] {
			get { return GetAttribute (i); }
		}

		public override string this [string name] {
			get { return GetAttribute (name); }
		}

		public override string this [string localName, string ns] {
			get { return GetAttribute (localName, ns); }
		}

		int IXmlLineInfo.LineNumber {
			get { return readerLineInfo != null ? readerLineInfo.LineNumber : 0; }
		}

		int IXmlLineInfo.LinePosition {
			get { return readerLineInfo != null ? readerLineInfo.LinePosition : 0; }
		}

		public override string LocalName {
			get {
				if (currentDefaultAttribute < 0)
					return reader.LocalName;
				if (defaultAttributeConsumed)
					return String.Empty;
				return defaultAttributes [currentDefaultAttribute].QualifiedName.Name;
			}
		}

		public override string Name {
			get {
				if (currentDefaultAttribute < 0)
					return reader.Name;
				if (defaultAttributeConsumed)
					return String.Empty;

				XmlQualifiedName qname = defaultAttributes [currentDefaultAttribute].QualifiedName;
				string prefix = Prefix;
				if (prefix == String.Empty)
					return qname.Name;
				else
					return String.Concat (prefix, ":", qname.Name);
			}
		}

		public override string NamespaceURI {
			get {
				if (currentDefaultAttribute < 0)
					return reader.NamespaceURI;
				if (defaultAttributeConsumed)
					return String.Empty;
				return defaultAttributes [currentDefaultAttribute].QualifiedName.Namespace;
			}
		}

		public override XmlNameTable NameTable {
			get { return reader.NameTable; }
		}

		public override XmlNodeType NodeType {
			get {
				if (currentDefaultAttribute < 0)
					return reader.NodeType;
				if (defaultAttributeConsumed)
					return XmlNodeType.Text;
				return XmlNodeType.Attribute;
			}
		}

		public XmlParserContext ParserContext {
			get { return XmlSchemaUtil.GetParserContext (reader); }
		}

		public override string Prefix {
			get {
				if (currentDefaultAttribute < 0)
					return reader.Prefix;
				if (defaultAttributeConsumed)
					return String.Empty;
				XmlQualifiedName qname = defaultAttributes [currentDefaultAttribute].QualifiedName;
				string prefix = nsResolver.LookupPrefix (qname.Namespace);
				if (prefix == null)
					return String.Empty;
				else
					return prefix;
			}
		}

		public override char QuoteChar {
			get { return reader.QuoteChar; }
		}

		public override ReadState ReadState {
			get { return reader.ReadState; }
		}

		public override IXmlSchemaInfo SchemaInfo {
			get {
				return new XmlSchemaInfo () {
					//ContentType = this.ContentType,
					IsDefault = this.IsDefault,
					IsNil = this.IsNil,
					MemberType = this.MemberType,
					SchemaAttribute = this.SchemaAttribute,
					SchemaElement = this.SchemaElement,
					SchemaType = this.SchemaType,
					Validity = this.Validity
					};
			}
		}

		public override string Value {
			get {
				if (currentDefaultAttribute < 0)
					return reader.Value;
				string value = defaultAttributes [currentDefaultAttribute].ValidatedDefaultValue;
				if (value == null)
					value = defaultAttributes [currentDefaultAttribute].ValidatedFixedValue;
				return value;
			}
		}

		public override string XmlLang {
			get {
				string xmlLang = reader.XmlLang;
				if (xmlLang != null)
					return xmlLang;
				int idx = this.FindDefaultAttribute ("lang", XmlNamespaceManager.XmlnsXml);
				if (idx < 0)
					return null;
				xmlLang = defaultAttributes [idx].ValidatedDefaultValue;
				if (xmlLang == null)
					xmlLang = defaultAttributes [idx].ValidatedFixedValue;
				return xmlLang;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				XmlSpace space = reader.XmlSpace;
				if (space != XmlSpace.None)
					return space;
				int idx = this.FindDefaultAttribute ("space", XmlNamespaceManager.XmlnsXml);
				if (idx < 0)
					return XmlSpace.None;
				string spaceSpec = defaultAttributes [idx].ValidatedDefaultValue;
				if (spaceSpec == null)
					spaceSpec = defaultAttributes [idx].ValidatedFixedValue;
				return (XmlSpace) Enum.Parse (typeof (XmlSpace), spaceSpec, false);
			}
		}
		#endregion

		#region Public Methods

		// Overrided Methods

		public override void Close ()
		{
			reader.Close ();
		}

		public override string GetAttribute (int i)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.GetAttribute (i);
			}

			if (reader.AttributeCount > i)
				reader.GetAttribute (i);
			int defIdx = i - reader.AttributeCount;
			if (i < AttributeCount)
				return defaultAttributes [defIdx].DefaultValue;

			throw new ArgumentOutOfRangeException ("i", i, "Specified attribute index is out of range.");
		}

		public override string GetAttribute (string name)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.GetAttribute (name);
			}

			string value = reader.GetAttribute (name);
			if (value != null)
				return value;

			XmlQualifiedName qname = SplitQName (name);
			return GetDefaultAttribute (qname.Name, qname.Namespace);
		}

		private XmlQualifiedName SplitQName (string name)
		{
			XmlConvert.VerifyName (name);

			Exception ex = null;
			XmlQualifiedName qname = XmlSchemaUtil.ToQName (reader, name, out ex);
			if (ex != null)
				return XmlQualifiedName.Empty;
			else
				return qname;
		}

		public override string GetAttribute (string localName, string ns)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.GetAttribute (localName, ns);
			}

			string value = reader.GetAttribute (localName, ns);
			if (value != null)
				return value;

			return GetDefaultAttribute (localName, ns);
		}

		private string GetDefaultAttribute (string localName, string ns)
		{
			int idx = this.FindDefaultAttribute (localName, ns);
			if (idx < 0)
				return null;
			string value = defaultAttributes [idx].ValidatedDefaultValue;
			if (value == null)
				value = defaultAttributes [idx].ValidatedFixedValue;
			return value;
		}

		private int FindDefaultAttribute (string localName, string ns)
		{
			for (int i = 0; i < this.defaultAttributes.Length; i++) {
				XsAttr attr = defaultAttributes [i];
				if (attr.QualifiedName.Name == localName &&
					(ns == null || attr.QualifiedName.Namespace == ns))
					return i;
			}
			return -1;
		}

		bool IXmlLineInfo.HasLineInfo ()
		{
			return readerLineInfo != null && readerLineInfo.HasLineInfo ();
		}

		public override string LookupNamespace (string prefix)
		{
			return reader.LookupNamespace (prefix);
		}

		public override void MoveToAttribute (int i)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				reader.MoveToAttribute (i);
				return;
			}

			currentAttrType = null;
			if (i < reader.AttributeCount) {
				reader.MoveToAttribute (i);
				this.currentDefaultAttribute = -1;
				this.defaultAttributeConsumed = false;
			}

			if (i < AttributeCount) {
				this.currentDefaultAttribute = i - reader.AttributeCount;
				this.defaultAttributeConsumed = false;
			}
			else
				throw new ArgumentOutOfRangeException ("i", i, "Attribute index is out of range.");
		}

		public override bool MoveToAttribute (string name)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.MoveToAttribute (name);
			}

			currentAttrType = null;
			bool b = reader.MoveToAttribute (name);
			if (b) {
				this.currentDefaultAttribute = -1;
				this.defaultAttributeConsumed = false;
				return true;
			}

			return MoveToDefaultAttribute (name, null);
		}

		public override bool MoveToAttribute (string localName, string ns)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.MoveToAttribute (localName, ns);
			}

			currentAttrType = null;
			bool b = reader.MoveToAttribute (localName, ns);
			if (b) {
				this.currentDefaultAttribute = -1;
				this.defaultAttributeConsumed = false;
				return true;
			}

			return MoveToDefaultAttribute (localName, ns);
		}

		private bool MoveToDefaultAttribute (string localName, string ns)
		{
			int idx = this.FindDefaultAttribute (localName, ns);
			if (idx < 0)
				return false;
			currentDefaultAttribute = idx;
			defaultAttributeConsumed = false;
			return true;
		}

		public override bool MoveToElement ()
		{
			currentDefaultAttribute = -1;
			defaultAttributeConsumed = false;
			currentAttrType = null;
			return reader.MoveToElement ();
		}

		public override bool MoveToFirstAttribute ()
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.MoveToFirstAttribute ();
			}

			currentAttrType = null;
			if (reader.AttributeCount > 0) {
				bool b = reader.MoveToFirstAttribute ();
				if (b) {
					currentDefaultAttribute = -1;
					defaultAttributeConsumed = false;
				}
				return b;
			}

			if (this.defaultAttributes.Length > 0) {
				currentDefaultAttribute = 0;
				defaultAttributeConsumed = false;
				return true;
			}
			else
				return false;
		}

		public override bool MoveToNextAttribute ()
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.MoveToNextAttribute ();
			}

			currentAttrType = null;
			if (currentDefaultAttribute >= 0) {
				if (defaultAttributes.Length == currentDefaultAttribute + 1)
					return false;
				currentDefaultAttribute++;
				defaultAttributeConsumed = false;
				return true;
			}

			bool b = reader.MoveToNextAttribute ();
			if (b) {
				currentDefaultAttribute = -1;
				defaultAttributeConsumed = false;
				return true;
			}

			if (defaultAttributes.Length > 0) {
				currentDefaultAttribute = 0;
				defaultAttributeConsumed = false;
				return true;
			}
			else
				return false;
		}

		public override bool Read ()
		{
			if (!reader.Read ()) {
				if (!validationDone) {
					v.EndValidation ();
					validationDone = true;
				}
				return false;
			}

			ResetStateOnRead ();

			switch (reader.NodeType) {
			case XmlNodeType.Element:
				string sl = reader.GetAttribute (
					"schemaLocation",
					XmlSchema.InstanceNamespace);
				string noNsSL = reader.GetAttribute (
					"noNamespaceSchemaLocation",
					XmlSchema.InstanceNamespace);
				string xsiType = reader.GetAttribute ("type",
					XmlSchema.InstanceNamespace);
				string xsiNil = reader.GetAttribute ("nil",
					XmlSchema.InstanceNamespace);

				v.ValidateElement (reader.LocalName,
					reader.NamespaceURI,
					xsinfo,
					xsiType,
					xsiNil,
					sl,
					noNsSL);

				if (reader.MoveToFirstAttribute ()) {
					do {
						switch (reader.NamespaceURI) {
						case XmlSchema.InstanceNamespace:
							switch (reader.LocalName) {
							case "schemaLocation":
							case "noNamespaceSchemaLocation":
							case "nil":
							case "type":
								continue;
							}
							break;
						case XmlNamespaceManager.XmlnsXmlns:
							continue;
						}
						v.ValidateAttribute (
							reader.LocalName,
							reader.NamespaceURI,
							getter,
							xsinfo);
					} while (reader.MoveToNextAttribute ());
					reader.MoveToElement ();
				}
				v.GetUnspecifiedDefaultAttributes (
					defaultAttributesCache);
				defaultAttributes = (XsAttr [])
					defaultAttributesCache.ToArray (
					typeof (XsAttr));
				v.ValidateEndOfAttributes (xsinfo);
				defaultAttributesCache.Clear ();

				if (reader.IsEmptyElement)
					goto case XmlNodeType.EndElement;
				break;
			case XmlNodeType.EndElement:
				// FIXME: find out what another overload means.
				v.ValidateEndElement (xsinfo);
				break;
			case XmlNodeType.Text:
				v.ValidateText (getter);
				break;
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Whitespace:
				v.ValidateWhitespace (getter);
				break;
			}

			return true;
		}

		public override bool ReadAttributeValue ()
		{
			if (currentDefaultAttribute < 0)
				return reader.ReadAttributeValue ();

			if (this.defaultAttributeConsumed)
				return false;

			defaultAttributeConsumed = true;
			return true;
		}

#if NET_1_0
		public override string ReadInnerXml ()
		{
			// MS.NET 1.0 has a serious bug here. It skips validation.
			return ReadInnerXmlInternal ();
		}

		public override string ReadOuterXml ()
		{
			// MS.NET 1.0 has a serious bug here. It skips validation.
			return ReadInnerXmlInternal ();
		}

		// XmlReader.ReadString() should call derived this.Read().
		public override string ReadString ()
		{
			return ReadStringInternal ();
		}
#endif

		// This class itself does not have this feature.
		public override void ResolveEntity ()
		{
			reader.ResolveEntity ();
		}

		#endregion

		#region IXmlSchemaInfo

		public bool IsNil {
			get { return xsinfo.IsNil; }
		}

		public XmlSchemaSimpleType MemberType {
			get { return xsinfo.MemberType; }
		}

		public XmlSchemaAttribute SchemaAttribute {
			get { return xsinfo.SchemaAttribute; }
		}

		public XmlSchemaElement SchemaElement {
			get { return xsinfo.SchemaElement; }
		}

		public XmlSchemaValidity Validity {
			get { return xsinfo.Validity; }
		}

		#endregion
	}
}

#endif
