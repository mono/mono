//
// System.Xml.XmlTextReader2.cs - XmlTextReader for .NET 2.0
//
// Author:
//   Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// Copyright (C) 2004 Novell, Inc.
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

using XmlTextReaderImpl = Mono.Xml2.XmlTextReader;

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Schema;
using Mono.Xml;

namespace System.Xml
{
	public class XmlTextReader : XmlReader,
		IXmlLineInfo, IXmlNamespaceResolver, IHasXmlParserContext
	{
		XmlTextReader entity;
		XmlTextReaderImpl source;
		bool entityInsideAttribute;
		bool insideAttribute;
		string cachedAttributeValue;
		bool attributeValueConsumed;

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
				if (Current == entity)
					return entity.EOF ? XmlNodeType.EndEntity : entity.NodeType;
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

		public override XmlReaderSettings Settings {
			get { return Current.Settings; }
		}

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

		public IDictionary GetNamespacesInScope (XmlNamespaceScope scope)
		{
			return ((IXmlNamespaceResolver) Current).GetNamespacesInScope (scope);
		}

		public override string LookupNamespace (string prefix)
		{
			return Current.LookupNamespace (prefix, false);
		}

		public override string LookupNamespace (string prefix, bool atomizedName)
		{
			return ((IXmlNamespaceResolver) Current).LookupNamespace (prefix, atomizedName);
		}

		string IXmlNamespaceResolver.LookupPrefix (string ns)
		{
			return ((IXmlNamespaceResolver) Current).LookupPrefix (ns, false);
		}

		public string LookupPrefix (string ns, bool atomizedName)
		{
			return ((IXmlNamespaceResolver) Current).LookupPrefix (ns, atomizedName);
		}

		public override void MoveToAttribute (int i)
		{
			if (entity != null && entityInsideAttribute) {
				entity.Close ();
				entity = null;
			}
			Current.MoveToAttribute (i);
			insideAttribute = true;
		}

		public override bool MoveToAttribute (string name)
		{
			if (entity != null && !entityInsideAttribute)
				return entity.MoveToAttribute (name);
			if (!source.MoveToAttribute (name))
				return false;
			if (entity != null && entityInsideAttribute) {
				entity.Close ();
				entity = null;
			}
			insideAttribute = true;
			return true;
		}

		public override bool MoveToAttribute (string localName, string namespaceName)
		{
			if (entity != null && !entityInsideAttribute)
				return entity.MoveToAttribute (localName, namespaceName);
			if (!source.MoveToAttribute (localName, namespaceName))
				return false;
			if (entity != null && entityInsideAttribute) {
				entity.Close ();
				entity = null;
			}
			insideAttribute = true;
			return true;
		}

		public override bool MoveToElement ()
		{
			if (entity != null && entityInsideAttribute) {
				entity.Close ();
				entity = null;
			}
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
			if (entity != null && entityInsideAttribute) {
				entity.Close ();
				entity = null;
			}
			insideAttribute = true;
			return true;
		}

		public override bool MoveToNextAttribute ()
		{
			if (entity != null && !entityInsideAttribute)
				return entity.MoveToNextAttribute ();
			if (!source.MoveToNextAttribute ())
				return false;
			if (entity != null && entityInsideAttribute) {
				entity.Close ();
				entity = null;
			}
			insideAttribute = true;
			return true;
		}

		public override bool Read ()
		{
			insideAttribute = false;

			if (entity != null && (entityInsideAttribute || entity.EOF)) {
				entity.Close ();
				entity = null;
			}
			if (entity != null) {
				if (entity.Read ())
					return true;
				if (EntityHandling == EntityHandling.ExpandEntities) {
					// EndEntity must be skipped
					entity.Close ();
					entity = null;
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
				if (entity.EOF) {
					entity.Close ();
					entity = null;
				}
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
				entity.ResetState ();
			source.ResetState ();
		}

		public override void ResolveEntity ()
		{
			if (entity != null)
				entity.ResolveEntity ();
			else {
				if (source.NodeType != XmlNodeType.EntityReference)
					throw new InvalidOperationException ("The current node is not an Entity Reference");
				XmlTextReaderImpl entReader = 
					ParserContext.Dtd.GenerateEntityContentReader (source.Name, ParserContext);
				if (entReader == null)
					throw new XmlException (this as IXmlLineInfo, this.BaseURI, String.Format ("Reference to undeclared entity '{0}'.", source.Name));
				entity = new XmlTextReader (
					entReader, insideAttribute);
				entity.CopyProperties (this);
			}
		}

		public override void Skip ()
		{
			base.Skip ();
		}

		[MonoTODO ("Check how expanded entity is handled here.")]
		public TextReader GetRemainder ()
		{
			if (entity != null) {
				entity.Close ();
				entity = null;
			}
			return source.GetRemainder ();
		}

		public bool HasLineInfo ()
		{
			return true;
		}

		[MonoTODO ("Check how expanded entity is handled here.")]
		public int ReadBase64 (byte [] buffer, int offset, int length)
		{
			if (entity != null)
				return entity.ReadBase64 (buffer, offset, length);
			else
				return source.ReadBase64 (buffer, offset, length);
		}

		[MonoTODO ("Check how expanded entity is handled here.")]
		public int ReadBinHex (byte [] buffer, int offset, int length)
		{
			if (entity != null)
				return entity.ReadBinHex (buffer, offset, length);
			else
				return source.ReadBinHex (buffer, offset, length);
		}

		[MonoTODO ("Check how expanded entity is handled here.")]
		public int ReadChars (char [] buffer, int offset, int length)
		{
			if (entity != null)
				return entity.ReadChars (buffer, offset, length);
			else
				return source.ReadChars (buffer, offset, length);
		}

		#endregion
	}
}

#endif
