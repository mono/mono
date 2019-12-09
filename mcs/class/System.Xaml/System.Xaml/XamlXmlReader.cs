//
// Copyright (C) 2011 Novell Inc. http://novell.com
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
			parser = new XamlXmlParser (xmlReader, schemaContext, settings);
		}
		
		#endregion

		XamlXmlParser parser;
		IEnumerator<XamlXmlNodeInfo> iter;

		public bool HasLineInfo {
			get { return iter != null ? iter.Current.HasLineInfo : false; }
		}

		public override bool IsEof {
			get { return iter != null ? iter.Current.NodeType == XamlNodeType.None : false; }
		}

		public int LineNumber {
			get { return iter != null ? iter.Current.LineNumber : 0; }
		}

		public int LinePosition {
			get { return iter != null ? iter.Current.LinePosition : 0; }
		}

		public override XamlMember Member {
			get { return iter != null && iter.Current.NodeType == XamlNodeType.StartMember ? (XamlMember) iter.Current.NodeValue : null; }
		}

		public override NamespaceDeclaration Namespace {
			get { return iter != null && iter.Current.NodeType == XamlNodeType.NamespaceDeclaration ? (NamespaceDeclaration) iter.Current.NodeValue : null; }
		}

		public override XamlNodeType NodeType {
			get { return iter != null ? iter.Current.NodeType : XamlNodeType.None; }
		}

		public override XamlSchemaContext SchemaContext {
			get { return parser.SchemaContext; }
		}

		public override XamlType Type {
			get { return iter != null && iter.Current.NodeType == XamlNodeType.StartObject ? (XamlType) iter.Current.NodeValue : null; }
		}

		public override object Value {
			get { return iter != null && iter.Current.NodeType == XamlNodeType.Value ? iter.Current.NodeValue : null; }
		}

		public override bool Read ()
		{
			if (IsDisposed)
				throw new ObjectDisposedException ("reader");
			if (iter == null)
				iter = parser.Parse ().GetEnumerator ();
			iter.MoveNext ();
			return iter.Current.NodeType != XamlNodeType.None;
		}
	}
	
	struct XamlXmlNodeInfo
	{
		public XamlXmlNodeInfo (XamlNodeType nodeType, object nodeValue, IXmlLineInfo lineInfo)
		{
			NodeType = nodeType;
			NodeValue = nodeValue;
			if (lineInfo != null && lineInfo.HasLineInfo ()) {
				HasLineInfo = true;
				LineNumber = lineInfo.LineNumber;
				LinePosition = lineInfo.LinePosition;
			} else {
				HasLineInfo = false;
				LineNumber = 0;
				LinePosition = 0;
			}
		}
		
		public bool HasLineInfo;
		public int LineNumber;
		public int LinePosition;
		public XamlNodeType NodeType;
		public object NodeValue;
	}
	
	class XamlXmlParser
	{
		public XamlXmlParser (XmlReader xmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
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
		
		XmlReader r;
		IXmlLineInfo line_info;
		XamlSchemaContext sctx;
		XamlXmlReaderSettings settings;
		IXamlNamespaceResolver xaml_namespace_resolver;

		internal XmlReader Reader {
			get { return r; }
		}

		public XamlSchemaContext SchemaContext {
			get { return sctx; }
		}

		XamlXmlNodeInfo Node (XamlNodeType nodeType, object nodeValue)
		{
			return new XamlXmlNodeInfo (nodeType, nodeValue, line_info);
		}

		public IEnumerable<XamlXmlNodeInfo> Parse ()
		{
			r.MoveToContent ();
			foreach (var xi in ReadObjectElement (null, null))
				yield return xi;
			yield return Node (XamlNodeType.None, null);
		}
		
		// Note that it could return invalid (None) node to tell the caller that it is not really an object element.
		IEnumerable<XamlXmlNodeInfo> ReadObjectElement (XamlType parentType, XamlMember currentMember)
		{
			if (r.NodeType != XmlNodeType.Element) {
				//throw new XamlParseException (String.Format ("Element is expected, but got {0}", r.NodeType));
				yield return Node (XamlNodeType.Value, r.Value);
				yield break;
			}

			if (r.MoveToFirstAttribute ()) {
				do {
					if (r.NamespaceURI == XamlLanguage.Xmlns2000Namespace)
						yield return Node (XamlNodeType.NamespaceDeclaration, new NamespaceDeclaration (r.Value, r.Prefix == "xmlns" ? r.LocalName : String.Empty));
				} while (r.MoveToNextAttribute ());
				r.MoveToElement ();
			}

			var sti = GetStartTagInfo ();

			var xt = sctx.GetXamlType (sti.TypeName);
			if (xt == null) {
				// Current element could be for another member in the parent type (if exists)
				if (parentType != null && (r.LocalName.IndexOf ('.') > 0 || parentType.GetMember (r.LocalName) != null)) {
					// stop the iteration and signal the caller to not read current element as an object. (It resolves conflicts between "start object for current collection's item" and "start member for the next member in the parent object".
					yield return Node (XamlNodeType.None, null);
					yield break;
				}

				// creates name-only XamlType. Also, it does not seem that it does not store this XamlType to XamlSchemaContext (Try GetXamlType(xtn) after reading such xaml node, it will return null).
				xt = new XamlType (sti.Namespace, sti.Name, sti.TypeName.TypeArguments == null ? null : sti.TypeName.TypeArguments.Select<XamlTypeName,XamlType> (xxtn => sctx.GetXamlType (xxtn)).ToArray (), sctx);
			}
			
			bool isGetObject = false;
			if (currentMember != null && !xt.CanAssignTo (currentMember.Type)) {
				if (currentMember.DeclaringType != null && currentMember.DeclaringType.ContentProperty == currentMember)
					isGetObject = true;

				// It could still be GetObject if current_member
				// is not a directive and current type is not
				// a markup extension.
				// (I'm not very sure about the condition;
				// it could be more complex.)
				// seealso: bug #682131
				else if (!(currentMember is XamlDirective) &&
				    !xt.IsMarkupExtension)
					isGetObject = true;
			}

			if (isGetObject) {
				yield return Node (XamlNodeType.GetObject, currentMember.Type);
				foreach (var ni in ReadMembers (parentType, currentMember.Type))
					yield return ni;
				yield return Node (XamlNodeType.EndObject, currentMember.Type);
				yield break;
			}
			// else

			yield return Node (XamlNodeType.StartObject, xt);

			// process attribute members (including MarkupExtensions)
			ProcessAttributesToMember (sctx, sti, xt);

			foreach (var pair in sti.Members) {
				yield return Node (XamlNodeType.StartMember, pair.Key);

				// Try markup extension
				// FIXME: is this rule correct?
				var v = pair.Value;
				if (!String.IsNullOrEmpty (v) && v [0] == '{') {
					if (v.Length > 1 && v [1] == '}') {
						//Escaped value
						yield return Node (XamlNodeType.Value, v.Substring (2));
					} else {
						var pai = ParsedMarkupExtensionInfo.Parse (v, xaml_namespace_resolver, sctx);
						yield return Node (XamlNodeType.StartObject, pai.Type);
						foreach (var xepair in pai.Arguments) {
							yield return Node (XamlNodeType.StartMember, xepair.Key);
							if (xepair.Value is List<string>)
								foreach (var s in (List<string>) xepair.Value)
									yield return Node (XamlNodeType.Value, s);
							else
								yield return Node (XamlNodeType.Value, xepair.Value);
							yield return Node (XamlNodeType.EndMember, xepair.Key);
						}
						yield return Node (XamlNodeType.EndObject, pai.Type);
					}
				}
				else
					yield return Node (XamlNodeType.Value, pair.Value);

				yield return Node (XamlNodeType.EndMember, pair.Key);
			}

			// process content members
			if (!r.IsEmptyElement) {
				r.Read ();
				foreach (var ni in ReadMembers (parentType, xt))
					yield return ni;
				r.ReadEndElement ();
			}
			else
				r.Read (); // consume empty element.

			yield return Node (XamlNodeType.EndObject, xt);
		}

		IEnumerable<XamlXmlNodeInfo> ReadMembers (XamlType parentType, XamlType xt)
		{
			for (r.MoveToContent (); r.NodeType != XmlNodeType.EndElement; r.MoveToContent ()) {
				switch (r.NodeType) {
				case XmlNodeType.Element:
					// FIXME: parse type arguments etc.
					foreach (var x in ReadMemberElement (parentType, xt)) {
						if (x.NodeType == XamlNodeType.None)
							yield break;
						yield return x;
					}
					continue;
				default:
					foreach (var x in ReadMemberText (xt))
						yield return x;
					continue;
				}
			}
		}

		StartTagInfo GetStartTagInfo ()
		{
			string name = r.LocalName;
			string ns = r.NamespaceURI;
			string typeArgNames = null;

			var members = new List<Pair> ();
			var atts = ProcessAttributes (r, members);

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

			IList<XamlTypeName> typeArgs = typeArgNames == null ? null : XamlTypeName.ParseList (typeArgNames, xaml_namespace_resolver);
			var xtn = new XamlTypeName (ns, name, typeArgs);
			return new StartTagInfo () { Name = name, Namespace = ns, TypeName = xtn, Members = members, Attributes = atts};
		}

		bool xmlbase_done;

		// returns remaining attributes to be processed
		Dictionary<string,string> ProcessAttributes (XmlReader r, List<Pair> members)
		{
			var l = members;

			// base (top element)
			if (!xmlbase_done) {
				xmlbase_done = true;
				string xmlbase = r.GetAttribute ("base", XamlLanguage.Xml1998Namespace) ?? r.BaseURI;
				if (xmlbase != null)
					l.Add (new Pair (XamlLanguage.Base, xmlbase));
			}

			var atts = new Dictionary<string,string> ();

			if (r.MoveToFirstAttribute ()) {
				do {
					switch (r.NamespaceURI) {
					case XamlLanguage.Xml1998Namespace:
						switch (r.LocalName) {
						case "base":
							continue; // already processed.
						case "lang":
							l.Add (new Pair (XamlLanguage.Lang, r.Value));
							continue;
						case "space":
							l.Add (new Pair (XamlLanguage.Space, r.Value));
							continue;
						}
						break;
					case XamlLanguage.Xmlns2000Namespace:
						continue;
					case XamlLanguage.Xaml2006Namespace:
						XamlDirective d = FindStandardDirective (r.LocalName, AllowedMemberLocations.Attribute);
						if (d != null) {
							l.Add (new Pair (d, r.Value));
							continue;
						}
						throw new NotSupportedException (String.Format ("Attribute '{0}' is not supported", r.Name));
					default:
						if (r.NamespaceURI == String.Empty) {
							atts.Add (r.Name, r.Value);
							continue;
						}
						// Should we just ignore unknown attribute in XAML namespace or any other namespaces ?
						// Probably yes for compatibility with future version.
						break;
					}
				} while (r.MoveToNextAttribute ());
				r.MoveToElement ();
			}
			return atts;
		}


		void ProcessAttributesToMember (XamlSchemaContext sctx, StartTagInfo sti, XamlType xt)
		{
			foreach (var p in sti.Attributes) {
				int idx = p.Key.IndexOf (':');
				string prefix = idx > 0 ? p.Key.Substring (0, idx) : String.Empty;
				string aname = idx > 0 ? p.Key.Substring (idx + 1) : p.Key;
				idx = aname.IndexOf ('.');
				if (idx > 0) {
					string apns = prefix.Length > 0 ? r.LookupNamespace (prefix) : r.NamespaceURI;
					var apname = aname.Substring (0, idx);
					var axtn = new XamlTypeName (apns, apname, null);
					var at = sctx.GetXamlType (axtn);
					var am = at.GetAttachableMember (aname.Substring (idx + 1));
					if (am != null)
						sti.Members.Add (new Pair (am, p.Value));
					// ignore unknown attribute
				}
				var xm = xt.GetMember (aname);
				if (xm != null)
					sti.Members.Add (new Pair (xm, p.Value));
				// ignore unknown attribute
			}
		}

		// returns an optional member without xml node.
		XamlMember GetExtraMember (XamlType xt)
		{
			if (xt.ContentProperty != null) // e.g. Array.Items
				return xt.ContentProperty;
			if (xt.IsCollection || xt.IsDictionary)
				return XamlLanguage.Items;
			return null;
		}

		static XamlDirective FindStandardDirective (string name, AllowedMemberLocations loc)
		{
			return XamlLanguage.AllDirectives.FirstOrDefault (dd => (dd.AllowedLocation & loc) != 0 && dd.Name == name);
		}

		IEnumerable<XamlXmlNodeInfo> ReadMemberText (XamlType xt)
		{
			// this value is for Initialization, or Content property value
			XamlMember xm;
			if (xt.ContentProperty != null)
				xm = xt.ContentProperty;
			else
				xm = XamlLanguage.Initialization;
			yield return Node (XamlNodeType.StartMember, xm);
			yield return Node (XamlNodeType.Value, r.Value);
			r.Read ();
			yield return Node (XamlNodeType.EndMember, xm);
		}
		
		// member element, implicit member, children via content property, or value
		IEnumerable<XamlXmlNodeInfo> ReadMemberElement (XamlType parentType, XamlType xt)
		{
			XamlMember xm = null;
			var name = r.LocalName;
			int idx = name.IndexOf ('.');
			// FIXME: it skips strict type name check, as it could result in MarkupExtension mismatch (could be still checked, though)
			if (idx >= 0/* && name.Substring (0, idx) == xt.Name*/) {
				name = name.Substring (idx + 1);
				xm = xt.GetMember (name);
			} else {
				xm = (XamlMember) FindStandardDirective (name, AllowedMemberLocations.MemberElement) ??
					// not a standard directive? then try attachable
					xt.GetAttachableMember (name) ??
					// still not? then try ordinal member
					xt.GetMember (name);
				if (xm == null) {
					// still not? could it be omitted as content property or items ?
					if ((xm = GetExtraMember (xt)) != null) {
						// Note that this does not involve r.Read()
						foreach (var ni in ReadMember (xt, xm))
							yield return ni;
						yield break;
					}
				}
			}
			if (xm == null) {
				// Current element could be for another member in the parent type (if exists)
				if (parentType != null && parentType.GetMember (name) != null) {
					// stop the iteration and signal the caller to not read current element as an object. (It resolves conflicts between "start object for current collection's item" and "start member for the next member in the parent object".
					yield return Node (XamlNodeType.None, null);
					yield break;
				}

				// ok, then create unknown member.
				xm = new XamlMember (name, xt, false); // FIXME: not sure if isAttachable is always false.
			}

			if (!r.IsEmptyElement) {
				r.Read ();
				foreach (var ni in ReadMember (xt, xm))
					yield return ni;
				r.MoveToContent ();
				r.ReadEndElement ();
			}
			else
				r.Read ();
		}

		IEnumerable<XamlXmlNodeInfo> ReadMember (XamlType parentType, XamlMember xm)
		{
			yield return Node (XamlNodeType.StartMember, xm);

			if (xm.IsEvent) {
				yield return Node (XamlNodeType.Value, r.Value);
				r.Read ();
			} else if (!xm.IsWritePublic) {
				if (xm.Type.IsXData)
					foreach (var ni in ReadXData ())
						yield return ni;
				else if (xm.Type.IsCollection) {
					yield return Node (XamlNodeType.GetObject, xm.Type);
					yield return Node (XamlNodeType.StartMember, XamlLanguage.Items);
					foreach (var ni in ReadCollectionItems (parentType, XamlLanguage.Items))
						yield return ni;
					yield return Node (XamlNodeType.EndMember, XamlLanguage.Items);
					yield return Node (XamlNodeType.EndObject, xm.Type);
				}
				else
					throw new XamlParseException (String.Format ("Read-only member '{0}' showed up in the source XML, and the xml contains element content that cannot be read.", xm.Name)) { LineNumber = this.LineNumber, LinePosition = this.LinePosition };
			} else {
				if (xm.Type.IsCollection || xm.Type.IsDictionary) {
					foreach (var ni in ReadCollectionItems (parentType, xm))
						yield return ni;
				}
				else
					foreach (var ni in ReadObjectElement (parentType, xm)) {
						if (ni.NodeType == XamlNodeType.None)
							throw new Exception ("should not happen");
						yield return ni;
					}
			}

			yield return Node (XamlNodeType.EndMember, xm);
		}

		IEnumerable<XamlXmlNodeInfo> ReadCollectionItems (XamlType parentType, XamlMember xm)
		{
			for (r.MoveToContent (); r.NodeType != XmlNodeType.EndElement; r.MoveToContent ()) {

				foreach (var ni in ReadObjectElement (parentType, xm)) {
					if (ni.NodeType == XamlNodeType.None)
						yield break;
					yield return ni;
				}
			}
		}
		
		IEnumerable<XamlXmlNodeInfo> ReadXData ()
		{
			var xt = XamlLanguage.XData;
			var xm = xt.GetMember ("Text");
			yield return Node (XamlNodeType.StartObject, xt);
			yield return Node (XamlNodeType.StartMember, xm);
			yield return Node (XamlNodeType.Value, r.ReadInnerXml ());
			yield return Node (XamlNodeType.EndMember, xm);
			yield return Node (XamlNodeType.EndObject, xt);
		}

		public int LineNumber {
			get { return line_info != null && line_info.HasLineInfo () ? line_info.LineNumber : 0; }
		}

		public int LinePosition {
			get { return line_info != null && line_info.HasLineInfo () ? line_info.LinePosition : 0; }
		}

		internal class StartTagInfo
		{
			public string Name;
			public string Namespace;
			public XamlTypeName TypeName;
			public List<Pair> Members;
			public Dictionary<string,string> Attributes;
		}
		
		internal class NamespaceResolver : IXamlNamespaceResolver
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
