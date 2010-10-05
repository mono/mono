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
using System.Xaml.Schema;

using Pair = System.Collections.Generic.KeyValuePair<System.Xaml.XamlMember,string>;

namespace System.Xaml
{
	// FIXME: is GetObject supported by this reader?
	public class XamlXmlReader : XamlReader, IXamlLineInfo
	{
		#region constructors

		public XamlXmlReader (Stream stream)
			: this (stream, (XamlXmlReaderSettings) null)
		{
		}

		public XamlXmlReader (string fileName)
			: this (fileName, (XamlXmlReaderSettings) null)
		{
		}

		public XamlXmlReader (TextReader textReader)
			: this (textReader, (XamlXmlReaderSettings) null)
		{
		}

		public XamlXmlReader (XmlReader xmlReader)
			: this (xmlReader, (XamlXmlReaderSettings) null)
		{
		}

		public XamlXmlReader (Stream stream, XamlSchemaContext schemaContext)
			: this (stream, schemaContext, null)
		{
		}

		public XamlXmlReader (Stream stream, XamlXmlReaderSettings settings)
			: this (stream, new XamlSchemaContext (null, null), settings)
		{
		}

		public XamlXmlReader (string fileName, XamlSchemaContext schemaContext)
			: this (fileName, schemaContext, null)
		{
		}

		public XamlXmlReader (string fileName, XamlXmlReaderSettings settings)
			: this (fileName, new XamlSchemaContext (null, null), settings)
		{
		}

		public XamlXmlReader (TextReader textReader, XamlSchemaContext schemaContext)
			: this (textReader, schemaContext, null)
		{
		}

		public XamlXmlReader (TextReader textReader, XamlXmlReaderSettings settings)
			: this (textReader, new XamlSchemaContext (null, null), settings)
		{
		}

		public XamlXmlReader (XmlReader xmlReader, XamlSchemaContext schemaContext)
			: this (xmlReader, schemaContext, null)
		{
		}

		public XamlXmlReader (XmlReader xmlReader, XamlXmlReaderSettings settings)
			: this (xmlReader, new XamlSchemaContext (null, null), settings)
		{
		}

