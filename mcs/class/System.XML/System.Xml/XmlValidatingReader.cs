//
// System.Xml.XmlValidatingReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// Copyright (C) Tim Coleman, 2002
// (C)2003 Atsushi Enomoto
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
#endif
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Xml.Schema;
using Mono.Xml;
using Mono.Xml.Schema;

namespace System.Xml
{
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
#if NET_2_0
	[Obsolete("Use XmlReader created by XmlReader.Create() method using"
		+ " appropriate XmlReaderSettings instead.")]
	public class XmlValidatingReader : XmlReader, IXmlLineInfo, IXmlNamespaceResolver, IHasXmlParserContext
#else
	public class XmlValidatingReader : XmlReader, IXmlLineInfo, IHasXmlParserContext
#endif
	{

		#region Fields

		EntityHandling entityHandling;
		XmlReader sourceReader;
		XmlTextReader xmlTextReader;
		XmlReader validatingReader;
		XmlResolver resolver; // Only used to non-XmlTextReader XmlReader
		bool resolverSpecified;
		ValidationType validationType;
		// for 2.0: Now it is obsolete. It is allocated only when it is required
		XmlSchemaCollection schemas;
		DTDValidatingReader dtdReader;
		IHasXmlSchemaInfo schemaInfo;
		StringBuilder storedCharacters;

		#endregion // Fields

		#region Constructors

		public XmlValidatingReader (XmlReader reader)
		{
			sourceReader = reader;
			xmlTextReader = reader as XmlTextReader;
			if (xmlTextReader == null)
				resolver = new XmlUrlResolver ();
			entityHandling = EntityHandling.ExpandEntities;
			validationType = ValidationType.Auto;
			storedCharacters = new StringBuilder ();
		}

		public XmlValidatingReader (Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
			: this (new XmlTextReader (xmlFragment, fragType, context))
		{
		}

		public XmlValidatingReader (string xmlFragment, XmlNodeType fragType, XmlParserContext context)
			: this (new XmlTextReader (xmlFragment, fragType, context))
		{
		}

		#endregion // Constructors

		#region Properties

		public override int AttributeCount {
			get { return validatingReader == null ? 0 : validatingReader.AttributeCount; }
		}

		public override string BaseURI {
			get { return validatingReader == null ? sourceReader.BaseURI : validatingReader.BaseURI; }
		}

#if NET_2_0
		public override bool CanReadBinaryContent {
			get { return true; }
		}
#endif

		// This property for this class always return true.
		public override bool CanResolveEntity {
			get { return true; }
		}

		public override int Depth { 
			get { return validatingReader == null ? 0 : validatingReader.Depth; }
		}

		public Encoding Encoding {
			get {
				if (xmlTextReader != null)
					return xmlTextReader.Encoding;
				else
					throw new NotSupportedException ("Encoding is supported only for XmlTextReader.");
			}
		}

		public EntityHandling EntityHandling {
			get { return entityHandling; }
			set {
				entityHandling = value;
				if (dtdReader != null)
					dtdReader.EntityHandling = value;
			}
		}

		public override bool EOF { 
			get { return validatingReader == null ? false : validatingReader.EOF; }
		}

#if DTD_HANDLE_EVENTS
		internal bool HasValidationEvent {
			get { return ValidationEventHandler != null; }
		}
#endif

		public override bool HasValue { 
			get { return validatingReader == null ? false : validatingReader.HasValue; }
		}

		public override bool IsDefault {
			get { return validatingReader == null ? false : validatingReader.IsDefault; }
		}

		public override bool IsEmptyElement { 
			get { return validatingReader == null ? false : validatingReader.IsEmptyElement; }
		}

#if NET_2_0
#else
		public override string this [int i] { 
			get { return GetAttribute (i); }
		}

		public override string this [string name] { 
			get { return GetAttribute (name); }
		}

		public override string this [string localName, string namespaceName] { 
			get { return GetAttribute (localName, namespaceName); }
		}
#endif

#if NET_2_0
		public int LineNumber {
#else
		int IXmlLineInfo.LineNumber {
#endif
			get {
				if (IsDefault)
					return 0;
				IXmlLineInfo info = validatingReader as IXmlLineInfo;
				return info != null ? info.LineNumber : 0;
			}
		}

#if NET_2_0
		public int LinePosition {
#else
		int IXmlLineInfo.LinePosition {
#endif
			get {
				if (IsDefault)
					return 0;
				IXmlLineInfo info = validatingReader as IXmlLineInfo;
				return info != null ? info.LinePosition : 0;
			}
		}

		public override string LocalName { 
			get {
				if (validatingReader == null)
					return String.Empty;
				else if (Namespaces)
					return validatingReader.LocalName;
				else
					return validatingReader.Name;
			}
		}

		public override string Name {
			get { return validatingReader == null ? String.Empty : validatingReader.Name; }
		}

		public bool Namespaces {
			get {
				if (xmlTextReader != null)
					return xmlTextReader.Namespaces;
				else
					return true;
			}
			set {
				if (ReadState != ReadState.Initial)
					throw new InvalidOperationException ("Namespaces have to be set before reading.");

				if (xmlTextReader != null)
					xmlTextReader.Namespaces = value;
				else
					throw new NotSupportedException ("Property 'Namespaces' is supported only for XmlTextReader.");
			}
		}

		public override string NamespaceURI { 
			get {
				if (validatingReader == null)
					return String.Empty;
				else if (Namespaces)
					return validatingReader.NamespaceURI;
				else
					return String.Empty;
			}
		}

		public override XmlNameTable NameTable { 
			get { return validatingReader == null ? sourceReader.NameTable : validatingReader.NameTable; }
		}

		public override XmlNodeType NodeType { 
			get { return validatingReader == null ? XmlNodeType.None : validatingReader.NodeType; }
		}

		public override string Prefix {
			get { return validatingReader == null ? String.Empty : validatingReader.Prefix; }
		}

		public override char QuoteChar { 
			get { return validatingReader == null ? sourceReader.QuoteChar : validatingReader.QuoteChar; }
		}

		public XmlReader Reader {
			get { return sourceReader; }
		}

		public override ReadState ReadState { 
			get {
				if (validatingReader == null)
					return ReadState.Initial;
				return validatingReader.ReadState; 
			}
		}

		internal XmlResolver Resolver {
			get {
				// This is special rule... MS.NET shares the
				// XmlResolver between XmlTextReader and
				// XmlValidatingReader, so we mimick that
				// silly behavior here.
				if (this.xmlTextReader != null)
					return this.xmlTextReader.Resolver;
				else if (resolverSpecified)
					return resolver;
				else
					return null;
			}
		}

		public XmlSchemaCollection Schemas {
			get {
				if (schemas == null)
					schemas = new XmlSchemaCollection (NameTable);
				return schemas;
			}
		}

		public object SchemaType {
			get { return schemaInfo.SchemaType; }
		}

#if NET_2_0
		[MonoTODO]
		public override XmlReaderSettings Settings {
			get { return validatingReader == null ? sourceReader.Settings : validatingReader.Settings; }
		}
#endif

		[MonoTODO]
		// We decided not to support XDR schema; it is obsolete.
		public ValidationType ValidationType {
			get { return validationType; }
			set {
				if (ReadState != ReadState.Initial)
					throw new InvalidOperationException ("ValidationType cannot be set after the first call to Read method.");
				switch (validationType) {
				case ValidationType.Auto:
				case ValidationType.DTD:
				case ValidationType.None:
				case ValidationType.Schema:
					validationType = value; 
					break;
				case ValidationType.XDR:
					throw new NotSupportedException ();
				}
			}
		}

		public override string Value {
			get { return validatingReader == null ? String.Empty : validatingReader.Value; }
		}

		public override string XmlLang {
			get { return validatingReader == null ? String.Empty : validatingReader.XmlLang; }
		}

		public XmlResolver XmlResolver {
			set {
				resolverSpecified = true;
				resolver = value;
				if (xmlTextReader != null)
					xmlTextReader.XmlResolver = value;

				XsdValidatingReader xsvr = validatingReader as XsdValidatingReader;
				if (xsvr != null)
					xsvr.XmlResolver = value;
				DTDValidatingReader dvr = validatingReader as DTDValidatingReader;
				if (dvr != null)
					dvr.XmlResolver = value;
			}
		}

		public override XmlSpace XmlSpace {
			get { return validatingReader == null ? XmlSpace.None : validatingReader.XmlSpace; }
		}

		#endregion // Properties

		#region Methods

		public override void Close ()
		{
			if (validatingReader == null)
				sourceReader.Close ();
			else
				validatingReader.Close ();
		}

		public override string GetAttribute (int i)
		{
			if (validatingReader == null)
				throw new IndexOutOfRangeException ("Reader is not started.");
			return validatingReader [i];
		}

		public override string GetAttribute (string name)
		{
			return validatingReader == null ? null : validatingReader [name];
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			return validatingReader == null ? null : validatingReader [localName, namespaceURI];
		}

		XmlParserContext IHasXmlParserContext.ParserContext {
			get {
				if (dtdReader != null)
					return dtdReader.ParserContext;
				IHasXmlParserContext i = sourceReader as IHasXmlParserContext;
				return i != null ? i.ParserContext : null;
			}
		}

#if NET_2_0
		IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope (XmlNamespaceScope scope)
		{
			return ((IHasXmlParserContext) this).ParserContext.NamespaceManager.GetNamespacesInScope (scope);
		}
#endif

#if NET_2_0
		public bool HasLineInfo ()
#else
		bool IXmlLineInfo.HasLineInfo ()
#endif
		{
			IXmlLineInfo info = validatingReader as IXmlLineInfo;
			return info != null ? info.HasLineInfo () : false;
		}

		public override string LookupNamespace (string prefix)
		{
			if (validatingReader != null)
				return validatingReader.LookupNamespace (prefix);
			else
				return sourceReader.LookupNamespace (prefix);
		}

#if NET_2_0
		string IXmlNamespaceResolver.LookupPrefix (string ns)
		{
			IXmlNamespaceResolver res = null;
			if (validatingReader != null)
				res = sourceReader as IXmlNamespaceResolver;
			else
				res = validatingReader as IXmlNamespaceResolver;
			return res != null ?
				res.LookupNamespace (ns) :
				null;
		}
#endif


		public override void MoveToAttribute (int i)
		{
			if (validatingReader == null)
				throw new IndexOutOfRangeException ("Reader is not started.");
			else
				validatingReader.MoveToAttribute (i);
		}

		public override bool MoveToAttribute (string name)
		{
			if (validatingReader == null)
				return false;
			return validatingReader.MoveToAttribute (name);
		}

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			if (validatingReader == null)
				return false;
			return validatingReader.MoveToAttribute (localName, namespaceURI);
		}

		public override bool MoveToElement ()
		{
			if (validatingReader == null)
				return false;
			return validatingReader.MoveToElement ();
		}

		public override bool MoveToFirstAttribute ()
		{
			if (validatingReader == null)
				return false;
			return validatingReader.MoveToFirstAttribute ();
		}

		public override bool MoveToNextAttribute ()
		{
			if (validatingReader == null)
				return false;
			return validatingReader.MoveToNextAttribute ();
		}

		[MonoTODO]
		// We decided not to support XDR schema; it is obsolete.
		public override bool Read ()
		{
			if (validatingReader == null) {
				switch (ValidationType) {
				case ValidationType.Auto:
				case ValidationType.None:
					goto case ValidationType.Schema; // might be specified by xsi:schemaLocation.
				case ValidationType.DTD:
					validatingReader = dtdReader = new DTDValidatingReader (sourceReader, this);
					dtdReader.XmlResolver = Resolver;
					break;
				case ValidationType.Schema:
					dtdReader = new DTDValidatingReader (sourceReader, this);
					XsdValidatingReader xsvr = new XsdValidatingReader (dtdReader);
					xsvr.ValidationEventHandler +=
						new ValidationEventHandler (
							OnValidationEvent);
					xsvr.ValidationType = ValidationType;
					xsvr.Schemas = Schemas.SchemaSet;
					xsvr.XmlResolver = Resolver;
					validatingReader = xsvr;
					dtdReader.XmlResolver = Resolver;
					break;
				case ValidationType.XDR:
					throw new NotSupportedException ();
				}
				schemaInfo = validatingReader as IHasXmlSchemaInfo;
			}
			return validatingReader.Read ();
		}

		public override bool ReadAttributeValue ()
		{
			if (validatingReader == null)
				return false;
			return validatingReader.ReadAttributeValue ();
		}

		public override string ReadString ()
		{
			return base.ReadString ();
		}

		public object ReadTypedValue ()
		{
			if (dtdReader == null)
				return null;
			XmlSchemaDatatype dt = schemaInfo.SchemaType as XmlSchemaDatatype;
			if (dt == null) {
				XmlSchemaType st = schemaInfo.SchemaType as XmlSchemaType;
				if (st != null)
					dt = st.Datatype;
			}
			if (dt == null)
				return null;
			switch (NodeType) {
			case XmlNodeType.Element:
				if (IsEmptyElement)
					return null;

				storedCharacters.Length = 0;
				bool loop = true;
				do {
					Read ();
					switch (NodeType) {
					case XmlNodeType.Whitespace:
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
						storedCharacters.Append (Value);
						break;
					case XmlNodeType.Comment:
						break;
					default:
						loop = false;
						break;
					}
				} while (loop && !EOF);
				return dt.ParseValue (storedCharacters.ToString (), NameTable, dtdReader.ParserContext.NamespaceManager);
			case XmlNodeType.Attribute:
				return dt.ParseValue (Value, NameTable, dtdReader.ParserContext.NamespaceManager);
			}
			return null;
		}

		public override void ResolveEntity ()
		{
			validatingReader.ResolveEntity ();
		}

		// It should be "protected" as usual "event model"
		// methods are, but validation event is not exposed,
		// so it is no other way to make it "internal".
		internal void OnValidationEvent (object o, ValidationEventArgs e)
		{
			if (ValidationEventHandler != null)
				ValidationEventHandler (o, e);
			else if (ValidationType != ValidationType.None && e.Severity == XmlSeverityType.Error)
				throw e.Exception;
		}

#if NET_2_0
		[MonoTODO] // FIXME: Check how expanded entity is handled here.
		public override int ReadContentAsBase64 (byte [] buffer, int index, int count)
		{
			if (validatingReader != null)
				return validatingReader.ReadContentAsBase64 (buffer, index, count);
			else
				return sourceReader.ReadContentAsBase64 (buffer, index, count);
		}

		[MonoTODO] // FIXME: Check how expanded entity is handled here.
		public override int ReadContentAsBinHex (byte [] buffer, int index, int count)
		{
			if (validatingReader != null)
				return validatingReader.ReadContentAsBinHex (buffer, index, count);
			else
				return sourceReader.ReadContentAsBinHex (buffer, index, count);
		}

		[MonoTODO] // FIXME: Check how expanded entity is handled here.
		public override int ReadElementContentAsBase64 (byte [] buffer, int index, int count)
		{
			if (validatingReader != null)
				return validatingReader.ReadElementContentAsBase64 (buffer, index, count);
			else
				return sourceReader.ReadElementContentAsBase64 (buffer, index, count);
		}

		[MonoTODO] // FIXME: Check how expanded entity is handled here.
		public override int ReadElementContentAsBinHex (byte [] buffer, int index, int count)
		{
			if (validatingReader != null)
				return validatingReader.ReadElementContentAsBinHex (buffer, index, count);
			else
				return sourceReader.ReadElementContentAsBinHex (buffer, index, count);
		}
#endif
		#endregion // Methods

		#region Events and Delegates

		public event ValidationEventHandler ValidationEventHandler;

		#endregion // Events and Delegates
	}
}
