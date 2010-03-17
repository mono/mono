//
// XmlTextWriter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.

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
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

/*

This is a fresh implementation of XmlTextWriter since Mono 1.1.14.

Here are some implementation notes (mostly common to previous module):

- WriteProcessingInstruction() does not reject 'X' 'M' 'L'

	XmlWriter violates section 2.6 of W3C XML 1.0 specification (3rd. 
	edition) since it incorrectly allows such PI target that consists of
	case-insensitive sequence of 'X' - 'M' - 'L'. This is XmlWriter API
	design failure which does not provide perfect WriteStartDocument().

- XmlTextWriter does not escape trailing ']' in internal subset.

	The fact is as this subsection title shows. It means, to make an
	XmlWriter compatible with other XmlWriters, it should always escape
	the trailing ']' of the input, but XmlTextWriter runs no check.

- Prefix autogeneration for global attributes

	When an attribute has a non-empty namespace URI, the prefix must be
	non-empty string (since if the prefix is empty it is regarded as a
	local attribute). In such case, a dummy prefix must be created.

	Since attributes are written to TextWriter almost immediately, the
	same prefix might appear in the later attributes.

- Namespace context

	Namespace handling in XmlTextWriter is pretty nasty.

	First of all, if WriteStartElement() takes null namespaceURI, then
	the element has no explicit namespace and it is treated as if
	Namespaces property were set as false.

	Namespace context is structured by some writer methods:

	- WriteStartElement() : If it has a non-empty argument prefix, then
	  the new prefix is bound to the argument namespaceURI. If prefix
	  is "" and namespaceURI is not empty, then it consists of a
	  default namespace.

	- WriteStartAttribute() : there are two namespace provisions here:
	  1) like WriteStartElement() prefix and namespaceURI are not empty
	  2) prefix is "xmlns", or localName is "xmlns" and prefix is ""
	  If prefix is "" and namespaceURI is not empty, then the prefix is
	  "mocked up" (since an empty prefix is not possible for attributes).

	- WriteQualifiedName() : the argument name and namespaceURI creates
	  a new namespace mapping. Note that default namespace (prefix "")
	  is not constructed at the state of WriteState.Attribute.

	Note that WriteElementString() internally calls WriteStartElement()
	and WriteAttributeString() internally calls WriteStartAttribute().

	Sometimes those namespace outputs are in conflict. For example, if

		w.WriteStartElement ("p", "foo", "urn:foo");
		w.WriteStartAttribute ("xmlns", "p", "urn:bar");
		w.WriteEndElement ();

	urn:foo will be lost.

	Here are the rules:

	- If either prefix or localName is explicitly "xmlns" in
	  WriteStartAttribute(), it takes the highest precedence.
	- For WriteStartElement(), prefix is always preserved, but
	  namespaceURI context might not (because of the rule above).
	- For WriteStartAttribute(), prefix is preserved only if there is
	  no previous mapping in the local element. If it is in conflict,
	  a new prefix is "mocked up" like an empty prefix.

- DetermineAttributePrefix(): local mapping overwrite

	(do not change this section title unless you also change cross
	references in this file.)

	Even if the prefix is already mapped to another namespace, it might
	be overridable because the conflicting mapping might reside in one
	of the ancestors.

	To check it, we once try to remove existing mapping. If it is 
	successfully removed, then the mapping is locally added. In that
	case, we cannot override it, so mock another prefix up.


- Attribute value preservation

	Since xmlns and xml:* attributes are used to determine some public
	behaviors such as XmlLang, XmlSpace and LookupPrefix(), it must
	preserve what value is being written. At the same time, users might
	call WriteString(), WhiteEntityRef() etc. separately, in such cases
	we must preserve what is output to the stream.

	This preservation is done using a "temporary preservation buffer",
	the output Flush() behavior is different from MS. In such case that
	XmlTextWriter uses that buffer, it won't be write anything until
	XmlTextWriter.WriteEndAttribute() is called. If we implement it like
	MS, it results in meaningless performance loss (it is not something 
	people should expect. There is no solid behavior on when start tag 
	closing '>' is written).

*/


