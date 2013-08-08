//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Xml.Linq
{
	[XmlSchemaProvider (null, IsAny = true)]
	public class XElement : XContainer, IXmlSerializable
	{
		static IEnumerable <XElement> emptySequence =
			new List <XElement> ();

		public static IEnumerable <XElement> EmptySequence {
			get { return emptySequence; }
		}

		XName name;
		XAttribute attr_first, attr_last;
		bool explicit_is_empty = true;

		public XElement (XName name, object content)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			this.name = name;
			Add (content);
		}

		public XElement (XElement other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			name = other.name;
			Add (other.Attributes ());
			Add (other.Nodes ());
		}

		public XElement (XName name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			this.name = name;
		}

		public XElement (XName name, params object [] content)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			this.name = name;
			Add (content);
		}

		public XElement (XStreamingElement other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			this.name = other.Name;
			Add (other.Contents);
		}

		[CLSCompliant (false)]
		public static explicit operator bool (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return XUtil.ConvertToBoolean (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator bool? (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value == null ? (bool?) null : XUtil.ConvertToBoolean (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator DateTime (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return XUtil.ToDateTime (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator DateTime? (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value == null ? (DateTime?) null : XUtil.ToDateTime (element.Value);
		}

#if !TARGET_JVM // Same as for System.Xml.XmlConvert.ToDateTimeOffset

		[CLSCompliant (false)]
		public static explicit operator DateTimeOffset (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return XmlConvert.ToDateTimeOffset (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator DateTimeOffset? (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value == null ? (DateTimeOffset?) null : XmlConvert.ToDateTimeOffset (element.Value);
		}

#endif

		[CLSCompliant (false)]
		public static explicit operator decimal (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return XmlConvert.ToDecimal (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator decimal? (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value == null ? (decimal?) null : XmlConvert.ToDecimal (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator double (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return XmlConvert.ToDouble (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator double? (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value == null ? (double?) null : XmlConvert.ToDouble (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator float (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return XmlConvert.ToSingle (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator float? (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value == null ? (float?) null : XmlConvert.ToSingle (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator Guid (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return XmlConvert.ToGuid (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator Guid? (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value == null ? (Guid?) null : XmlConvert.ToGuid (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator int (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return XmlConvert.ToInt32 (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator int? (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value == null ? (int?) null : XmlConvert.ToInt32 (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator long (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return XmlConvert.ToInt64 (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator long? (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value == null ? (long?) null : XmlConvert.ToInt64 (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator uint (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return XmlConvert.ToUInt32 (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator uint? (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value == null ? (uint?) null : XmlConvert.ToUInt32 (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator ulong (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return XmlConvert.ToUInt64 (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator ulong? (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value == null ? (ulong?) null : XmlConvert.ToUInt64 (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator TimeSpan (XElement element)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			return XmlConvert.ToTimeSpan (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator TimeSpan? (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value == null ? (TimeSpan?) null : XmlConvert.ToTimeSpan (element.Value);
		}

		[CLSCompliant (false)]
		public static explicit operator string (XElement element)
		{
			if (element == null)
				return null;
			
			return element.Value;
		}

		public XAttribute FirstAttribute {
			get { return attr_first; }
			internal set { attr_first = value; }
		}

		public XAttribute LastAttribute {
			get { return attr_last; }
			internal set { attr_last = value; }
		}

		public bool HasAttributes {
			get { return attr_first != null; }
		}

		public bool HasElements {
			get {
				foreach (object o in Nodes ())
					if (o is XElement)
						return true;
				return false;
			}
		}

		public bool IsEmpty {
			get { return !Nodes ().GetEnumerator ().MoveNext () && explicit_is_empty; }
			internal set { explicit_is_empty = value; }
		}

		public XName Name {
			get { return name; }
			set {
				if (value == null)
					throw new ArgumentNullException ("Name");
				OnNameChanging (this);
				name = value;
				OnNameChanged (this);
			}
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Element; }
		}

		public string Value {
			get {
				StringBuilder sb = null;
				foreach (XNode n in Nodes ()) {
					if (sb == null)
						sb = new StringBuilder ();
					if (n is XText)
						sb.Append (((XText) n).Value);
					else if (n is XElement)
						sb.Append (((XElement) n).Value);
				}
				return sb == null ? String.Empty : sb.ToString ();
			}
			set {
				RemoveNodes ();
				Add (value);
			}
		}

		IEnumerable <XElement> GetAncestorList (XName name, bool getMeIn)
		{
			List <XElement> list = new List <XElement> ();
			if (getMeIn)
				list.Add (this);
			for (XElement el = Parent as XElement; el != null; el = el.Parent as XElement)
				if (name == null || el.Name == name)
					list.Add (el);
			return list;
		}

		public XAttribute Attribute (XName name)
		{
			foreach (XAttribute a in Attributes ())
				if (a.Name == name)
					return a;
			return null;
		}

		public IEnumerable <XAttribute> Attributes ()
		{
			XAttribute next;
			for (XAttribute a = attr_first; a != null; a = next) {
				next = a.NextAttribute;
				yield return a;
			}
		}

		// huh?
		public IEnumerable <XAttribute> Attributes (XName name)
		{
			foreach (XAttribute a in Attributes ())
				if (a.Name == name)
					yield return a;
		}

		static void DefineDefaultSettings (XmlReaderSettings settings, LoadOptions options)
		{
			settings.ProhibitDtd = false;

			settings.IgnoreWhitespace = (options & LoadOptions.PreserveWhitespace) == 0;
		}

		static XmlReaderSettings CreateDefaultSettings (LoadOptions options)
		{
			var settings = new XmlReaderSettings ();
			DefineDefaultSettings (settings, options);
			return settings;
		}

		public static XElement Load (string uri)
		{
			return Load (uri, LoadOptions.None);
		}

		public static XElement Load (string uri, LoadOptions options)
		{
			XmlReaderSettings s = CreateDefaultSettings (options);

			using (XmlReader r = XmlReader.Create (uri, s)) {
				return LoadCore (r, options);
			}
		}

		public static XElement Load (TextReader textReader)
		{
			return Load (textReader, LoadOptions.None);
		}

		public static XElement Load (TextReader textReader, LoadOptions options)
		{
			XmlReaderSettings s = CreateDefaultSettings (options);

			using (XmlReader r = XmlReader.Create (textReader, s)) {
				return LoadCore (r, options);
			}
		}

		public static XElement Load (XmlReader reader)
		{
			return Load (reader, LoadOptions.None);
		}

		public static XElement Load (XmlReader reader, LoadOptions options)
		{
			XmlReaderSettings s = reader.Settings != null ? reader.Settings.Clone () : new XmlReaderSettings ();
			DefineDefaultSettings (s, options);

			using (XmlReader r = XmlReader.Create (reader, s)) {
				return LoadCore (r, options);
			}
		}

#if NET_4_0
		public static XElement Load (Stream stream)
		{
			return Load (stream, LoadOptions.None);
		}

		public static XElement Load (Stream stream, LoadOptions options)
		{
			XmlReaderSettings s = new XmlReaderSettings ();
			DefineDefaultSettings (s, options);

			using (XmlReader r = XmlReader.Create (stream, s)) {
				return LoadCore (r, options);
			}
		}
#endif

		internal static XElement LoadCore (XmlReader r, LoadOptions options)
		{
			r.MoveToContent ();
			if (r.NodeType != XmlNodeType.Element)
				throw new InvalidOperationException ("The XmlReader must be positioned at an element");
			XName name = XName.Get (r.LocalName, r.NamespaceURI);
			XElement e = new XElement (name);
			e.FillLineInfoAndBaseUri (r, options);

			if (r.MoveToFirstAttribute ()) {
				do {
					// not sure how current Orcas behavior makes sense here though ...
					if (r.LocalName == "xmlns" && r.NamespaceURI == XNamespace.Xmlns.NamespaceName)
						e.SetAttributeValue (XNamespace.None.GetName ("xmlns"), r.Value);
					else
						e.SetAttributeValue (XName.Get (r.LocalName, r.NamespaceURI), r.Value);
					e.LastAttribute.FillLineInfoAndBaseUri (r, options);
				} while (r.MoveToNextAttribute ());
				r.MoveToElement ();
			}
			if (!r.IsEmptyElement) {
				r.Read ();
				e.ReadContentFrom (r, options);
				r.ReadEndElement ();
				e.explicit_is_empty = false;
			} else {
				e.explicit_is_empty = true;
				r.Read ();
			}
			return e;
		}

		public static XElement Parse (string text)
		{
			return Parse (text, LoadOptions.None);
		}

		public static XElement Parse (string text, LoadOptions options)
		{
			return Load (new StringReader (text), options);
		}

		public void RemoveAll ()
		{
			RemoveAttributes ();
			RemoveNodes ();
		}

		public void RemoveAttributes ()
		{
			while (attr_first != null)
				attr_last.Remove ();
		}

		public void Save (string fileName)
		{
			Save (fileName, SaveOptions.None);
		}

		public void Save (string fileName, SaveOptions options)
		{
			XmlWriterSettings s = new XmlWriterSettings ();

			if ((options & SaveOptions.DisableFormatting) == SaveOptions.None)
				s.Indent = true;
#if NET_4_0
			if ((options & SaveOptions.OmitDuplicateNamespaces) == SaveOptions.OmitDuplicateNamespaces)
				s.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
#endif
			using (XmlWriter w = XmlWriter.Create (fileName, s)) {
				Save (w);
			}
		}

		public void Save (TextWriter textWriter)
		{
			Save (textWriter, SaveOptions.None);
		}

		public void Save (TextWriter textWriter, SaveOptions options)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			
			if ((options & SaveOptions.DisableFormatting) == SaveOptions.None)
				s.Indent = true;
#if NET_4_0
			if ((options & SaveOptions.OmitDuplicateNamespaces) == SaveOptions.OmitDuplicateNamespaces)
				s.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
#endif
			using (XmlWriter w = XmlWriter.Create (textWriter, s)) {
				Save (w);
			}
		}

		public void Save (XmlWriter writer)
		{
			WriteTo (writer);
		}

#if NET_4_0
		public void Save (Stream stream)
		{
			Save (stream, SaveOptions.None);
		}

		public void Save (Stream stream, SaveOptions options)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			if ((options & SaveOptions.DisableFormatting) == SaveOptions.None)
				s.Indent = true;
			if ((options & SaveOptions.OmitDuplicateNamespaces) == SaveOptions.OmitDuplicateNamespaces)
				s.NamespaceHandling |= NamespaceHandling.OmitDuplicates;

			using (var writer = XmlWriter.Create (stream, s)){
				Save (writer);
			}
		}
#endif
		public IEnumerable <XElement> AncestorsAndSelf ()
		{
			return GetAncestorList (null, true);
		}

		public IEnumerable <XElement> AncestorsAndSelf (XName name)
		{
			return GetAncestorList (name, true);
		}

		public IEnumerable <XElement> DescendantsAndSelf ()
		{
			List <XElement> list = new List <XElement> ();
			list.Add (this);
			list.AddRange (Descendants ());
			return list;
		}

		public IEnumerable <XElement> DescendantsAndSelf (XName name)
		{
			List <XElement> list = new List <XElement> ();
			if (name == this.name)
				list.Add (this);
			list.AddRange (Descendants (name));
			return list;
		}

		public IEnumerable <XNode> DescendantNodesAndSelf ()
		{
			yield return this;
			foreach (XNode node in DescendantNodes ())
				yield return node;
		}

		public void SetAttributeValue (XName name, object value)
		{
			XAttribute a = Attribute (name);
			if (value == null) {
				if (a != null)
					a.Remove ();
			} else {
				if (a == null) {
					SetAttributeObject (new XAttribute (name, value));
				}
				else
					a.Value = XUtil.ToString (value);
			}
		}

		void SetAttributeObject (XAttribute a)
		{
			OnAddingObject (a);
			a = (XAttribute) XUtil.GetDetachedObject (a);
			a.SetOwner (this);
			if (attr_first == null) {
				attr_first = a;
				attr_last = a;
			} else {
				attr_last.NextAttribute = a;
				a.PreviousAttribute = attr_last;
				attr_last = a;
			}
			OnAddedObject (a);
		}

		string LookupPrefix (string ns, XmlWriter w)
		{
			string prefix = ns.Length > 0 ? GetPrefixOfNamespace (ns) ?? w.LookupPrefix (ns) : String.Empty;
			foreach (XAttribute a in Attributes ()) {
				if (a.IsNamespaceDeclaration && a.Value == ns) {
					if (a.Name.Namespace == XNamespace.Xmlns)
						prefix = a.Name.LocalName;
					// otherwise xmlns="..."
					break;
				}
			}
			return prefix;
		}
		
		static string CreateDummyNamespace (ref int createdNS, IEnumerable<XAttribute> atts, bool isAttr)
		{
			if (!isAttr && atts.All (a => a.Name.LocalName != "xmlns" || a.Name.NamespaceName == XNamespace.Xmlns.NamespaceName))
				return String.Empty;
			string p = null;
			do {
				p = "p" + (++createdNS);
				// check conflict
				if (atts.All (a => a.Name.LocalName != p || a.Name.NamespaceName == XNamespace.Xmlns.NamespaceName))
					break;
			} while (true);
			return p;
		}

		public override void WriteTo (XmlWriter writer)
		{
			// some people expect the same prefix output as in input,
			// in the loss of performance... see bug #466423.
			string prefix = LookupPrefix (name.NamespaceName, writer);
			int createdNS = 0;
			if (prefix == null)
				prefix = CreateDummyNamespace (ref createdNS, Attributes (), false);

			writer.WriteStartElement (prefix, name.LocalName, name.Namespace.NamespaceName);

			foreach (XAttribute a in Attributes ()) {
				if (a.IsNamespaceDeclaration) {
					if (a.Name.Namespace == XNamespace.Xmlns)
						writer.WriteAttributeString ("xmlns", a.Name.LocalName, XNamespace.Xmlns.NamespaceName, a.Value);
					else
						writer.WriteAttributeString ("xmlns", a.Value);
				} else {
					string apfix = LookupPrefix (a.Name.NamespaceName, writer);
					if (apfix == null)
						apfix = CreateDummyNamespace (ref createdNS, Attributes (), true);
					writer.WriteAttributeString (apfix, a.Name.LocalName, a.Name.Namespace.NamespaceName, a.Value);
				}
			}

			foreach (XNode node in Nodes ())
				node.WriteTo (writer);

			if (explicit_is_empty)
				writer.WriteEndElement ();
			else
				writer.WriteFullEndElement ();
		}

		public XNamespace GetDefaultNamespace ()
		{
			for (XElement el = this; el != null; el = el.Parent)
				foreach (XAttribute a in el.Attributes ())
					if (a.IsNamespaceDeclaration && a.Name.Namespace == XNamespace.None)
						return XNamespace.Get (a.Value);
			return XNamespace.None; // nothing is declared.
		}

		public XNamespace GetNamespaceOfPrefix (string prefix)
		{
			for (XElement el = this; el != null; el = el.Parent)
				foreach (XAttribute a in el.Attributes ())
					if (a.IsNamespaceDeclaration && (prefix.Length == 0 && a.Name.LocalName == "xmlns" || a.Name.LocalName == prefix))
						return XNamespace.Get (a.Value);
			return XNamespace.None; // nothing is declared.
		}

		public string GetPrefixOfNamespace (XNamespace ns)
		{
			foreach (string prefix in GetPrefixOfNamespaceCore (ns))
				if (GetNamespaceOfPrefix (prefix) == ns)
					return prefix;
			return null; // nothing is declared
		}
		
		IEnumerable<string> GetPrefixOfNamespaceCore (XNamespace ns)
		{
			for (XElement el = this; el != null; el = el.Parent)
				foreach (XAttribute a in el.Attributes ())
					if (a.IsNamespaceDeclaration && a.Value == ns.NamespaceName)
						yield return a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName;
		}

		public void ReplaceAll (object content)
		{
			// it's waste of resource, but from bug #11298 it must save content
			// snapshot first and then remove existing attributes.
			ReplaceAll (XUtil.ExpandArray (content).ToArray ());
		}

		public void ReplaceAll (params object [] content)
		{
			RemoveNodes ();
			Add (content);
		}

		public void ReplaceAttributes (object content)
		{
			// it's waste of resource, but from bug #11298 it must save content
			// snapshot first and then remove existing attributes.
			ReplaceAttributes (XUtil.ExpandArray (content).ToArray ());
		}

		public void ReplaceAttributes (params object [] content)
		{
			RemoveAttributes ();
			Add (content);
		}

		public void SetElementValue (XName name, object value)
		{
			var element = Element (name);
			if (element == null) {
				if (value != null)
					Add (new XElement (name, value));
			} else if (element != null && value == null) {
				element.Remove ();
			} else
				element.SetValue (value);
		}

		public void SetValue (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (value is XAttribute || value is XDocument || value is XDeclaration || value is XDocumentType)
				throw new ArgumentException (String.Format ("Node type {0} is not allowed as element value", value.GetType ()));
			RemoveNodes ();
			foreach (object o in XUtil.ExpandArray (value))
				Add (o);
		}

		internal override bool OnAddingObject (object o, bool rejectAttribute, XNode refNode, bool addFirst)
		{
			if (o is XDocument || o is XDocumentType || o is XDeclaration || (rejectAttribute && o is XAttribute))
				throw new ArgumentException (String.Format ("A node of type {0} cannot be added as a content", o.GetType ()));

			XAttribute a = o as XAttribute;
			if (a != null) {
				foreach (XAttribute ia in Attributes ())
					if (a.Name == ia.Name)
						throw new InvalidOperationException (String.Format ("Duplicate attribute: {0}", a.Name));
				SetAttributeObject (a);
				return true;
			}
			else if (o is string && refNode is XText) {
				((XText) refNode).Value += o as string;
				return true;
			}
			else
				return false;
		}

		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			Save (writer);
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{
			ReadContentFrom (reader, LoadOptions.None);
		}

		XmlSchema IXmlSerializable.GetSchema ()
		{
			return null;
		}
	}
}
