//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Markup;
using System.Xaml.Schema;
using System.Xml;

/*

** Value output node type

When an object contains a member:
- it becomes an attribute when it contains a value.
- it becomes an element when it contains an object.
*/

namespace System.Xaml
{
	public class XamlXmlWriter : XamlWriter
	{
		public XamlXmlWriter (Stream stream, XamlSchemaContext schemaContext)
			: this (stream, schemaContext, null)
		{
		}
		
		public XamlXmlWriter (Stream stream, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
			: this (XmlWriter.Create (stream), schemaContext, null)
		{
		}
		
		public XamlXmlWriter (TextWriter textWriter, XamlSchemaContext schemaContext)
			: this (XmlWriter.Create (textWriter), schemaContext, null)
		{
		}
		
		public XamlXmlWriter (TextWriter textWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
			: this (XmlWriter.Create (textWriter), schemaContext, null)
		{
		}
		
		public XamlXmlWriter (XmlWriter xmlWriter, XamlSchemaContext schemaContext)
			: this (xmlWriter, schemaContext, null)
		{
		}
		
		public XamlXmlWriter (XmlWriter xmlWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
		{
			if (xmlWriter == null)
				throw new ArgumentNullException ("xmlWriter");
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");
			this.w = xmlWriter;
			this.sctx = schemaContext;
			this.settings = settings ?? new XamlXmlWriterSettings ();
			this.manager = new XamlWriterStateManager<XamlXmlWriterException, InvalidOperationException> (true);
		}

		XmlWriter w;
		XamlSchemaContext sctx;
		XamlXmlWriterSettings settings;

		XamlWriterStateManager manager;

		Stack<object> nodes = new Stack<object> ();
		bool is_first_member_content, member_had_namespaces;
		object first_member_value;

		public override XamlSchemaContext SchemaContext {
			get { return sctx; }
		}

		public XamlXmlWriterSettings Settings {
			get { return settings; }
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposing)
				return;

			while (nodes.Count > 0) {
				var obj = nodes.Peek ();
				if (obj is XamlMember) {
					manager.OnClosingItem ();
					WriteEndMember ();
				}
				else if (obj is XamlType)
					WriteEndObject ();
				else
					nodes.Pop ();
			}
			if (settings.CloseOutput)
				w.Close ();
		}

		public void Flush ()
		{
			w.Flush ();
		}

		public override void WriteEndMember ()
		{
			manager.EndMember ();
			WriteStackedStartMember (XamlNodeType.EndMember);
			DoEndMember ();
			member_had_namespaces = false;
		}

		public override void WriteEndObject ()
		{
			manager.EndObject (nodes.Count > 1);
			WritePendingNamespaces ();
			w.WriteEndElement ();
			nodes.Pop ();
		}

		public override void WriteGetObject ()
		{
			manager.GetObject ();
			WriteStackedStartMember (XamlNodeType.GetObject);

			var xm = (XamlMember) GetNonNamespaceNode ();
			if (!xm.Type.IsCollection)
				throw new InvalidOperationException (String.Format ("WriteGetObject method can be invoked only when current member '{0}' is of collection type", xm.Name));

			DoEndMember ();
			
			// FIXME: it likely has to write the "retrieved" object here.
		}

		public override void WriteNamespace (NamespaceDeclaration namespaceDeclaration)
		{
			if (namespaceDeclaration == null)
				throw new ArgumentNullException ("namespaceDeclaration");

			manager.Namespace ();

			nodes.Push (namespaceDeclaration);
		}

		public override void WriteStartMember (XamlMember property)
		{
			if (property == null)
				throw new ArgumentNullException ("property");

			if (manager.HasNamespaces)
				member_had_namespaces = true;

			manager.StartMember ();
			nodes.Push (property);

			is_first_member_content = true;
		}

		public override void WriteStartObject (XamlType xamlType)
		{
			if (xamlType == null)
				throw new ArgumentNullException ("xamlType");

			manager.StartObject ();

			WriteStackedStartMember (XamlNodeType.StartObject);

			nodes.Push (xamlType);
			DoWriteStartObject (xamlType);
		}
		
		public override void WriteValue (object value)
		{
			if (value != null && !(value is string))
				throw new ArgumentException ("Non-string value cannot be written.");

			manager.Value ();

			var xt = value != null ? SchemaContext.GetXamlType (value.GetType ()) : XamlLanguage.Null;
			
			var xm = GetNonNamespaceNode () as XamlMember;
			if (xm == XamlLanguage.Initialization) {
				// do not reject type mismatch, as the value will be a string.
			}
			//else if (xt != null && xt.UnderlyingType != null && !xt.UnderlyingType.IsInstanceOfType (value))
			//	throw new ArgumentException (String.Format ("Value is not of type {0} but {1}", xt, value != null ? value.GetType ().FullName : "(null)"));

			if (!is_first_member_content) {
				WriteStackedStartMember (XamlNodeType.Value);
				DoWriteValue (value, IsAttribute (xt, xm));
			}
			else
				first_member_value = value;
		}

		void DoEndMember ()
		{
			var xm = nodes.Pop (); // XamlMember
			if (xm == XamlLanguage.Initialization) {
				// do nothing
			}
			else if (xm == XamlLanguage.PositionalParameters)
				throw new NotImplementedException ();
			else {
				switch (w.WriteState) {
				case WriteState.Content:
				case WriteState.Element:
					WritePendingNamespaces ();
					w.WriteEndElement ();
					break;
				default:
					w.WriteEndAttribute ();
					break;
				}
			}

			is_first_member_content = false;
			first_member_value = null;
		}

		bool IsAttribute (XamlType xt, XamlMember xm)
		{
			if (xm == XamlLanguage.Initialization)
				return false;
			if (w.WriteState == WriteState.Content)
				return false;
			var xd = xm as XamlDirective;
			if (xd != null && (xd.AllowedLocation & AllowedMemberLocations.Attribute) == 0)
				return false;
			if (xm.TypeConverter != null && xm.TypeConverter.ConverterInstance.CanConvertTo (typeof (string)))
				return true;
			return false;
		}

		void WriteStackedStartMember (XamlNodeType next)
		{
			if (!is_first_member_content)
				return;

			var xm = GetNonNamespaceNode () as XamlMember;
			if (xm == null)
				return;

			bool isAttr = false;
			if (xm == XamlLanguage.Initialization) {
				// do nothing
			}
			else if (xm == XamlLanguage.PositionalParameters)
				throw new NotImplementedException ();
			else if (member_had_namespaces || next == XamlNodeType.StartObject || !IsAttribute (GetCurrentType (false), xm))
				DoWriteStartMemberElement (xm);
			else {
				isAttr = true;
				DoWriteStartMemberAttribute (xm);
			}
			if (first_member_value != null)
				DoWriteValue (first_member_value, isAttr);
			is_first_member_content = false;
		}

		void DoWriteStartObject (XamlType xamlType)
		{
			string prefix = GetPrefix (xamlType.PreferredXamlNamespace);
			w.WriteStartElement (prefix, xamlType.InternalXmlName, xamlType.PreferredXamlNamespace);
			WritePendingNamespaces ();
		}
		
		void DoWriteStartMemberElement (XamlMember xm)
		{
			var xt = GetCurrentType ();
			string prefix = GetPrefix (xm.PreferredXamlNamespace);
			string name = xm.IsDirective ? xm.Name : String.Concat (xt.Name, ".", xm.Name);
			w.WriteStartElement (prefix, name, xm.PreferredXamlNamespace);
			WritePendingNamespaces ();
		}
		
		void DoWriteStartMemberAttribute (XamlMember xm)
		{
			var xt = GetCurrentType ();
			if (xt.PreferredXamlNamespace == xm.PreferredXamlNamespace)
				w.WriteStartAttribute (xm.Name);
			else {
				string prefix = GetPrefix (xm.PreferredXamlNamespace);
				w.WriteStartAttribute (prefix, xm.Name, xm.PreferredXamlNamespace);
			}
		}

		void DoWriteValue (object value, bool isAttr)
		{
			var xt = value == null ? XamlLanguage.Null : SchemaContext.GetXamlType (value.GetType ());

			var vs = xt.TypeConverter;
			var c = vs != null ? vs.ConverterInstance : null;
			if (!isAttr)
				WritePendingNamespaces ();
			if (c != null && c.CanConvertTo (typeof (string)))
				w.WriteString (c.ConvertToInvariantString (value));
			else
				w.WriteValue (value);
		}

		object GetNonNamespaceNode ()
		{
			if (nodes.Count == 0)
				return null;
			var obj = nodes.Pop ();
			try {
				if (obj is NamespaceDeclaration)
					return GetNonNamespaceNode ();
				else
					return obj;
			} finally {
				nodes.Push (obj);
			}
		}
		
		XamlType GetCurrentType ()
		{
			return GetCurrentType (false);
		}

		XamlType GetCurrentType (bool alsoSearchMemberType)
		{
			if (nodes.Count == 0)
				return null;
			var obj = nodes.Pop ();
			try {
				if (obj is XamlType)
					return (XamlType) obj;
				else if (alsoSearchMemberType && obj is XamlMember)
					return ((XamlMember) obj).Type;
				else
					return GetCurrentType (alsoSearchMemberType);
			} finally {
				nodes.Push (obj);
			}
		}

		string GetPrefix (string ns)
		{
			var decl = nodes.LastOrDefault (d => d is NamespaceDeclaration && ((NamespaceDeclaration) d).Namespace == ns) as NamespaceDeclaration;
			if (decl != null)
				return decl.Prefix;
			return w.LookupPrefix (ns);
		}

		Stack<NamespaceDeclaration> tmp_nss = new Stack<NamespaceDeclaration> ();

		void WritePendingNamespaces ()
		{
			if (w.WriteState != WriteState.Element)
				return;

			// write namespace that are put *before* current item.

			var top = nodes.Pop (); // temporarily pop out

			while (nodes.Count > 0) {
				var obj = nodes.Pop ();
				var nd = obj as NamespaceDeclaration;
				if (nd == null) {
					nodes.Push (obj);
					break;
				}
				tmp_nss.Push (nd);
			}
			while (tmp_nss.Count > 0) {
				var nd = tmp_nss.Pop ();
				DoWriteNamespace (nd);
			}
			manager.NamespaceCleanedUp ();

			nodes.Push (top); // push back
		}

		void DoWriteNamespace (NamespaceDeclaration nd)
		{
			if (String.IsNullOrEmpty (nd.Prefix))
				w.WriteAttributeString ("xmlns", nd.Namespace);
			else
				w.WriteAttributeString ("xmlns", nd.Prefix, XamlLanguage.Xmlns2000Namespace, nd.Namespace);
		}
	}
}
