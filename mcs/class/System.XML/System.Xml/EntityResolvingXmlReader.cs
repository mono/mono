//
// EntityResolvingXmlReader.cs - XmlReader that handles entity resolution
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

using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Xml.Schema;
using System.Xml;

namespace Mono.Xml
{
	[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
	internal class EntityResolvingXmlReader : XmlReader, IXmlNamespaceResolver,
		IXmlLineInfo, IHasXmlParserContext
	{
		EntityResolvingXmlReader entity;
		XmlReader source;
		XmlParserContext context;
		XmlResolver resolver;
		EntityHandling entity_handling;
		bool entity_inside_attr;
		bool inside_attr;
		bool do_resolve;

		public EntityResolvingXmlReader (XmlReader source)
		{
			this.source = source;
			IHasXmlParserContext container = source as IHasXmlParserContext;
			if (container != null)
				this.context = container.ParserContext;
			else
				this.context = new XmlParserContext (source.NameTable, new XmlNamespaceManager (source.NameTable), null, XmlSpace.None);
		}

		EntityResolvingXmlReader (XmlReader entityContainer,
			bool inside_attr)
		{
			source = entityContainer;
			this.entity_inside_attr = inside_attr;
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
				if (entity != null) {
					if (entity.ReadState == ReadState.Initial)
						return source.NodeType;
					return entity.EOF ? XmlNodeType.EndEntity : entity.NodeType;
				}
				return source.NodeType;
			}
		}

		internal XmlParserContext ParserContext {
			get { return context; }
		}

		XmlParserContext IHasXmlParserContext.ParserContext {
			get { return context; }
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

		private void CopyProperties (EntityResolvingXmlReader other)
		{
			context = other.context;
			resolver = other.resolver;
			entity_handling = other.entity_handling;
		}

		// public members

		public EntityHandling EntityHandling {
			get { return entity_handling; }
			set {
				if (entity != null)
					entity.EntityHandling = value;
				entity_handling = value;
			}
		}

		public int LineNumber {
			get {
				IXmlLineInfo li = Current as IXmlLineInfo;
				return li == null ? 0 : li.LineNumber;
			}
		}

		public int LinePosition {
			get {
				IXmlLineInfo li = Current as IXmlLineInfo;
				return li == null ? 0 : li.LinePosition;
			}
		}

		public XmlResolver XmlResolver {
			set {
				if (entity != null)
					entity.XmlResolver = value;
				resolver = value;
			}
		}

		#endregion

		#region Methods

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

		string IXmlNamespaceResolver.LookupPrefix (string ns)
		{
			return ((IXmlNamespaceResolver) Current).LookupPrefix (ns);
		}

		public override string LookupNamespace (string prefix)
		{
			return Current.LookupNamespace (prefix);
		}

		public override void MoveToAttribute (int i)
		{
			if (entity != null && entity_inside_attr) {
				entity.Close ();
				entity = null;
			}
			Current.MoveToAttribute (i);
			inside_attr = true;
		}

		public override bool MoveToAttribute (string name)
		{
			if (entity != null && !entity_inside_attr)
				return entity.MoveToAttribute (name);
			if (!source.MoveToAttribute (name))
				return false;
			if (entity != null && entity_inside_attr) {
				entity.Close ();
				entity = null;
			}
			inside_attr = true;
			return true;
		}

		public override bool MoveToAttribute (string localName, string namespaceName)
		{
			if (entity != null && !entity_inside_attr)
				return entity.MoveToAttribute (localName, namespaceName);
			if (!source.MoveToAttribute (localName, namespaceName))
				return false;
			if (entity != null && entity_inside_attr) {
				entity.Close ();
				entity = null;
			}
			inside_attr = true;
			return true;
		}

		public override bool MoveToElement ()
		{
			if (entity != null && entity_inside_attr) {
				entity.Close ();
				entity = null;
			}
			if (!Current.MoveToElement ())
				return false;
			inside_attr = false;
			return true;
		}

		public override bool MoveToFirstAttribute ()
		{
			if (entity != null && !entity_inside_attr)
				return entity.MoveToFirstAttribute ();
			if (!source.MoveToFirstAttribute ())
				return false;
			if (entity != null && entity_inside_attr) {
				entity.Close ();
				entity = null;
			}
			inside_attr = true;
			return true;
		}

		public override bool MoveToNextAttribute ()
		{
			if (entity != null && !entity_inside_attr)
				return entity.MoveToNextAttribute ();
			if (!source.MoveToNextAttribute ())
				return false;
			if (entity != null && entity_inside_attr) {
				entity.Close ();
				entity = null;
			}
			inside_attr = true;
			return true;
		}

		public override bool Read ()
		{
			if (do_resolve) {
				DoResolveEntity ();
				do_resolve = false;
			}

			inside_attr = false;

			if (entity != null && (entity_inside_attr || entity.EOF)) {
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
			if (entity != null && entity_inside_attr) {
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

		public override void ResolveEntity ()
		{
			DoResolveEntity ();
		}

		void DoResolveEntity ()
		{
			if (entity != null)
				entity.ResolveEntity ();
			else {
				if (source.NodeType != XmlNodeType.EntityReference)
					throw new InvalidOperationException ("The current node is not an Entity Reference");
				if (ParserContext.Dtd == null)
					throw new XmlException (this as IXmlLineInfo, this.BaseURI, String.Format ("Cannot resolve entity without DTD: '{0}'", source.Name));
				XmlReader entReader = ParserContext.Dtd.GenerateEntityContentReader (
					source.Name, ParserContext);
				if (entReader == null)
					throw new XmlException (this as IXmlLineInfo, this.BaseURI, String.Format ("Reference to undeclared entity '{0}'.", source.Name));

				entity = new EntityResolvingXmlReader (
					entReader, inside_attr);
				entity.CopyProperties (this);
			}
		}

		public override void Skip ()
		{
			base.Skip ();
		}

		public bool HasLineInfo ()
		{
			IXmlLineInfo li = Current as IXmlLineInfo;
			return li == null ? false : li.HasLineInfo ();
		}

		#endregion
	}
}
