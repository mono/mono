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
using System.IO;
using System.Linq;
using System.Xml;

/*

* State transition

Unlike XmlWriter, XAML nodes are not immediately writable because object
output has to be delayed to be determined whether it should write
an attribute or an element.

** Value output node type

When an object contains a member:
- it becomes an attribute when it contains a value.
- it becomes an element when it contains an object.

** NamespaceDeclarations

NamespaceDeclaration does not immediately participate in the state transition
but some write methods reject stored namespaces (e.g. WriteEndObject cannot
handle them). In such cases, they throw InvalidOperationException, while the
writer throws XamlXmlWriterException for usual state transition.

Though they still seems to affect some outputs. If a member with simple
value is written after a namespace, then it becomes an element, not attribute.

** state transition

states are: Initial, ObjectStarted, MemberStarted, ValueWritten, MemberDone, End

Initial + StartObject -> ObjectStarted : push(xt)
ObjectStarted + StartMember -> MemberStarted : push(xm)
ObjectStarted + EndObject -> ObjectWritten or End : pop()
MemberStarted + StartObject -> ObjectStarted : push(xt)
MemberStarted + Value -> ValueWritten
MemberStarted + GetObject -> MemberDone : pop()
ObjectWritten + StartObject -> ObjectStarted : push(x)
ObjectWritten + Value -> ValueWritten : pop()
ObjectWritten + EndMember -> MemberDone : pop()
ValueWritten + StartObject -> ObjectStarted : push(x)
ValueWritten + EndMember -> MemberDone : pop()
MemberDone + EndObject -> ObjectWritten or End : pop() // xt
MemberDone + StartMember -> MemberStarted : push(xm)


*/

namespace System.Xaml
{
	public class XamlXmlWriter : XamlWriter
	{
		enum XamlWriteState
		{
			Initial,
			ObjectStarted,
			MemberStarted,
			ObjectWritten,
			ValueWritten,
			MemberDone,
			End
		}

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
		}

		XmlWriter w;
		XamlSchemaContext sctx;
		XamlXmlWriterSettings settings;

		Stack<object> nodes = new Stack<object> ();
		XamlWriteState state = XamlWriteState.Initial;
		bool is_first_member_content, has_namespace;
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
					// somewhat hacky state change to not reject StartMember->EndMember.
					if (state == XamlWriteState.MemberStarted)
						state = XamlWriteState.ValueWritten;
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
			RejectNamespaces (XamlNodeType.EndMember);

			CheckState (XamlNodeType.EndMember);
			
			WriteStackedStartMember (XamlNodeType.EndMember);
			DoEndMember ();