#if NET_1_1
namespace System.Xml
#else
namespace Mono.Xml
#endif
{
	public class XmlTextWriter : XmlWriter
	{
		// Static/constant members.

		const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
		const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";

		static readonly Encoding unmarked_utf8encoding =
			new UTF8Encoding (false, false);
		static char [] escaped_text_chars;
		static char [] escaped_attr_chars;

		// Internal classes

		class XmlNodeInfo
		{
			public string Prefix;
			public string LocalName;
			public string NS;
			public bool HasSimple;
			public bool HasElements;
			public string XmlLang;
			public XmlSpace XmlSpace;
		}

		internal class StringUtil
		{
			static CultureInfo cul = CultureInfo.InvariantCulture;
			static CompareInfo cmp =
				CultureInfo.InvariantCulture.CompareInfo;

			public static int IndexOf (string src, string target)
			{
				return cmp.IndexOf (src, target);
			}

			public static int Compare (string s1, string s2)
			{
				return cmp.Compare (s1, s2);
			}

			public static string Format (
				string format, params object [] args)
			{
				return String.Format (cul, format, args);
			}
		}

		enum XmlDeclState {
			Allow,
			Ignore,
			Auto,
			Prohibit,
		}

		// Instance fields

		Stream base_stream;
		TextWriter source; // the input TextWriter to .ctor().
		TextWriter writer;
		// It is used for storing xml:space, xml:lang and xmlns values.
		StringWriter preserver;
		string preserved_name;
		bool is_preserved_xmlns;

		bool allow_doc_fragment;
		bool close_output_stream = true;
		bool ignore_encoding;
		bool namespaces = true;
		XmlDeclState xmldecl_state = XmlDeclState.Allow;

		bool check_character_validity;
		NewLineHandling newline_handling = NewLineHandling.None;

		bool is_document_entity;
		WriteState state = WriteState.Start;
		XmlNodeType node_state = XmlNodeType.None;
		XmlNamespaceManager nsmanager;
		int open_count;
		XmlNodeInfo [] elements = new XmlNodeInfo [10];
		Stack new_local_namespaces = new Stack ();
		ArrayList explicit_nsdecls = new ArrayList ();
		NamespaceHandling namespace_handling;

		bool indent;
		int indent_count = 2;
		char indent_char = ' ';
		string indent_string = "  ";
		string newline;
		bool indent_attributes;

		char quote_char = '"';

		bool v2;

		// Constructors

		public XmlTextWriter (string filename, Encoding encoding)
			: this (new FileStream (filename, FileMode.Create, FileAccess.Write, FileShare.None), encoding)
		{
		}

		public XmlTextWriter (Stream stream, Encoding encoding)
			: this (new StreamWriter (stream,
				encoding == null ? unmarked_utf8encoding : encoding))
		{
			ignore_encoding = (encoding == null);
			Initialize (writer);
			allow_doc_fragment = true;
		}

		public XmlTextWriter (TextWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			ignore_encoding = (writer.Encoding == null);
			Initialize (writer);
			allow_doc_fragment = true;
		}

#if NET_2_0
		internal XmlTextWriter (
			TextWriter writer, XmlWriterSettings settings, bool closeOutput)
		{
			v2 = true;

			if (settings == null)
				settings = new XmlWriterSettings ();

			Initialize (writer);

			close_output_stream = closeOutput;
			allow_doc_fragment =
				settings.ConformanceLevel != ConformanceLevel.Document;
			switch (settings.ConformanceLevel) {
			case ConformanceLevel.Auto:
				xmldecl_state = settings.OmitXmlDeclaration ? XmlDeclState.Ignore : XmlDeclState.Allow;
				break;
			case ConformanceLevel.Document:
				// LAMESPEC:
				// On MSDN, XmlWriterSettings.OmitXmlDeclaration is documented as:
				// "The XML declaration is always written if
				//  ConformanceLevel is set to Document, even 
				//  if OmitXmlDeclaration is set to true. "
				// but it is incorrect. It does consider 
				// OmitXmlDeclaration property.
				xmldecl_state = settings.OmitXmlDeclaration ? XmlDeclState.Ignore : XmlDeclState.Auto;
				break;
			case ConformanceLevel.Fragment:
				xmldecl_state = XmlDeclState.Prohibit;
				break;
			}
			if (settings.Indent)
				Formatting = Formatting.Indented;
			indent_string = settings.IndentChars == null ?
				String.Empty : settings.IndentChars;
			if (settings.NewLineChars != null)
				newline = settings.NewLineChars;
			indent_attributes = settings.NewLineOnAttributes;

			check_character_validity = settings.CheckCharacters;
			newline_handling = settings.NewLineHandling;
			namespace_handling = settings.NamespaceHandling;
		}
#endif

		void Initialize (TextWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			XmlNameTable name_table = new NameTable ();
			this.writer = writer;
			if (writer is StreamWriter)
				base_stream = ((StreamWriter) writer).BaseStream;
			source = writer;
			nsmanager = new XmlNamespaceManager (name_table);
			newline = writer.NewLine;

			escaped_text_chars =
				newline_handling != NewLineHandling.None ?
				new char [] {'&', '<', '>', '\r', '\n'} :
				new char [] {'&', '<', '>'};
			escaped_attr_chars =
				new char [] {'"', '&', '<', '>', '\r', '\n'};
		}

#if NET_2_0
		// 2.0 XmlWriterSettings support

		// As for ConformanceLevel, MS.NET is inconsistent with
		// MSDN documentation. For example, even if ConformanceLevel
		// is set as .Auto, multiple WriteStartDocument() calls
		// result in an error.
		// ms-help://MS.NETFramework.v20.en/wd_xml/html/7db8802b-53d8-4735-a637-4d2d2158d643.htm

#endif

		// Literal Output Control

		public Formatting Formatting {
			get { return indent ? Formatting.Indented : Formatting.None; }
			set {
				// Someone thinks it should be settable even
				// after writing some content (bug #78148).
				// I totally disagree but here is the fix.

				//if (state != WriteState.Start)
				//	throw InvalidOperation ("Formatting must be set before it is actually used to write output.");
				indent = (value == Formatting.Indented);
			}
		}

		public int Indentation {
			get { return indent_count; }
			set {
				if (value < 0)
					throw ArgumentError ("Indentation must be non-negative integer.");
				indent_count = value;
				indent_string = value == 0 ? String.Empty :
					new string (indent_char, indent_count);
			}
		}

		public char IndentChar {
			get { return indent_char; }
			set {
				indent_char = value;
				indent_string = new string (indent_char, indent_count);
			}
		}

		public char QuoteChar {
			get { return quote_char; }
			set {
				if (state == WriteState.Attribute)
					throw InvalidOperation ("QuoteChar must not be changed inside attribute value.");
				if ((value != '\'') && (value != '\"'))
					throw ArgumentError ("Only ' and \" are allowed as an attribute quote character.");
				quote_char = value;
				escaped_attr_chars [0] = quote_char;
			}
		}

		// Context Retriever

		public override string XmlLang {
			get { return open_count == 0 ? null : elements [open_count - 1].XmlLang; }
		}

		public override XmlSpace XmlSpace {
			get { return open_count == 0 ? XmlSpace.None : elements [open_count - 1].XmlSpace; }
		}

		public override WriteState WriteState {
			get { return state; }
		}

		public override string LookupPrefix (string namespaceUri)
		{
			if (namespaceUri == null || namespaceUri == String.Empty)
				throw ArgumentError ("The Namespace cannot be empty.");

			if (namespaceUri == nsmanager.DefaultNamespace)
				return String.Empty;

			string prefix = nsmanager.LookupPrefixExclusive (
				namespaceUri, false);

			// XmlNamespaceManager has changed to return null
			// when NSURI not found.
			// (Contradiction to the ECMA documentation.)
			return prefix;
		}

		// Stream Control

		public Stream BaseStream {
			get { return base_stream; }
		}

		public override void Close ()
		{
#if NET_2_0
			if (state != WriteState.Error) {
#endif
				if (state == WriteState.Attribute)
					WriteEndAttribute ();
				while (open_count > 0)
					WriteEndElement ();
#if NET_2_0
			}
#endif

			if (close_output_stream)
				writer.Close ();
			else
				writer.Flush ();
			state = WriteState.Closed;
		}

		public override void Flush ()
		{
			writer.Flush ();
		}

		// Misc Control
		public bool Namespaces {
			get { return namespaces; }
			set {
				if (state != WriteState.Start)
					throw InvalidOperation ("This property must be set before writing output.");
				namespaces = value;
			}
		}

		// XML Declaration

		public override void WriteStartDocument ()
		{
			WriteStartDocumentCore (false, false);
			is_document_entity = true;
		}

		public override void WriteStartDocument (bool standalone)
		{
			WriteStartDocumentCore (true, standalone);
			is_document_entity = true;
		}

		void WriteStartDocumentCore (bool outputStd, bool standalone)
		{
			if (state != WriteState.Start)
				throw StateError ("XmlDeclaration");

			switch (xmldecl_state) {
			case XmlDeclState.Ignore:
				return;
			case XmlDeclState.Prohibit:
				throw InvalidOperation ("WriteStartDocument cannot be called when ConformanceLevel is Fragment.");
			}

			state = WriteState.Prolog;

			writer.Write ("<?xml version=");
			writer.Write (quote_char);
			writer.Write ("1.0");
			writer.Write (quote_char);
			if (!ignore_encoding) {
				writer.Write (" encoding=");
				writer.Write (quote_char);
				writer.Write (writer.Encoding.WebName);
				writer.Write (quote_char);
			}
			if (outputStd) {
				writer.Write (" standalone=");
				writer.Write (quote_char);
				writer.Write (standalone ? "yes" : "no");
				writer.Write (quote_char);
			}
			writer.Write ("?>");

			xmldecl_state = XmlDeclState.Ignore;
		}

		public override void WriteEndDocument ()
		{
			switch (state) {
#if NET_2_0
			case WriteState.Error:
#endif
			case WriteState.Closed:
			case WriteState.Start:
				throw StateError ("EndDocument");
			}

			if (state == WriteState.Attribute)
				WriteEndAttribute ();
			while (open_count > 0)
				WriteEndElement ();

			state = WriteState.Start;
			is_document_entity = false;
		}

		// DocType Declaration

		public override void WriteDocType (string name,
			string pubid, string sysid, string subset)
		{
			if (name == null)
				throw ArgumentError ("name");
			if (!XmlChar.IsName (name))
				throw ArgumentError ("name");

			if (node_state != XmlNodeType.None)
				throw StateError ("DocType");
			node_state = XmlNodeType.DocumentType;

			if (xmldecl_state == XmlDeclState.Auto)
				OutputAutoStartDocument ();

			WriteIndent ();

			writer.Write ("<!DOCTYPE ");
			writer.Write (name);
			if (pubid != null) {
				writer.Write (" PUBLIC ");
				writer.Write (quote_char);
				writer.Write (pubid);
				writer.Write (quote_char);
				writer.Write (' ');
				writer.Write (quote_char);
				if (sysid != null)
					writer.Write (sysid);
				writer.Write (quote_char);
			}
			else if (sysid != null) {
				writer.Write (" SYSTEM ");
				writer.Write (quote_char);
				writer.Write (sysid);
				writer.Write (quote_char);
			}

			if (subset != null) {
				writer.Write ("[");
				// LAMESPEC: see the top of this source.
				writer.Write (subset);
				writer.Write ("]");
			}
			writer.Write ('>');

			state = WriteState.Prolog;
		}

		// StartElement

		public override void WriteStartElement (
			string prefix, string localName, string namespaceUri)
		{
#if NET_2_0
			if (state == WriteState.Error || state == WriteState.Closed)
#else
			if (state == WriteState.Closed)
#endif
				throw StateError ("StartTag");
			node_state = XmlNodeType.Element;

			bool anonPrefix = (prefix == null);
			if (prefix == null)
				prefix = String.Empty;

			// Crazy namespace check goes here.
			//
			// 1. if Namespaces is false, then any significant 
			//    namespace indication is not allowed.
			// 2. if Prefix is non-empty and NamespaceURI is
			//    empty, it is an error in 1.x, or it is reset to
			//    an empty string in 2.0.
			// 3. null NamespaceURI indicates that namespace is
			//    not considered.
			// 4. prefix must not be equivalent to "XML" in
			//    case-insensitive comparison.
			if (!namespaces && namespaceUri != null && namespaceUri.Length > 0)
				throw ArgumentError ("Namespace is disabled in this XmlTextWriter.");
			if (!namespaces && prefix.Length > 0)
				throw ArgumentError ("Namespace prefix is disabled in this XmlTextWriter.");

			// If namespace URI is empty, then either prefix
			// must be empty as well, or there is an
			// existing namespace mapping for the prefix.
			if (prefix.Length > 0 && namespaceUri == null) {
				namespaceUri = nsmanager.LookupNamespace (prefix, false);
				if (namespaceUri == null || namespaceUri.Length == 0)
					throw ArgumentError ("Namespace URI must not be null when prefix is not an empty string.");
			}
			// Considering the fact that WriteStartAttribute()
			// automatically changes argument namespaceURI, this
			// is kind of silly implementation. See bug #77094.
			if (namespaces &&
			    prefix != null && prefix.Length == 3 &&
			    namespaceUri != XmlNamespace &&
			    (prefix [0] == 'x' || prefix [0] == 'X') &&
			    (prefix [1] == 'm' || prefix [1] == 'M') &&
			    (prefix [2] == 'l' || prefix [2] == 'L'))
				throw new ArgumentException ("A prefix cannot be equivalent to \"xml\" in case-insensitive match.");


			if (xmldecl_state == XmlDeclState.Auto)
				OutputAutoStartDocument ();
			if (state == WriteState.Element)
				CloseStartElement ();
			if (open_count > 0)
				elements [open_count - 1].HasElements = true;

			nsmanager.PushScope ();

			if (namespaces && namespaceUri != null) {
				// If namespace URI is empty, then prefix must 
				// be empty as well.
				if (anonPrefix && namespaceUri.Length > 0)
					prefix = LookupPrefix (namespaceUri);
				if (prefix == null || namespaceUri.Length == 0)
					prefix = String.Empty;
			}

			WriteIndent ();

			writer.Write ("<");

			if (prefix.Length > 0) {
				writer.Write (prefix);
				writer.Write (':');
			}
			writer.Write (localName);

			if (elements.Length == open_count) {
				XmlNodeInfo [] tmp = new XmlNodeInfo [open_count << 1];
				Array.Copy (elements, tmp, open_count);
				elements = tmp;
			}
			if (elements [open_count] == null)
				elements [open_count] =
					new XmlNodeInfo ();
			XmlNodeInfo info = elements [open_count];
			info.Prefix = prefix;
			info.LocalName = localName;
			info.NS = namespaceUri;
			info.HasSimple = false;
			info.HasElements = false;
			info.XmlLang = XmlLang;
			info.XmlSpace = XmlSpace;
			open_count++;

			if (namespaces && namespaceUri != null) {
				string oldns = nsmanager.LookupNamespace (prefix, false);
				if (oldns != namespaceUri) {
					nsmanager.AddNamespace (prefix, namespaceUri);
					new_local_namespaces.Push (prefix);
				}
			}

			state = WriteState.Element;
		}

		void CloseStartElement ()
		{
			CloseStartElementCore ();

			if (state == WriteState.Element)
				writer.Write ('>');
			state = WriteState.Content;
		}

		void CloseStartElementCore ()
		{
			if (state == WriteState.Attribute)
				WriteEndAttribute ();

			if (new_local_namespaces.Count == 0) {
				if (explicit_nsdecls.Count > 0)
					explicit_nsdecls.Clear ();
				return;
			}

			// Missing xmlns attributes are added to 
			// explicit_nsdecls (it is cleared but this way
			// I save another array creation).
			int idx = explicit_nsdecls.Count;
			while (new_local_namespaces.Count > 0) {
				string p = (string) new_local_namespaces.Pop ();
				bool match = false;
				for (int i = 0; i < explicit_nsdecls.Count; i++) {
					if ((string) explicit_nsdecls [i] == p) {
						match = true;
						break;
					}
				}
				if (match)
					continue;
				explicit_nsdecls.Add (p);
			}

			for (int i = idx; i < explicit_nsdecls.Count; i++) {
				string prefix = (string) explicit_nsdecls [i];
				string ns = nsmanager.LookupNamespace (prefix, false);
				if (ns == null)
					continue; // superceded
				if (prefix.Length > 0) {
					writer.Write (" xmlns:");
					writer.Write (prefix);
				} else {
					writer.Write (" xmlns");
				}
				writer.Write ('=');
				writer.Write (quote_char);
				WriteEscapedString (ns, true);
				writer.Write (quote_char);
			}
			explicit_nsdecls.Clear ();
		}

		// EndElement

		public override void WriteEndElement ()
		{
			WriteEndElementCore (false);
		}

		public override void WriteFullEndElement ()
		{
			WriteEndElementCore (true);
		}

		void WriteEndElementCore (bool full)
		{
#if NET_2_0
			if (state == WriteState.Error || state == WriteState.Closed)
#else
			if (state == WriteState.Closed)
#endif
				throw StateError ("EndElement");
			if (open_count == 0)
				throw InvalidOperation ("There is no more open element.");

			// bool isEmpty = state != WriteState.Content;

			CloseStartElementCore ();

			nsmanager.PopScope ();

			if (state == WriteState.Element) {
				if (full)
					writer.Write ('>');
				else
					writer.Write (" />");
			}

			if (full || state == WriteState.Content)
				WriteIndentEndElement ();

			XmlNodeInfo info = elements [--open_count];

			if (full || state == WriteState.Content) {
				writer.Write ("</");
				if (info.Prefix.Length > 0) {
					writer.Write (info.Prefix);
					writer.Write (':');
				}
				writer.Write (info.LocalName);
				writer.Write ('>');
			}

			state = WriteState.Content;
			if (open_count == 0)
				node_state = XmlNodeType.EndElement;
		}

		// Attribute

		public override void WriteStartAttribute (
			string prefix, string localName, string namespaceUri)
		{
			// LAMESPEC: this violates the expected behavior of
			// this method, as it incorrectly allows unbalanced
			// output of attributes. Microfot changes description
			// on its behavior at their will, regardless of
			// ECMA description.
			if (state == WriteState.Attribute)
				WriteEndAttribute ();

			if (state != WriteState.Element && state != WriteState.Start)
				throw StateError ("Attribute");

			if ((object) prefix == null)
				prefix = String.Empty;

			// For xmlns URI, prefix is forced to be "xmlns"
			bool isNSDecl = false;
			if (namespaceUri == XmlnsNamespace) {
				isNSDecl = true;
				if (prefix.Length == 0 && localName != "xmlns")
					prefix = "xmlns";
			}
			else
				isNSDecl = (prefix == "xmlns" ||
					localName == "xmlns" && prefix.Length == 0);

			if (namespaces) {
				// MS implementation is pretty hacky here. 
				// Regardless of namespace URI it is regarded
				// as NS URI for "xml".
				if (prefix == "xml")
					namespaceUri = XmlNamespace;
				// infer namespace URI.
				else if ((object) namespaceUri == null || (v2 && namespaceUri.Length == 0)) {
					if (isNSDecl)
						namespaceUri = XmlnsNamespace;
					else
						namespaceUri = String.Empty;
				}

				// It is silly design - null namespace with
				// "xmlns" are allowed (for namespace-less
				// output; while there is Namespaces property)
				// On the other hand, namespace "" is not 
				// allowed.
				if (isNSDecl && namespaceUri != XmlnsNamespace)
					throw ArgumentError (String.Format ("The 'xmlns' attribute is bound to the reserved namespace '{0}'", XmlnsNamespace));

				// If namespace URI is empty, then either prefix
				// must be empty as well, or there is an
				// existing namespace mapping for the prefix.
				if (prefix.Length > 0 && namespaceUri.Length == 0) {
					namespaceUri = nsmanager.LookupNamespace (prefix, false);
					if (namespaceUri == null || namespaceUri.Length == 0)
						throw ArgumentError ("Namespace URI must not be null when prefix is not an empty string.");
				}

				// Dive into extremely complex procedure.
				if (!isNSDecl && namespaceUri.Length > 0)
					prefix = DetermineAttributePrefix (
						prefix, localName, namespaceUri);
			}

			if (indent_attributes)
				WriteIndentAttribute ();
			else if (state != WriteState.Start)
				writer.Write (' ');

			if (prefix.Length > 0) {
				writer.Write (prefix);
				writer.Write (':');
			}
			writer.Write (localName);
			writer.Write ('=');
			writer.Write (quote_char);

			if (isNSDecl || prefix == "xml") {
				if (preserver == null)
					preserver = new StringWriter ();
				else
					preserver.GetStringBuilder ().Length = 0;
				writer = preserver;

				if (!isNSDecl) {
					is_preserved_xmlns = false;
					preserved_name = localName;
				} else {
					is_preserved_xmlns = true;
					preserved_name = localName == "xmlns" ? 
						String.Empty : localName;
				}
			}

			state = WriteState.Attribute;
		}

		// See also:
		// "DetermineAttributePrefix(): local mapping overwrite"
		string DetermineAttributePrefix (
			string prefix, string local, string ns)
		{
			bool mockup = false;
			if (prefix.Length == 0) {
				prefix = LookupPrefix (ns);
				if (prefix != null && prefix.Length > 0)
					return prefix;
				mockup = true;
			} else {
				prefix = nsmanager.NameTable.Add (prefix);
				string existing = nsmanager.LookupNamespace (prefix, true);
				if (existing == ns)
					return prefix;
				if (existing != null) {
					// See code comment on the head of
					// this source file.
					nsmanager.RemoveNamespace (prefix, existing);
					if (nsmanager.LookupNamespace (prefix, true) != existing) {
						mockup = true;
						nsmanager.AddNamespace (prefix, existing);
					}
				}
			}

			if (mockup)
				prefix = MockupPrefix (ns, true);
			new_local_namespaces.Push (prefix);
			nsmanager.AddNamespace (prefix, ns);

			return prefix;
		}

		string MockupPrefix (string ns, bool skipLookup)
		{
			string prefix = skipLookup ? null :
				LookupPrefix (ns);
			if (prefix != null && prefix.Length > 0)
				return prefix;
			for (int p = 1; ; p++) {
				prefix = StringUtil.Format ("d{0}p{1}", open_count, p);
				if (new_local_namespaces.Contains (prefix))
					continue;
				if (null != nsmanager.LookupNamespace (
					nsmanager.NameTable.Get (prefix)))
					continue;
				nsmanager.AddNamespace (prefix, ns);
				new_local_namespaces.Push (prefix);
				return prefix;
			}
		}

		public override void WriteEndAttribute ()
		{
			if (state != WriteState.Attribute)
				throw StateError ("End of attribute");

			if (writer == preserver) {
				writer = source;
				string value = preserver.ToString ();
				if (is_preserved_xmlns) {
					if (preserved_name.Length > 0 &&
					    value.Length == 0)
						throw ArgumentError ("Non-empty prefix must be mapped to non-empty namespace URI.");
					string existing = nsmanager.LookupNamespace (preserved_name, false);

					// consider OmitDuplicates here.
					if ((namespace_handling & NamespaceHandling.OmitDuplicates) == 0 || existing != value)
						explicit_nsdecls.Add (preserved_name);

					if (open_count > 0) {

						if (v2 &&
						    elements [open_count - 1].Prefix == preserved_name &&
						    elements [open_count - 1].NS != value)
							throw new XmlException (String.Format ("Cannot redefine the namespace for prefix '{0}' used at current element", preserved_name));

						if (elements [open_count - 1].NS == String.Empty &&
						    elements [open_count - 1].Prefix == preserved_name)
						    	; // do nothing
						else if (existing != value)
							nsmanager.AddNamespace (preserved_name, value);
					}
				} else {
					switch (preserved_name) {
					case "lang":
						if (open_count > 0)
							elements [open_count - 1].XmlLang = value;
						break;
					case "space":
						switch (value) {
						case "default":
							if (open_count > 0)
								elements [open_count - 1].XmlSpace = XmlSpace.Default;
							break;
						case "preserve":
							if (open_count > 0)
								elements [open_count - 1].XmlSpace = XmlSpace.Preserve;
							break;
						default:
							throw ArgumentError ("Invalid value for xml:space.");
						}
						break;
					}
				}
				writer.Write (value);
			}

			writer.Write (quote_char);
			state = WriteState.Element;
		}

		// Non-Text Content

		public override void WriteComment (string text)
		{
			if (text == null)
				throw ArgumentError ("text");

			if (text.Length > 0 && text [text.Length - 1] == '-')
				throw ArgumentError ("An input string to WriteComment method must not end with '-'. Escape it with '&#2D;'.");
			if (StringUtil.IndexOf (text, "--") > 0)
				throw ArgumentError ("An XML comment cannot end with \"-\".");

			if (state == WriteState.Attribute || state == WriteState.Element)
				CloseStartElement ();

			WriteIndent ();

			ShiftStateTopLevel ("Comment", false, false, false);

			writer.Write ("<!--");
			writer.Write (text);
			writer.Write ("-->");
		}

		// LAMESPEC: see comments on the top of this source.
		public override void WriteProcessingInstruction (string name, string text)
		{
			if (name == null)
				throw ArgumentError ("name");
			if (text == null)
				throw ArgumentError ("text");

			WriteIndent ();

			if (!XmlChar.IsName (name))
				throw ArgumentError ("A processing instruction name must be a valid XML name.");

			if (StringUtil.IndexOf (text, "?>") > 0)
				throw ArgumentError ("Processing instruction cannot contain \"?>\" as its value.");

			ShiftStateTopLevel ("ProcessingInstruction", false, name == "xml", false);

			writer.Write ("<?");
			writer.Write (name);
			writer.Write (' ');
			writer.Write (text);
			writer.Write ("?>");

			if (state == WriteState.Start)
				state = WriteState.Prolog;
		}

		// Text Content

		public override void WriteWhitespace (string text)
		{
			if (text == null)
				throw ArgumentError ("text");

			// huh? Shouldn't it accept an empty string???
			if (text.Length == 0 ||
			    XmlChar.IndexOfNonWhitespace (text) >= 0)
				throw ArgumentError ("WriteWhitespace method accepts only whitespaces.");

			ShiftStateTopLevel ("Whitespace", true, false, true);

			writer.Write (text);
		}

		public override void WriteCData (string text)
		{
			if (text == null)
				text = String.Empty;
			ShiftStateContent ("CData", false);

			if (StringUtil.IndexOf (text, "]]>") >= 0)
				throw ArgumentError ("CDATA section must not contain ']]>'.");
			writer.Write ("<![CDATA[");
			WriteCheckedString (text);
			writer.Write ("]]>");
		}

		public override void WriteString (string text)
		{
			if (text == null || (text.Length == 0 && !v2))
				return; // do nothing, including state transition.
			ShiftStateContent ("Text", true);

			WriteEscapedString (text, state == WriteState.Attribute);
		}

		public override void WriteRaw (string raw)
		{
			if (raw == null)
				return; // do nothing, including state transition.

			//WriteIndent ();

			// LAMESPEC: It rejects XMLDecl while it allows
			// DocType which could consist of non well-formed XML.
			ShiftStateTopLevel ("Raw string", true, true, true);

			writer.Write (raw);
		}

		public override void WriteCharEntity (char ch)
		{
			WriteCharacterEntity (ch, '\0', false);
		}

		public override void WriteSurrogateCharEntity (char low, char high)
		{
			WriteCharacterEntity (low, high, true);
		}

		void WriteCharacterEntity (char ch, char high, bool surrogate)
		{
			if (surrogate &&
			    ('\uD800' > high || high > '\uDC00' ||
			     '\uDC00' > ch || ch > '\uDFFF'))
				throw ArgumentError (String.Format ("Invalid surrogate pair was found. Low: &#x{0:X}; High: &#x{0:X};", (int) ch, (int) high));
			else if (check_character_validity && XmlChar.IsInvalid (ch))
				throw ArgumentError (String.Format ("Invalid character &#x{0:X};", (int) ch));

			ShiftStateContent ("Character", true);

			int v = surrogate ? (high - 0xD800) * 0x400 + ch - 0xDC00 + 0x10000 : (int) ch;
			writer.Write ("&#x");
			writer.Write (v.ToString ("X", CultureInfo.InvariantCulture));
			writer.Write (';');
		}

		public override void WriteEntityRef (string name)
		{
			if (name == null)
				throw ArgumentError ("name");
			if (!XmlChar.IsName (name))
				throw ArgumentError ("Argument name must be a valid XML name.");

			ShiftStateContent ("Entity reference", true);

			writer.Write ('&');
			writer.Write (name);
			writer.Write (';');
		}

		// Applied methods

		public override void WriteName (string name)
		{
			if (name == null)
				throw ArgumentError ("name");
			if (!XmlChar.IsName (name))
				throw ArgumentError ("Not a valid name string.");
			WriteString (name);
		}

		public override void WriteNmToken (string nmtoken)
		{
			if (nmtoken == null)
				throw ArgumentError ("nmtoken");
			if (!XmlChar.IsNmToken (nmtoken))
				throw ArgumentError ("Not a valid NMTOKEN string.");
			WriteString (nmtoken);
		}

		public override void WriteQualifiedName (
			string localName, string ns)
		{
			if (localName == null)
				throw ArgumentError ("localName");
			if (ns == null)
				ns = String.Empty;

			if (ns == XmlnsNamespace)
				throw ArgumentError ("Prefix 'xmlns' is reserved and cannot be overriden.");
			if (!XmlChar.IsNCName (localName))
				throw ArgumentError ("localName must be a valid NCName.");

			ShiftStateContent ("QName", true);

			string prefix = ns.Length > 0 ? LookupPrefix (ns) : String.Empty;
			if (prefix == null) {
				if (state == WriteState.Attribute)
					prefix = MockupPrefix (ns, false);
				else
					throw ArgumentError (String.Format ("Namespace '{0}' is not declared.", ns));
			}

			if (prefix != String.Empty) {
				writer.Write (prefix);
				writer.Write (":");
			}
			writer.Write (localName);
		}

		// Chunk data

		void CheckChunkRange (Array buffer, int index, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (index < 0 || buffer.Length < index)
				throw ArgumentOutOfRangeError ("index");
			if (count < 0 || buffer.Length < index + count)
				throw ArgumentOutOfRangeError ("count");
		}

		public override void WriteBase64 (byte [] buffer, int index, int count)
		{
			CheckChunkRange (buffer, index, count);

			WriteString (Convert.ToBase64String (buffer, index, count));
		}

		public override void WriteBinHex (byte [] buffer, int index, int count)
		{
			CheckChunkRange (buffer, index, count);

			ShiftStateContent ("BinHex", true);

			XmlConvert.WriteBinHex (buffer, index, count, writer);
		}

		public override void WriteChars (char [] buffer, int index, int count)
		{
			CheckChunkRange (buffer, index, count);

			ShiftStateContent ("Chars", true);

			WriteEscapedBuffer (buffer, index, count,
				state == WriteState.Attribute);
		}

		public override void WriteRaw (char [] buffer, int index, int count)
		{
			CheckChunkRange (buffer, index, count);

			ShiftStateContent ("Raw text", false);

			writer.Write (buffer, index, count);
		}

		// Utilities

		void WriteIndent ()
		{
			WriteIndentCore (0, false);
		}

		void WriteIndentEndElement ()
		{
			WriteIndentCore (-1, false);
		}

		void WriteIndentAttribute ()
		{
			if (!WriteIndentCore (0, true))
				writer.Write (' '); // space is required instead.
		}

		bool WriteIndentCore (int nestFix, bool attribute)
		{
			if (!indent)
				return false;
			for (int i = open_count - 1; i >= 0; i--)
				if (!attribute && elements [i].HasSimple)
					return false;

			if (state != WriteState.Start)
				writer.Write (newline);
			for (int i = 0; i < open_count + nestFix; i++)
				writer.Write (indent_string);
			return true;
		}

		void OutputAutoStartDocument ()
		{
			if (state != WriteState.Start)
				return;
			WriteStartDocumentCore (false, false);
		}

		void ShiftStateTopLevel (string occured, bool allowAttribute, bool dontCheckXmlDecl, bool isCharacter)
		{
			switch (state) {
#if NET_2_0
			case WriteState.Error:
#endif
			case WriteState.Closed:
				throw StateError (occured);
			case WriteState.Start:
				if (isCharacter)
					CheckMixedContentState ();
				if (xmldecl_state == XmlDeclState.Auto && !dontCheckXmlDecl)
					OutputAutoStartDocument ();
				state = WriteState.Prolog;
				break;
			case WriteState.Attribute:
				if (allowAttribute)
					break;
				goto case WriteState.Closed;
			case WriteState.Element:
				if (isCharacter)
					CheckMixedContentState ();
				CloseStartElement ();
				break;
			case WriteState.Content:
				if (isCharacter)
					CheckMixedContentState ();
				break;
			}

		}

		void CheckMixedContentState ()
		{
//			if (open_count > 0 &&
//			    state != WriteState.Attribute)
//				elements [open_count - 1].HasSimple = true;
			if (open_count > 0)
				elements [open_count - 1].HasSimple = true;
		}

		void ShiftStateContent (string occured, bool allowAttribute)
		{
			switch (state) {
#if NET_2_0
			case WriteState.Error:
#endif
			case WriteState.Closed:
					throw StateError (occured);
			case WriteState.Prolog:
			case WriteState.Start:
				if (!allow_doc_fragment || is_document_entity)
					goto case WriteState.Closed;
				if (xmldecl_state == XmlDeclState.Auto)
					OutputAutoStartDocument ();
				CheckMixedContentState ();
				state = WriteState.Content;
				break;
			case WriteState.Attribute:
				if (allowAttribute)
					break;
				goto case WriteState.Closed;
			case WriteState.Element:
				CloseStartElement ();
				CheckMixedContentState ();
				break;
			case WriteState.Content:
				CheckMixedContentState ();
				break;
			}
		}

		void WriteEscapedString (string text, bool isAttribute)
		{
			char [] escaped = isAttribute ?
				escaped_attr_chars : escaped_text_chars;

			int idx = text.IndexOfAny (escaped);
			if (idx >= 0) {
				char [] arr = text.ToCharArray ();
				WriteCheckedBuffer (arr, 0, idx);
				WriteEscapedBuffer (
					arr, idx, arr.Length - idx, isAttribute);
			} else {
				WriteCheckedString (text);
			}
		}

		void WriteCheckedString (string s)
		{
			int i = XmlChar.IndexOfInvalid (s, true);
			if (i >= 0) {
				char [] arr = s.ToCharArray ();
				writer.Write (arr, 0, i);
				WriteCheckedBuffer (arr, i, arr.Length - i);
			} else {
				// no invalid character.
				writer.Write (s);
			}
		}

		void WriteCheckedBuffer (char [] text, int idx, int length)
		{
			int start = idx;
			int end = idx + length;
			while ((idx = XmlChar.IndexOfInvalid (text, start, length, true)) >= 0) {
				if (check_character_validity) // actually this is one time pass.
					throw ArgumentError (String.Format ("Input contains invalid character at {0} : &#x{1:X};", idx, (int) text [idx]));
				if (start < idx)
					writer.Write (text, start, idx - start);
				writer.Write ("&#x");
				writer.Write (((int) text [idx]).ToString (
					"X",
					CultureInfo.InvariantCulture));
				writer.Write (';');
				length -= idx - start + 1;
				start = idx + 1;
			}
			if (start < end)
				writer.Write (text, start, end - start);
		}

		void WriteEscapedBuffer (char [] text, int index, int length,
			bool isAttribute)
		{
			int start = index;
			int end = index + length;
			for (int i = start; i < end; i++) {
				switch (text [i]) {
				default:
					continue;
				case '&':
				case '<':
				case '>':
					if (start < i)
						WriteCheckedBuffer (text, start, i - start);
					writer.Write ('&');
					switch (text [i]) {
					case '&': writer.Write ("amp;"); break;
					case '<': writer.Write ("lt;"); break;
					case '>': writer.Write ("gt;"); break;
					case '\'': writer.Write ("apos;"); break;
					case '"': writer.Write ("quot;"); break;
					}
					break;
				case '"':
				case '\'':
					if (isAttribute && text [i] == quote_char)
						goto case '&';
					continue;
				case '\r':
					if (i + 1 < end && text [i] == '\n')
						i++; // CRLF
					goto case '\n';
				case '\n':
					if (start < i)
						WriteCheckedBuffer (text, start, i - start);
					if (isAttribute) {
						writer.Write (text [i] == '\r' ?
							"&#xD;" : "&#xA;");
						break;
					}
					switch (newline_handling) {
					case NewLineHandling.Entitize:
						writer.Write (text [i] == '\r' ?
							"&#xD;" : "&#xA;");
						break;
					case NewLineHandling.Replace:
						writer.Write (newline);
						break;
					default:
						writer.Write (text [i]);
						break;
					}
					break;
				}
				start = i + 1;
			}
			if (start < end)
				WriteCheckedBuffer (text, start, end - start);
		}

		// Exceptions

		Exception ArgumentOutOfRangeError (string name)
		{
#if NET_2_0
			state = WriteState.Error;
#endif
			return new ArgumentOutOfRangeException (name);
		}

		Exception ArgumentError (string msg)
		{
#if NET_2_0
			state = WriteState.Error;
#endif
			return new ArgumentException (msg);
		}

		Exception InvalidOperation (string msg)
		{
#if NET_2_0
			state = WriteState.Error;
#endif
			return new InvalidOperationException (msg);
		}

		Exception StateError (string occured)
		{
			return InvalidOperation (String.Format ("This XmlWriter does not accept {0} at this state {1}.", occured, state));
		}
	}
}
