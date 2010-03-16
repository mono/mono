//
// System.Xml.XmlNodeReader2.cs - splitted XmlNodeReader that manages entities.
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//	Atsushi Enomoto  (ginga@kit.hi-ho.ne.jp)
//
// (C) Ximian, Inc.
// (C) Atsushi Enomoto
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
#if NET_2_0
using System.Collections.Generic;
#endif
using System.Xml;
using System.Xml.Schema;
using System.Text;
using Mono.Xml;

namespace System.Xml
{
#if NET_2_0
	public class XmlNodeReader : XmlReader, IHasXmlParserContext, IXmlNamespaceResolver
#else
	public class XmlNodeReader : XmlReader, IHasXmlParserContext
#endif
	{
		XmlReader entity;
		XmlNodeReaderImpl source;
		bool entityInsideAttribute;
		bool insideAttribute;

		#region Constructor

		public XmlNodeReader (XmlNode node)
		{
			source = new XmlNodeReaderImpl (node);
		}
		
		private XmlNodeReader (XmlNodeReaderImpl entityContainer, bool insideAttribute)
		{
			source = new XmlNodeReaderImpl (entityContainer);
			this.entityInsideAttribute = insideAttribute;
		}
		
		#endregion

		#region Properties

		private XmlReader Current {
			get { return entity != null && entity.ReadState != ReadState.Initial ? entity : source; }
		}

		public override int AttributeCount {
			get { return Current.AttributeCount; }
		}

		public override string BaseURI {
			get { return Current.BaseURI; }
		}

#if NET_2_0
		public override bool CanReadBinaryContent {
			get { return true; }
		}

/*
		public override bool CanReadValueChunk {
			get { return true; }
		}
*/
#else
		internal override bool CanReadBinaryContent {
			get { return true; }
		}

/*
		internal override bool CanReadValueChunk {
			get { return true; }
		}
*/
#endif

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

		public override bool HasAttributes {
			get { return Current.HasAttributes; }
		}

#if !MOONLIGHT
		public override bool HasValue {
			get { return Current.HasValue; }
		}
#endif

		public override bool IsDefault {
			get { return Current.IsDefault; }
		}

		public override bool IsEmptyElement {
			get { return Current.IsEmptyElement; }
		}

#if NET_2_0
#else
		public override string this [int i] {
			get { return GetAttribute (i); }
		}

		public override string this [string name] {
			get { return GetAttribute (name); }
		}

		public override string this [string name, string namespaceURI] {
			get { return GetAttribute (name, namespaceURI); }
		}
#endif

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

		XmlParserContext IHasXmlParserContext.ParserContext {
			get { return ((IHasXmlParserContext) Current).ParserContext; }
		}

		public override string Prefix {
			get { return Current.Prefix; }
		}

#if NET_2_0
#else
		public override char QuoteChar {
			get { return '"'; }
		}
#endif

		public override ReadState ReadState {
			get { return entity != null ? ReadState.Interactive : source.ReadState; }
		}

#if NET_2_0
		public override IXmlSchemaInfo SchemaInfo {
			get { return entity != null ? entity.SchemaInfo : source.SchemaInfo; }
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
		#endregion

		#region Methods

		// If current entityReference is a child of an attribute,
		// then MoveToAttribute simply means that we no more need this entity Current.
		// Otherwise, this invokation means that
		// it is expected to move to resolved (maybe) element's attribute.
		//
		// This rule applies to many methods like MoveTo*Attribute().

		public override void Close ()
		{
			if (entity != null)
				entity.Close ();
			source.Close ();
		}

		public override string GetAttribute (int attributeIndex)
		{
			return Current.GetAttribute (attributeIndex);
		}

		public override string GetAttribute (string name)
		{
			return Current.GetAttribute (name);
		}

		public override string GetAttribute (string name, string namespaceURI)
		{
			return Current.GetAttribute (name, namespaceURI);
		}

#if NET_2_0
		IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope (XmlNamespaceScope scope)
		{
			return ((IXmlNamespaceResolver) Current).GetNamespacesInScope (scope);
		}
#endif

		public override string LookupNamespace (string prefix)
		{
			return Current.LookupNamespace (prefix);
		}

#if NET_2_0
		string IXmlNamespaceResolver.LookupPrefix (string ns)
		{
			return ((IXmlNamespaceResolver) Current).LookupPrefix (ns);
		}
#endif

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

		public override bool MoveToAttribute (string localName, string namespaceURI)
		{
			if (entity != null && !entityInsideAttribute)
				return entity.MoveToAttribute (localName, namespaceURI);
			if (!source.MoveToAttribute (localName, namespaceURI))
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
			if (entity != null && entityInsideAttribute)
				entity = null;
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
			if (entity != null && (entityInsideAttribute || entity.EOF))
				entity = null;
			if (entity != null) {
				entity.Read ();
				return true; // either success or EndEntity
			}
			else
				return source.Read ();
		}

		public override bool ReadAttributeValue ()
		{
			if (entity != null && entityInsideAttribute) {
				if (entity.EOF)
					entity = null;
				else {
					entity.Read ();
					return true; // either success or EndEntity
				}
			}
			return Current.ReadAttributeValue ();
		}

#if NET_2_0
		public override int ReadContentAsBase64 (
			byte [] buffer, int offset, int length)
		{
//			return base.ReadContentAsBase64 (
//				buffer, offset, length);
			// FIXME: This is problematic wrt end of entity.
			if (entity != null)
				return entity.ReadContentAsBase64 (
					buffer, offset, length);
			else
				return source.ReadContentAsBase64 (
					buffer, offset, length);
		}

		public override int ReadContentAsBinHex (
			byte [] buffer, int offset, int length)
		{
//			return base.ReadContentAsBinHex (
//				buffer, offset, length);
			// FIXME: This is problematic wrt end of entity.
			if (entity != null)
				return entity.ReadContentAsBinHex (
					buffer, offset, length);
			else
				return source.ReadContentAsBinHex (
					buffer, offset, length);
		}

		public override int ReadElementContentAsBase64 (
			byte [] buffer, int offset, int length)
		{
//			return base.ReadElementContentAsBase64 (
//				buffer, offset, length);
			// FIXME: This is problematic wrt end of entity.
			if (entity != null)
				return entity.ReadElementContentAsBase64 (
					buffer, offset, length);
			else
				return source.ReadElementContentAsBase64 (
					buffer, offset, length);
		}

		public override int ReadElementContentAsBinHex (
			byte [] buffer, int offset, int length)
		{
//			return base.ReadElementContentAsBinHex (
//				buffer, offset, length);
			// FIXME: This is problematic wrt end of entity.
			if (entity != null)
				return entity.ReadElementContentAsBinHex (
					buffer, offset, length);
			else
				return source.ReadElementContentAsBinHex (
					buffer, offset, length);
		}
#endif

		public override string ReadString ()
		{
			return base.ReadString ();
		}

#if !MOONLIGHT
		public override void ResolveEntity ()
		{
			if (entity != null)
				entity.ResolveEntity ();
			else {
				if (source.NodeType != XmlNodeType.EntityReference)
					throw new InvalidOperationException ("The current node is not an Entity Reference");
				entity = new XmlNodeReader (source, insideAttribute);
			}
		}
#endif

		public override void Skip ()
		{
			if (entity != null && entityInsideAttribute)
				entity = null;
			Current.Skip ();
		}
		#endregion
	}
}