			state = XamlWriteState.MemberDone;
		}

		public override void WriteEndObject ()
		{
			RejectNamespaces (XamlNodeType.EndObject);

			CheckState (XamlNodeType.EndObject);
			
			w.WriteEndElement ();
			nodes.Pop ();

			state = nodes.Count > 0 ? XamlWriteState.ObjectWritten : XamlWriteState.End;
		}

		public override void WriteGetObject ()
		{
			CheckState (XamlNodeType.GetObject);
			
			RejectNamespaces (XamlNodeType.GetObject);

			WriteStackedStartMember (XamlNodeType.GetObject);

			var xm = (XamlMember) GetNonNamespaceNode ();
			if (!xm.Type.IsCollection)
				throw new InvalidOperationException (String.Format ("WriteGetObject method can be invoked only when current member '{0}' is of collection type", xm.Name));

			DoEndMember ();

			state = XamlWriteState.MemberDone;
		}

		public override void WriteNamespace (NamespaceDeclaration namespaceDeclaration)
		{
			if (namespaceDeclaration == null)
				throw new ArgumentNullException ("namespaceDeclaration");

			nodes.Push (namespaceDeclaration);
			has_namespace = true;
		}

		public override void WriteStartMember (XamlMember property)
		{
			CheckState (XamlNodeType.StartMember);
			
			nodes.Push (property);

			state = XamlWriteState.MemberStarted;
			is_first_member_content = true;
		}

		public override void WriteStartObject (XamlType xamlType)
		{
			CheckState (XamlNodeType.StartObject);

			WriteStackedStartMember (XamlNodeType.StartObject);

			nodes.Push (xamlType);
			DoWriteStartObject (xamlType);

			state = XamlWriteState.ObjectStarted;
		}
		
		public override void WriteValue (object value)
		{
			CheckState (XamlNodeType.Value);
			RejectNamespaces (XamlNodeType.Value);

			var xt = GetCurrentType ();
			if (xt != null && xt.UnderlyingType != null && !xt.UnderlyingType.IsInstanceOfType (value))
				throw new ArgumentException (String.Format ("Value is not of type {0}", xt));

			if (!is_first_member_content) {
				WriteStackedStartMember (XamlNodeType.Value);
				DoWriteValue (value);
			}
			else
				first_member_value = value;

			state = XamlWriteState.ValueWritten;
		}

		void DoEndMember ()
		{
			if (w.WriteState == WriteState.Content)
				w.WriteEndElement ();
			else
				w.WriteEndAttribute ();

			nodes.Pop (); // XamlMember
			is_first_member_content = false;
			first_member_value = null;
		}

		void WriteStackedStartMember (XamlNodeType next)
		{
			if (!is_first_member_content)
				return;

			var xm = GetNonNamespaceNode () as XamlMember;
			if (xm == null)
				return;

			if (next == XamlNodeType.StartObject || w.WriteState == WriteState.Content || has_namespace)
				DoWriteStartMemberElement (xm);
			else
				DoWriteStartMemberAttribute (xm);
			if (first_member_value != null)
				DoWriteValue (first_member_value);
			is_first_member_content = false;
		}

		void DoWriteStartObject (XamlType xamlType)
		{
			string prefix = GetPrefix (xamlType.PreferredXamlNamespace);
			w.WriteStartElement (prefix, xamlType.Name, xamlType.PreferredXamlNamespace);
			WriteAndClearNamespaces ();
		}
		
		void DoWriteStartMemberElement (XamlMember xm)
		{
			var xt = GetCurrentType ();
			string prefix = GetPrefix (xm.PreferredXamlNamespace);
			w.WriteStartElement (prefix, String.Concat (xt.Name, ".", xm.Name), xm.PreferredXamlNamespace);
			WriteAndClearNamespaces ();
		}
		
		void DoWriteStartMemberAttribute (XamlMember xm)
		{
			WriteAndClearNamespaces ();
			
			var xt = GetCurrentType ();
			if (xt.PreferredXamlNamespace == xm.PreferredXamlNamespace)
				w.WriteStartAttribute (xm.Name);
			else {
				string prefix = GetPrefix (xm.PreferredXamlNamespace);
				w.WriteStartAttribute (prefix, xm.Name, xm.PreferredXamlNamespace);
			}
		}

		void DoWriteValue (object value)
		{
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
			if (nodes.Count == 0)
				return null;
			var obj = nodes.Pop ();
			try {
				if (obj is XamlType)
					return (XamlType) obj;
				else
					return GetCurrentType ();
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
		
		void RejectNamespaces (XamlNodeType next)
		{
			if (nodes != null && nodes.Count > 0 && nodes.Peek () is NamespaceDeclaration) {
				// strange, but on WriteEndMember it throws XamlXmlWriterException, while for other nodes it throws IOE.
				string msg = String.Format ("Namespace declarations cannot be written before {0}", next);
				if (next == XamlNodeType.EndMember)
					throw new XamlXmlWriterException (msg);
				else
					throw new InvalidOperationException (msg);
			}
		}

		Stack<NamespaceDeclaration> tmp_nss = new Stack<NamespaceDeclaration> ();

		void WriteAndClearNamespaces ()
		{
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
			has_namespace = false;

			nodes.Push (top); // push back
		}

		void DoWriteNamespace (NamespaceDeclaration nd)
		{
			if (String.IsNullOrEmpty (nd.Prefix))
				w.WriteAttributeString ("xmlns", nd.Namespace);
			else
				w.WriteAttributeString ("xmlns", nd.Prefix, XamlLanguage.Xmlns2000Namespace, nd.Namespace);
		}

		void CheckState (XamlNodeType next)
		{
			switch (state) {
			case XamlWriteState.Initial:
				switch (next) {
				case XamlNodeType.StartObject:
					return;
				}
				break;
			case XamlWriteState.ObjectStarted:
				switch (next) {
				case XamlNodeType.StartMember:
				case XamlNodeType.EndObject:
					return;
				}
				break;
			case XamlWriteState.MemberStarted:
				switch (next) {
				case XamlNodeType.StartObject:
				case XamlNodeType.Value:
				case XamlNodeType.GetObject:
					return;
				}
				break;
			case XamlWriteState.ObjectWritten:
				switch (next) {
				case XamlNodeType.StartObject:
				case XamlNodeType.Value:
				case XamlNodeType.EndMember:
					return;
				}
				break;
			case XamlWriteState.ValueWritten:
				switch (next) {
				case XamlNodeType.StartObject:
				case XamlNodeType.EndMember:
					return;
				}
				break;
			case XamlWriteState.MemberDone:
				switch (next) {
				case XamlNodeType.StartMember:
				case XamlNodeType.EndObject:
					return;
				}
				break;
			}
			throw new XamlXmlWriterException (String.Format ("{0} is not allowed at current state {1}", next, state));
		}
	}
}
