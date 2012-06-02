//
// System.Xml.XmlTextReader2.cs - XmlTextReader for .NET 2.0
//
// Author:
//   Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using XmlTextReaderImpl = Mono.Xml2.XmlTextReader;

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Xml.Schema;
using Mono.Xml;

namespace System.Xml
{
	// FIXME: this implementation requires somewhat significant change
	// to expand entities and merge sequential text and entity references
	// especially to handle whitespace-only entities (such as bug #372839).
	//
	// To do it, we have to read ahead the next node when the input is
	// text, whitespace or significant whitespace and check if the next
	// node is EntityReference. If it is entref, then it have to merge
	// the input entity if it is a text.
	//
	// This "read ahead" operation may result in proceeding to the next
	// element, which badly affects IXmlNamespaceResolverimplementation.
	// So we cannot fix this in simple way.

	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	public class XmlTextReader : XmlReader,
		IXmlLineInfo, IXmlNamespaceResolver, IHasXmlParserContext
	{
		XmlTextReader entity;
		XmlTextReaderImpl source; // dtd2xsd expects this field's existence.
		bool entityInsideAttribute;
		bool insideAttribute;
		Stack<string> entityNameStack;

		protected XmlTextReader ()
		{
		}

		public XmlTextReader (Stream input)
			: this (new XmlStreamReader (input))
		{
		}

		public XmlTextReader (string url)
			: this(url, new NameTable ())
		{
		}

		public XmlTextReader (TextReader input)
			: this (input, new NameTable ())
		{
		}

		protected XmlTextReader (XmlNameTable nt)
			: this (String.Empty, XmlNodeType.Element, null)
		{
		}

		public XmlTextReader (Stream input, XmlNameTable nt)
			: this(new XmlStreamReader (input), nt)
 		{
		}

		public XmlTextReader (string url, Stream input)
			: this (url, new XmlStreamReader (input))
		{
		}

		public XmlTextReader (string url, TextReader input)
			: this (url, input, new NameTable ())
		{
		}

		public XmlTextReader (string url, XmlNameTable nt)
		{
			source = new XmlTextReaderImpl (url, nt);
		}

		public XmlTextReader (TextReader input, XmlNameTable nt)
			: this (String.Empty, input, nt)
		{
		}

		public XmlTextReader (Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
		{
			source = new XmlTextReaderImpl (xmlFragment, fragType, context);
		}

		public XmlTextReader (string url, Stream input, XmlNameTable nt)
			: this (url, new XmlStreamReader (input), nt)
		{
		}

		public XmlTextReader (string url, TextReader input, XmlNameTable nt)
		{
			source = new XmlTextReaderImpl (url, input, nt);
		}

		public XmlTextReader (string xmlFragment, XmlNodeType fragType, XmlParserContext context)
		{
			source = new XmlTextReaderImpl (xmlFragment, fragType, context);
		}

		internal XmlTextReader (string baseURI, TextReader xmlFragment, XmlNodeType fragType)
		{
			source = new XmlTextReaderImpl (baseURI, xmlFragment, fragType);
		}

		internal XmlTextReader (string baseURI, TextReader xmlFragment, XmlNodeType fragType, XmlParserContext context)
		{
			source = new XmlTextReaderImpl (baseURI, xmlFragment, fragType, context);
		}

		internal XmlTextReader (bool dummy, XmlResolver resolver, string url, XmlNodeType fragType, XmlParserContext context)
		{
			source = new XmlTextReaderImpl (dummy, resolver, url, fragType, context);
		}

		private XmlTextReader (XmlTextReaderImpl entityContainer, bool insideAttribute)
		{
			source = entityContainer;
			this.entityInsideAttribute = insideAttribute;
		}

		#region Properties

		private XmlReader Current {
			get { return entity != null && entity.ReadState != ReadState.Initial ? (XmlReader) entity : source; }
		}

		public override int AttributeCount {
			get { return Current.AttributeCount; }
		}

		public override string BaseURI {
			get { return Current.BaseURI; }
		}

		public override bool CanReadBinaryContent {
			get { return true; }
		}

		public override bool CanReadValueChunk {
			get { return true; }
		}

		public override bool CanResolveEntity {
			get { return true; }
		}

		public override int Depth {
			get {
				// On EndEntity, depth is the same as that 
				// of EntityReference.
				if (entity != null && entity.ReadState == ReadState.Interactive)
					return source.Depth + entity.Depth + 1;
				else
					return source.Depth;
			}
		}

		public override bool EOF {
			get { return source.EOF; }
		}

		public override bool HasValue {
			get { return Current.HasValue; }
		}

		public override bool IsDefault {
			get { return Current.IsDefault; }
		}

		public override bool IsEmptyElement {
			get { return Current.IsEmptyElement; }
		}

		public override string LocalName {
			get { return Current.LocalName; }
		}

		public override string Name {
			get { return Current.Name; }
		}

		public override string NamespaceURI {
			get { return Current.NamespaceURI; }
		}

		public override XmlNameTable NameTable {
			get { return Current.NameTable; }
		}

		public override XmlNodeType NodeType {
			get {
				if (entity != null)
					return entity.ReadState == ReadState.Initial ?
						source.NodeType :
						entity.EOF ? XmlNodeType.EndEntity :
						entity.NodeType;
				else
					return source.NodeType;
			}
		}

		internal XmlParserContext ParserContext {
			get { return ((IHasXmlParserContext) Current).ParserContext; }
		}

		XmlParserContext IHasXmlParserContext.ParserContext {
			get { return this.ParserContext; }
		}

		public override string Prefix {
			get { return Current.Prefix; }
		}

		public override char QuoteChar {
			get { return Current.QuoteChar; }
		}

		public override ReadState ReadState {
			get { return entity != null ? ReadState.Interactive : source.ReadState; }
		}

#if !NET_4_5
		public override XmlReaderSettings Settings {
			get { return base.Settings; }
		}
#endif

		public override string Value {
			get { return Current.Value; }
		}

		public override string XmlLang {
			get { return Current.XmlLang; }
		}

		public override XmlSpace XmlSpace {
			get { return Current.XmlSpace; }
		}

		// non-overrides

		internal bool CharacterChecking {
			get {
				if (entity != null)
					return entity.CharacterChecking;
				else
					return source.CharacterChecking;
			}
			set {
				if (entity != null)
					entity.CharacterChecking = value;
				source.CharacterChecking = value;
			}
		}

		internal bool CloseInput {
			get {
				if (entity != null)
					return entity.CloseInput;
				else
					return source.CloseInput;
			}
			set {
				if (entity != null)
					entity.CloseInput = value;
				source.CloseInput = value;
			}
		}

		internal ConformanceLevel Conformance {
			get { return source.Conformance; }
			set {
				if (entity != null)
					entity.Conformance = value;
				source.Conformance = value;
			}
		}

		internal XmlResolver Resolver {
			get { return source.Resolver; }
		}

		private void CopyProperties (XmlTextReader other)
		{
			CharacterChecking = other.CharacterChecking;
			CloseInput = other.CloseInput;
			if (other.Settings != null)
				Conformance = other.Settings.ConformanceLevel;
			XmlResolver = other.Resolver;
		}

		// public members

		public Encoding Encoding {
			get {
				if (entity != null)
					return entity.Encoding;
				else
					return source.Encoding;
			}
		}

		public EntityHandling EntityHandling {
			get { return source.EntityHandling; }
			set {
				if (entity != null)
					entity.EntityHandling = value;
				source.EntityHandling = value;
			}
		}

		public int LineNumber {
			get {
				if (entity != null)
					return entity.LineNumber;
				else
					return source.LineNumber;
			}
		}

		public int LinePosition {
			get {
				if (entity != null)
					return entity.LinePosition;
				else
					return source.LinePosition;
			}
		}

		public bool Namespaces {
			get { return source.Namespaces; }
			set {
				if (entity != null)
					entity.Namespaces = value;
				source.Namespaces = value;
			}
		}

		public bool Normalization {
			get { return source.Normalization; }
			set {
				if (entity != null)
					entity.Normalization = value;
				source.Normalization = value;
			}
		}

		public bool ProhibitDtd {
			get { return source.ProhibitDtd; }
			set {
				if (entity != null)
					entity.ProhibitDtd = value;
				source.ProhibitDtd = value;
			}
		}

		public WhitespaceHandling WhitespaceHandling {
			get { return source.WhitespaceHandling; }
			set {
				if (entity != null)
					entity.WhitespaceHandling = value;
				source.WhitespaceHandling = value;
			}
		}

		public XmlResolver XmlResolver {
			set {
				if (entity != null)
					entity.XmlResolver = value;
				source.XmlResolver = value;
			}
		}

		#endregion

		#region Methods

		internal void AdjustLineInfoOffset (int lineNumberOffset, int linePositionOffset)
		{
			if (entity != null)
				entity.AdjustLineInfoOffset (lineNumberOffset, linePositionOffset);
			source.AdjustLineInfoOffset (lineNumberOffset, linePositionOffset);
		}

		internal void SetNameTable (XmlNameTable nameTable)
		{
			if (entity != null)
				entity.SetNameTable (nameTable);
			source.SetNameTable (nameTable);
		}

		internal void SkipTextDeclaration ()
		{
			if (entity != null)
				entity.SkipTextDeclaration ();
			else
				source.SkipTextDeclaration ();
		}

		// overrides

		public override void Close ()
		{
			if (entity != null)
				entity.Close ();
			source.Close ();
		}

		public override string GetAttribute (int i)
		{
			return Current.GetAttribute (i);
		}

		// MS.NET 1.0 msdn says that this method returns String.Empty
		// for absent attribute, but in fact it returns null.
		// This description is corrected in MS.NET 1.1 msdn.
		public override string GetAttribute (string name)
		{
			return Current.GetAttribute (name);
		}

		public override string GetAttribute (string localName, string namespaceURI)
		{
			return Current.GetAttribute (localName, namespaceURI);
		}

		public IDictionary<string, string> GetNamespacesInScope (XmlNamespaceScope scope)
		{
			return ((IXmlNamespaceResolver) Current).GetNamespacesInScope (scope);
		}

		IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope (XmlNamespaceScope scope)
		{
			return GetNamespacesInScope (scope);
		}

		public override string LookupNamespace (string prefix)
		{
			return Current.LookupNamespace (prefix);
		}

		string IXmlNamespaceResolver.LookupPrefix (string ns)
		{
			return ((IXmlNamespaceResolver) Current).LookupPrefix (ns);
		}

		public override void MoveToAttribute (int i)
		{
			if (entity != null && entityInsideAttribute)
				CloseEntity ();
			Current.MoveToAttribute (i);
			insideAttribute = true;
		}

		public override bool MoveToAttribute (string name)
		{
			if (entity != null && !entityInsideAttribute)
				return entity.MoveToAttribute (name);
			if (!source.MoveToAttribute (name))
				return false;
			if (entity != null && entityInsideAttribute)
				CloseEntity ();
			insideAttribute = true;
			return true;
		}

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			if (entity != null && !entityInsideAttribute)
				return entity.MoveToAttribute (localName, namespaceURI);
			if (!source.MoveToAttribute (localName, namespaceURI))
				return false;
			if (entity != null && entityInsideAttribute)
				CloseEntity ();
			insideAttribute = true;
			return true;
		}

		public override bool MoveToElement ()
		{
			if (entity != null && entityInsideAttribute)
				CloseEntity ();
			if (!Current.MoveToElement ())
				return false;
			insideAttribute = false;
			return true;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (entity != null && !entityInsideAttribute)
				return entity.MoveToFirstAttribute ();
			if (!source.MoveToFirstAttribute ())
				return false;
			if (entity != null && entityInsideAttribute)
				CloseEntity ();
			insideAttribute = true;
			return true;
		}

		public override bool MoveToNextAttribute ()
		{
			if (entity != null && !entityInsideAttribute)
				return entity.MoveToNextAttribute ();
			if (!source.MoveToNextAttribute ())
				return false;
			if (entity != null && entityInsideAttribute)
				CloseEntity ();
			insideAttribute = true;
			return true;
		}

		public override bool Read ()
		{
			insideAttribute = false;

			if (entity != null && (entityInsideAttribute || entity.EOF))
				CloseEntity ();
			if (entity != null) {
				if (entity.Read ())
					return true;
				if (EntityHandling == EntityHandling.ExpandEntities) {
					// EndEntity must be skipped
					CloseEntity ();
					return Read ();
				}
				else
					return true; // either success or EndEntity
			}
			else {
				if (!source.Read ())
					return false;
				if (EntityHandling == EntityHandling.ExpandEntities
					&& source.NodeType == XmlNodeType.EntityReference) {
					ResolveEntity ();
					return Read ();
				}
				return true;
			}
		}

		public override bool ReadAttributeValue ()
		{
			if (entity != null && entityInsideAttribute) {
				if (entity.EOF)
					CloseEntity ();
				else {
					entity.Read ();
					return true; // either success or EndEntity
				}
			}
			return Current.ReadAttributeValue ();
		}

		public override string ReadString ()
		{
			return base.ReadString ();
		}

		public void ResetState ()
		{
			if (entity != null)
				CloseEntity ();
			source.ResetState ();
		}

		public override
		void ResolveEntity ()
		{
			if (entity != null)
				entity.ResolveEntity ();
			else {
				if (source.NodeType != XmlNodeType.EntityReference)
					throw new InvalidOperationException ("The current node is not an Entity Reference");
				XmlTextReaderImpl entReader = null;
				if (ParserContext.Dtd != null)
					entReader = ParserContext.Dtd.GenerateEntityContentReader (source.Name, ParserContext);
				if (entReader == null)
					throw new XmlException (this as IXmlLineInfo, this.BaseURI, String.Format ("Reference to undeclared entity '{0}'.", source.Name));
				if (entityNameStack == null)
					entityNameStack = new Stack<string> ();
				else if (entityNameStack.Contains (Name))
					throw new XmlException (String.Format ("General entity '{0}' has an invalid recursive reference to itself.", Name));
				entityNameStack.Push (Name);
				entity = new XmlTextReader (
					entReader, insideAttribute);
				entity.entityNameStack = entityNameStack;
				entity.CopyProperties (this);
			}
		}

		void CloseEntity ()
		{
			entity.Close ();
			entity = null;
			entityNameStack.Pop ();
		}

		public override void Skip ()
		{
			base.Skip ();
		}

		[MonoTODO] // FIXME: Check how expanded entity is handled here.
		public TextReader GetRemainder ()
		{
			if (entity != null) {
				entity.Close ();
				entity = null;
				entityNameStack.Pop ();
			}
			return source.GetRemainder ();
		}

		public bool HasLineInfo ()
		{
			return true;
		}

		[MonoTODO] // FIXME: Check how expanded entity is handled here.
		public int ReadBase64 (byte [] array, int offset, int len)
		{
			if (entity != null)
				return entity.ReadBase64 (array, offset, len);
			else
				return source.ReadBase64 (array, offset, len);
		}

		[MonoTODO] // FIXME: Check how expanded entity is handled here.
		public int ReadBinHex (byte [] array, int offset, int len)
		{
			if (entity != null)
				return entity.ReadBinHex (array, offset, len);
			else
				return source.ReadBinHex (array, offset, len);
		}

		[MonoTODO] // FIXME: Check how expanded entity is handled here.
		public int ReadChars (char [] buffer, int index, int count)
		{
			if (entity != null)
				return entity.ReadChars (buffer, index, count);
			else
				return source.ReadChars (buffer, index, count);
		}


		[MonoTODO] // FIXME: Check how expanded entity is handled here.
		public override int ReadContentAsBase64 (byte [] buffer, int index, int count)
		{
			if (entity != null)
				return entity.ReadContentAsBase64 (buffer, index, count);
			else
				return source.ReadContentAsBase64 (buffer, index, count);
		}

		[MonoTODO] // FIXME: Check how expanded entity is handled here.
		public override int ReadContentAsBinHex (byte [] buffer, int index, int count)
		{
			if (entity != null)
				return entity.ReadContentAsBinHex (buffer, index, count);
			else
				return source.ReadContentAsBinHex (buffer, index, count);
		}

		[MonoTODO] // FIXME: Check how expanded entity is handled here.
		public override int ReadElementContentAsBase64 (byte [] buffer, int index, int count)
		{
			if (entity != null)
				return entity.ReadElementContentAsBase64 (buffer, index, count);
			else
				return source.ReadElementContentAsBase64 (buffer, index, count);
		}

		[MonoTODO] // FIXME: Check how expanded entity is handled here.
		public override int ReadElementContentAsBinHex (byte [] buffer, int index, int count)
		{
			if (entity != null)
				return entity.ReadElementContentAsBinHex (buffer, index, count);
			else
				return source.ReadElementContentAsBinHex (buffer, index, count);
		}
		#endregion
	}
}

#endif