		public XamlXmlReader (Stream stream, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
			: this (XmlReader.Create (stream), schemaContext, settings)
		{
		}

		static readonly XmlReaderSettings file_reader_settings = new XmlReaderSettings () { CloseInput =true };

		public XamlXmlReader (string fileName, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
			: this (XmlReader.Create (fileName, file_reader_settings), schemaContext, settings)
		{
		}

		public XamlXmlReader (TextReader textReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
			: this (XmlReader.Create (textReader), schemaContext, settings)
		{
		}

		public XamlXmlReader (XmlReader xmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
		{
			if (xmlReader == null)
				throw new ArgumentNullException ("xmlReader");
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");

			sctx = schemaContext;
			this.settings = settings ?? new XamlXmlReaderSettings ();

			// filter out some nodes.
			var xrs = new XmlReaderSettings () {
				CloseInput = this.settings.CloseInput,
				IgnoreComments = true,
				IgnoreProcessingInstructions = true,
				IgnoreWhitespace = true };

			r = XmlReader.Create (xmlReader, xrs);
			line_info = r as IXmlLineInfo;
			xaml_namespace_resolver = new NamespaceResolver (r as IXmlNamespaceResolver);
		}
		
		#endregion

		XmlReader r;
		IXmlLineInfo line_info;
		XamlSchemaContext sctx;
		XamlXmlReaderSettings settings;
		bool is_eof;
		XamlNodeType node_type;
		
		object current;
		bool inside_object_not_member, is_empty_object, is_empty_member;
		Stack<XamlType> types = new Stack<XamlType> ();
		XamlMember current_member;

		IEnumerator<Pair> stored_member_enumerator;
		IXamlNamespaceResolver xaml_namespace_resolver;

		// This is used for things like collection Items.
		Dictionary<int,Stack<XamlMember>> inecting_members = new Dictionary<int,Stack<XamlMember>> ();

		public bool HasLineInfo {
			get { return line_info != null && line_info.HasLineInfo (); }
		}

		public override bool IsEof {
			get { return is_eof; }
		}

		public int LineNumber {
			get { return line_info != null ? line_info.LineNumber : 0; }
		}

		public int LinePosition {
			get { return line_info != null ? line_info.LinePosition : 0; }
		}

		public override XamlMember Member {
			get { return current as XamlMember; }
		}
		public override NamespaceDeclaration Namespace {
			get { return current as NamespaceDeclaration; }
		}

		public override XamlNodeType NodeType {
			get { return node_type; }
		}

		public override XamlSchemaContext SchemaContext {
			get { return sctx; }
		}

		public override XamlType Type {
			get { return current as XamlType; }
		}

		public override object Value {
			get { return NodeType == XamlNodeType.Value ? current : null; }
		}

		public override bool Read ()
		{
			if (IsDisposed)
				throw new ObjectDisposedException ("reader");
			if (is_eof)
				return false;

			// check this before is_empty_* so that they aren't ignored.
			if (MoveToNextStoredMember ())
				return true;

			if (is_empty_object) {
				is_empty_object = false;
				ReadEndType ();
				return true;
			}
			if (is_empty_member) {
				is_empty_member = false;
				ReadEndMember ();
				return true;
			}

			bool attrIterated = false;
			if (r.NodeType == XmlNodeType.Attribute) {
				attrIterated = true;
				if (r.MoveToNextAttribute ())
					if (CheckNextNamespace ())
						return true;
			}

			if (!r.EOF)
				r.MoveToContent ();
			if (r.EOF) {
				is_eof = true;
				return false;
			}

			switch (r.NodeType) {
			case XmlNodeType.Element:

				// could be: StartObject, StartMember, optionally preceding NamespaceDeclarations
				if (!attrIterated && r.MoveToFirstAttribute ())
					if (CheckNextNamespace ())
						return true;
				r.MoveToElement ();
				
				if (inside_object_not_member) {
					if (!ReadExtraStartMember ())
						ReadStartMember ();
				}
				else
					ReadStartType ();
				return true;

			case XmlNodeType.EndElement:

				// could be: EndObject, EndMember
				if (inside_object_not_member)
					ReadEndType ();
				else {
					if (!ReadExtraEndMember ())
						ReadEndMember ();
				}
				return true;

			default:

				// could be: Value
				ReadValue ();
				return true;
			}
		}

		XamlMember GetExtraMember (XamlType xt)
		{
			if (xt == XamlLanguage.Array)
				return xt.GetMember ("Items");
			if (xt.IsCollection)
				return XamlLanguage.Items;
			return null;
		}

		bool ReadExtraStartMember ()
		{
			var xm = GetExtraMember (types.Peek ());
			if (xm != null) {
				inside_object_not_member = false;
				current = current_member = xm;
				node_type = XamlNodeType.StartMember;
				return true;
			}
			return false;
		}

		bool ReadExtraEndMember ()
		{
			var xm = GetExtraMember (types.Peek ());
			if (xm != null) {
				inside_object_not_member = true;
				node_type = XamlNodeType.EndMember;
				return true;
			}
			return false;
		}

		bool CheckNextNamespace ()
		{
			do {
				if (r.NamespaceURI == XamlLanguage.Xmlns2000Namespace) {
					current = new NamespaceDeclaration (r.Value, r.Prefix == "xmlns" ? r.LocalName : String.Empty);
					node_type = XamlNodeType.NamespaceDeclaration;
					return true;
				}
			} while (r.MoveToNextAttribute ());
			return false;
		}

		void ReadStartType ()
		{
			string name = r.LocalName;
			string ns = r.NamespaceURI;
			string typeArgNames = null;
			var members = new List<Pair> ();
			var atts = ProcessAttributes (members);

			if (!r.IsEmptyElement) {
				r.Read ();
				do {
					r.MoveToContent ();
					switch (r.NodeType) {
					case XmlNodeType.Element:
					// FIXME: parse type arguments etc.
					case XmlNodeType.EndElement:
						break;
					default:
						// this value is for Initialization
						// FIXME: this could also be a WrappedContents
						members.Add (new Pair (XamlLanguage.Initialization, r.Value));
						r.Read ();
						continue;
					}
					break;
				} while (true);
			}
			else
				is_empty_object = true;

			// check TypeArguments to resolve Type, and remove them from the list. They don't appear as a node.
			var l = new List<Pair> ();
			foreach (var p in members) {
				if (p.Key == XamlLanguage.TypeArguments) {
					typeArgNames = p.Value;
					l.Add (p);
					break;
				}
			}
			foreach (var p in l)
				members.Remove (p);

			XamlType xt;
			IList<XamlTypeName> typeArgs = typeArgNames == null ? null : XamlTypeName.ParseList (typeArgNames, xaml_namespace_resolver);
			Type rtype = XamlLanguage.ResolveXamlTypeName (ns, name, typeArgs, xaml_namespace_resolver);
			if (rtype != null)
				xt = sctx.GetXamlType (rtype);
			else
				xt = sctx.GetXamlType (new XamlTypeName (ns, name, typeArgs));
			if (xt == null)
				// creates name-only XamlType. Also, it does not seem that it does not store this XamlType to XamlSchemaContext (Try GetXamlType(xtn) after reading such xaml node, it will return null).
				xt = new XamlType (ns, name, typeArgs == null ? null : typeArgs.Select<XamlTypeName,XamlType> (xtn => sctx.GetXamlType (xtn)).ToArray (), sctx);
			types.Push (xt);
			current = xt;

			foreach (var p in atts) {
				var xm = xt.GetMember (p.Key);
				if (xm != null)
					members.Add (new Pair (xm, p.Value));
				// ignore unknown attribute
			}

			node_type = XamlNodeType.StartObject;
			inside_object_not_member = true;

			// The next Read() results are likely directives.
			stored_member_enumerator = members.GetEnumerator ();
		}
		
		void ReadStartMember ()
		{
			var name = r.LocalName;
			int idx = name.IndexOf ('.');
			if (idx >= 0)
				name = name.Substring (idx + 1);

			var xt = types.Peek ();
			var xm = xt.GetMember (name);
			if (xm == null)
				// create unknown member.
				xm = new XamlMember (name, xt, false); // FIXME: not sure if isAttachable is always false.
			current = current_member = xm;

			node_type = XamlNodeType.StartMember;
			inside_object_not_member = false;
		}
		
		void ReadEndType ()
		{
			r.Read ();

			types.Pop ();
			current = null;
			node_type = XamlNodeType.EndObject;
			inside_object_not_member = false;
		}
		
		void ReadEndMember ()
		{
			r.Read ();

			current = current_member = null;
			node_type = XamlNodeType.EndMember;
			inside_object_not_member = true;
		}

		void ReadValue ()
		{
			// FIXME: (probably) use ValueSerializer to deserialize the value to the expected type.
			current = r.Value;

			r.Read ();

			node_type = XamlNodeType.Value;
		}

		// returns remaining attributes to be processed
		Dictionary<string,string> ProcessAttributes (List<Pair> members)
		{
			var l = members;

			// base
			string xmlbase = r.GetAttribute ("base", XamlLanguage.Xml1998Namespace) ?? r.BaseURI;
			if (types.Count == 0 && xmlbase != null) // top
				l.Add (new Pair (XamlLanguage.Base, xmlbase));

			var atts = new Dictionary<string,string> ();

			if (r.MoveToFirstAttribute ()) {
				do {
					if (r.NamespaceURI == XamlLanguage.Xmlns2000Namespace)
						continue;
					XamlDirective d = XamlLanguage.AllDirectives.FirstOrDefault (dd => (dd.AllowedLocation & AllowedMemberLocations.Attribute) != 0 && dd.Name == r.LocalName);
					if (d != null) {
						l.Add (new Pair (d, r.Value));
						continue;
					}
					if (r.NamespaceURI == String.Empty) {
						atts.Add (r.LocalName, r.Value);
						continue;
					}
					// Should we just ignore unknown attribute in XAML namespace or any other namespaces ?
					// Probably yes for compatibility with future version.
				} while (r.MoveToNextAttribute ());
				r.MoveToElement ();
			}
			return atts;
		}

		bool MoveToNextStoredMember ()
		{
			if (stored_member_enumerator != null) {
				// FIXME: value might have to be deserialized.
				switch (node_type) {
				case XamlNodeType.StartObject:
				case XamlNodeType.EndMember:
					// -> StartMember
					if (stored_member_enumerator.MoveNext ()) {
						current = current_member = stored_member_enumerator.Current.Key;
						node_type = XamlNodeType.StartMember;
						return true;
					}
					break;
				case XamlNodeType.StartMember:
					// -> Value
					current = stored_member_enumerator.Current.Value;
					node_type = XamlNodeType.Value;
					return true;
				case XamlNodeType.Value:
					// -> EndMember
					current = null;
					node_type = XamlNodeType.EndMember;
					return true;
				}
			}

			stored_member_enumerator = null;
			return false;
		}
		
		string GetLineString ()
		{
			return HasLineInfo ? String.Format (" Line {0}, at {1}", LineNumber, LinePosition) : String.Empty;
		}

		class NamespaceResolver : IXamlNamespaceResolver
		{
			IXmlNamespaceResolver source;

			public NamespaceResolver (IXmlNamespaceResolver source)
			{
				this.source = source;
			}

			public string GetNamespace (string prefix)
			{
				return source.LookupNamespace (prefix);
			}

			public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes ()
			{
				foreach (var p in source.GetNamespacesInScope (XmlNamespaceScope.All))
					yield return new NamespaceDeclaration (p.Value, p.Key);
			}
		}
	}
}
