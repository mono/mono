//
// Commons.Xml.Relaxng.RelaxngReader.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//
// Copyright (c) 2004 Novell Inc.
// All rights reserved
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
using System.Collections;
using System.Xml;

namespace Commons.Xml.Relaxng
{
	public class RelaxngReader : XmlDefaultReader
	{
		// static members.
		static RelaxngPattern grammarForRelaxng;
		static XmlReader relaxngXmlReader;
		static RelaxngReader ()
		{
			relaxngXmlReader = new XmlTextReader (typeof (RelaxngReader).Assembly.GetManifestResourceStream ("relaxng.rng"));
			grammarForRelaxng =
				RelaxngPattern.Read (relaxngXmlReader);
		}

		[Obsolete] // incorrectly introduced
		public static string RelaxngNS = "http://relaxng.org/ns/structure/1.0";
		public static RelaxngPattern GrammarForRelaxng {
			get { return grammarForRelaxng; }
		}


		// fields
		Stack nsStack = new Stack ();
		Stack datatypeLibraryStack = new Stack ();
		XmlResolver resolver;
		bool skipExternal = true;
//		ArrayList annotationNamespaces = new ArrayList ();

		// ctor
		public RelaxngReader (XmlReader reader)
			: this (reader, null)
		{
		}

		public RelaxngReader (XmlReader reader, string ns)
			: this (reader, ns, new XmlUrlResolver ())
		{
		}

		public RelaxngReader (XmlReader reader, string ns, XmlResolver resolver)
//			: base (grammarForRelaxng == null ? reader : new RelaxngValidatingReader (reader, grammarForRelaxng))
			: base (reader)
		{
			this.resolver = resolver;
			if (Reader.ReadState == ReadState.Initial)
				Read ();
			MoveToContent ();
			string nsval = GetSpaceStrippedAttribute ("ns", String.Empty);
			if (nsval == null)
				nsval = ns;
			nsStack.Push (nsval == null ? String.Empty : nsval);
			string dtlib = GetSpaceStrippedAttribute ("datatypeLibrary", String.Empty);
			datatypeLibraryStack.Push (dtlib != null ?
				dtlib : String.Empty);
		}

		public XmlResolver XmlResolver {
			set { resolver = value; }
		}

		internal XmlResolver Resolver {
			get { return resolver; }
		}

		private void FillLocation (RelaxngElementBase el)
		{
			el.BaseUri = BaseURI;
			IXmlLineInfo li = this as IXmlLineInfo;
			el.LineNumber = li != null ? li.LineNumber : 0;
			el.LinePosition = li != null ? li.LinePosition : 0;
		}

/*
		public void AddAnnotationNamespace (string ns)
		{
			if (!annotationNamespaces.Contains (ns))
				annotationNamespaces.Add (ns);
		}
*/

		// public
		public override bool Read ()
		{
			bool skipRead = false;
			bool b = false;
			bool loop = true;
			MoveToElement ();
			if (IsEmptyElement || NodeType == XmlNodeType.EndElement) { // this should be done here
				nsStack.Pop ();
				datatypeLibraryStack.Pop ();
			}
			do {
				if (!skipRead)
					b = Reader.Read ();
				else
					skipRead = false;
				switch (NodeType) {
				case XmlNodeType.ProcessingInstruction:
				case XmlNodeType.Comment:
				case XmlNodeType.EntityReference:
					continue;
				case XmlNodeType.Whitespace:
				// Skip whitespaces except for data and param.
				case XmlNodeType.SignificantWhitespace:
					if (LocalName != "value" && LocalName != "param") {
						continue;
					}
					else
						loop = false;
					break;
				case XmlNodeType.Element:
				case XmlNodeType.EndElement:
					if (!skipExternal)
						goto default;
					if (NamespaceURI != RelaxngGrammar.NamespaceURI) {
						Reader.Skip ();
						skipRead = true;
					}
					else
						loop = false;
					break;
				default:
					loop = false;
					break;
				}
			} while (!Reader.EOF && loop);

			switch (NodeType) {
			case XmlNodeType.Element:
				if (MoveToAttribute ("ns")) {
					nsStack.Push (Value.Trim ());
					MoveToElement ();
				}
				else
					nsStack.Push (ContextNamespace);

				if (MoveToAttribute ("datatypeLibrary")) {
					string uriString = Value.Trim ();
					if (uriString.Length == 0)
						datatypeLibraryStack.Push (String.Empty);
					else {
						try {
							Uri uri = new Uri (uriString);
							// MS.NET Uri is too lamespec
							datatypeLibraryStack.Push (uri.ToString ());
						} catch (UriFormatException ex) {
							throw new RelaxngException (ex.Message, ex);
						}
					}
					MoveToElement ();
				}
				else
					datatypeLibraryStack.Push (DatatypeLibrary);
				break;
			}

			return b;
		}

		// Properties

		public string ContextNamespace {
			get {
				if (nsStack.Count == 0)
					// It happens only on initialization.
					return String.Empty;
				return nsStack.Peek () as string;
			}
		}

		public string DatatypeLibrary {
			get {
				if (datatypeLibraryStack.Count == 0)
					// It happens only on initialization.
					return String.Empty;
				return datatypeLibraryStack.Peek () as string;
			}
		}

		// Utility methods.
		private void expect (string name)
		{
			if (NamespaceURI != RelaxngGrammar.NamespaceURI)
				throw new RelaxngException (String.Format ("Invalid document: expected namespace {0} but found {1}", RelaxngGrammar.NamespaceURI, NamespaceURI));
			else if (LocalName != name)
				throw new RelaxngException (String.Format ("Invalid document: expected local name {0} but found {1}", name, LocalName));
		}

		private void expectEnd (string name)
		{
			if (NodeType != XmlNodeType.EndElement)
				throw new RelaxngException (String.Format ("Expected EndElement '{1}' but found {0} '{2}'.", NodeType, name, LocalName));
			expect (name);

			Read ();
		}

		// Other than name class and pattern.
		private RelaxngStart ReadStart ()
		{
			RelaxngStart s = new RelaxngStart ();
			FillLocation (s);
			expect ("start");

			if (MoveToFirstAttribute ()) {
				do {
					if (NamespaceURI != String.Empty)
						continue;
					switch (LocalName) {
					case "datatypeLibrary":
					case  "combine":
						break;
					default:
						throw new RelaxngException ("Invalid attribute.");
					}
				} while (MoveToNextAttribute ());
				MoveToElement ();
			}

			if (MoveToAttribute ("combine")) {
				s.Combine = Value.Trim ();
				if (s.Combine != "choice" && s.Combine != "interleave")
					throw new RelaxngException ("Invalid combine attribute: " + s.Combine);
			}

			MoveToElement ();
			Read ();
			s.Pattern = ReadPattern ();
			expectEnd ("start");
			return s;
		}

		private string GetNameAttribute ()
		{
			string name = GetSpaceStrippedAttribute ("name", String.Empty);
			if (name == null)
				throw new RelaxngException ("Required attribute name is not found.");
			return XmlConvert.VerifyNCName (name);
		}

		private string GetSpaceStrippedAttribute (string name, string ns)
		{
			string v = GetAttribute (name, ns);
			return v != null ? v.Trim () : null;
		}

		private RelaxngDefine ReadDefine ()
		{
			RelaxngDefine def = new RelaxngDefine ();
			FillLocation (def);
			expect ("define");
			def.Name = GetNameAttribute ();
			def.Combine = GetSpaceStrippedAttribute ("combine", String.Empty);

			Read ();
			while (NodeType == XmlNodeType.Element)
				def.Patterns.Add (ReadPattern ());
			expectEnd ("define");
			return def;
		}

		private RelaxngParam ReadParam ()
		{
			RelaxngParam p = new RelaxngParam ();
			FillLocation (p);
			expect ("param");
			p.Name = GetNameAttribute ();
			p.Value = ReadString ().Trim ();
			expectEnd ("param");
			return p;
		}

		// NameClass reader (only if it is element-style.)
		private RelaxngNameClass ReadNameClass ()
		{
			switch (LocalName) {
			case "name":
				return ReadNameClassName ();
			case "anyName":
				return ReadNameClassAnyName ();
			case "nsName":
				return ReadNameClassNsName ();
			case "choice":
				return ReadNameClassChoice ();
			}
			throw new RelaxngException ("Invalid name class: " + LocalName);
		}

		private RelaxngName ReadNameClassName ()
		{
			string name = ReadString ().Trim ();
			RelaxngName rName = resolvedName (name);
			expectEnd ("name");
			return rName;
		}

		private RelaxngAnyName ReadNameClassAnyName ()
		{
			RelaxngAnyName an = new RelaxngAnyName ();
			FillLocation (an);
			if (!IsEmptyElement) {
				Read ();
				if (NodeType == XmlNodeType.EndElement) {
				} else {
					// expect except
					expect ("except");
					Read ();
					an.Except = new RelaxngExceptNameClass ();
					FillLocation (an.Except);
					while (NodeType == XmlNodeType.Element)
						an.Except.Names.Add (
							ReadNameClass ());
					expectEnd ("except");
				}
				expectEnd ("anyName");
			} else
				Read ();
			return an;
		}

		private RelaxngNsName ReadNameClassNsName ()
		{
			RelaxngNsName nn = new RelaxngNsName ();
			FillLocation (nn);
			nn.Namespace = this.ContextNamespace;
			if (!IsEmptyElement) {
				Read ();
				if (NodeType == XmlNodeType.EndElement) {
				} else {
					// expect except
					expect ("except");
//					Read ();
					nn.Except = ReadNameClassExcept ();//new RelaxngExceptNameClass ();
					FillLocation (nn.Except);
				}
				expectEnd ("nsName");
			} else
				Read ();
			return nn;
		}

		private RelaxngNameChoice ReadNameClassChoice ()
		{
			RelaxngNameChoice nc = new RelaxngNameChoice ();
			FillLocation (nc);
			if (IsEmptyElement)
				throw new RelaxngException ("Name choice must have at least one name class.");

			Read ();
			while (NodeType != XmlNodeType.EndElement) {
				nc.Children.Add (ReadNameClass ());
			}
			if (nc.Children.Count == 0)
				throw new RelaxngException ("Name choice must have at least one name class.");

			expectEnd ("choice");
			return nc;
		}

		private RelaxngExceptNameClass ReadNameClassExcept ()
		{
			RelaxngExceptNameClass x = new RelaxngExceptNameClass ();
			FillLocation (x);
			if (IsEmptyElement)
				throw new RelaxngException ("Name choice must have at least one name class.");

			Read ();
			while (NodeType != XmlNodeType.EndElement)
				x.Names.Add (ReadNameClass ());
			if (x.Names.Count == 0)
				throw new RelaxngException ("Name choice must have at least one name class.");

			expectEnd ("except");
			return x;
		}

		// Pattern reader

		public RelaxngPattern ReadPattern ()
		{
			while (NodeType != XmlNodeType.Element)
				if (!Read ())
					throw new RelaxngException ("RELAX NG pattern did not appear.");

			switch (LocalName) {
			case "element":
				return ReadElementPattern ();
			case "attribute":
				return ReadAttributePattern ();
			case "group":
				return ReadGroupPattern ();
			case "interleave":
				return ReadInterleavePattern ();
			case "choice":
				return ReadChoicePattern ();
			case "optional":
				return ReadOptionalPattern ();
			case "zeroOrMore":
				return ReadZeroOrMorePattern ();
			case "oneOrMore":
				return ReadOneOrMorePattern ();
			case "list":
				return ReadListPattern ();
			case "mixed":
				return ReadMixedPattern ();
			case "ref":
				return ReadRefPattern ();
			case "parentRef":
				return ReadParentRefPattern ();
			case "empty":
				return ReadEmptyPattern ();
			case "text":
				return ReadTextPattern ();
			case "data":
				return ReadDataPattern ();
			case "value":
				return ReadValuePattern ();
			case "notAllowed":
				return ReadNotAllowedPattern ();
			case "externalRef":
				return ReadExternalRefPattern ();
			case "grammar":
				return ReadGrammarPattern ();
			}
			throw new RelaxngException ("Non-supported pattern specification: " + LocalName);
		}

		private void ReadPatterns (RelaxngSingleContentPattern el)
		{
			do {
				el.Patterns.Add (ReadPattern ());
			} while (NodeType == XmlNodeType.Element);
		}

		private void ReadPatterns (RelaxngBinaryContentPattern el)
		{
			do {
				el.Patterns.Add (ReadPattern ());
			} while (NodeType == XmlNodeType.Element);
		}

		private RelaxngExcept ReadPatternExcept ()
		{
			RelaxngExcept x = new RelaxngExcept ();
			FillLocation (x);
			if (IsEmptyElement)
				throw new RelaxngException ("'except' must have at least one pattern.");
			Read ();
			while (NodeType != XmlNodeType.EndElement)
				x.Patterns.Add (ReadPattern ());
			if (x.Patterns.Count == 0)
				throw new RelaxngException ("'except' must have at least one pattern.");

			expectEnd ("except");
			return x;
		}

		private RelaxngInclude ReadInclude ()
		{
			RelaxngInclude i = new RelaxngInclude ();
			FillLocation (i);
			expect ("include");
			i.NSContext = ContextNamespace;
			string href = GetSpaceStrippedAttribute ("href", String.Empty);
			if (href == null)
				throw new RelaxngException ("Required attribute href was not found.");
			XmlResolver res = resolver != null ? resolver : new XmlUrlResolver ();
			i.Href = res.ResolveUri (BaseURI != null ? new Uri (BaseURI) : null, href).AbsoluteUri;
			if (!IsEmptyElement) {
				Read ();
				this.readGrammarIncludeContent (i.Starts, i.Defines, i.Divs, null);
				expectEnd ("include");
			}
			else
				Read ();
			return i;
		}

		private void readGrammarIncludeContent (IList starts, IList defines, IList divs, IList includes)
		{
			while (NodeType == XmlNodeType.Element) {
				switch (LocalName) {
				case "start":
					starts.Add (ReadStart ());
					break;
				case "define":
					defines.Add (ReadDefine ());
					break;
				case "div":
					divs.Add (ReadDiv (includes != null));
					break;
				case "include":
					if (includes != null)
						includes.Add (ReadInclude ());
					else
						throw new RelaxngException ("Unexpected content: " + Name);
					break;
				default:
					throw new RelaxngException ("Unexpected content: " + Name);
				}
			}
		}

		private RelaxngDiv ReadDiv (bool allowIncludes)
		{
			expect ("div");
			RelaxngDiv div = new RelaxngDiv ();
			FillLocation (div);
			if (!IsEmptyElement) {
				Read ();
				readGrammarIncludeContent (div.Starts, div.Defines, div.Divs, div.Includes);
				expectEnd ("div");
			}
			else
				Read ();
			return div;
		}

		private RelaxngName resolvedName (string nameSpec)
		{
			int colonAt = nameSpec.IndexOf (':');
			string prefix = (colonAt < 0) ? "" : nameSpec.Substring (0, colonAt);
			string local = (colonAt < 0) ? nameSpec : nameSpec.Substring (colonAt + 1, nameSpec.Length - colonAt - 1);
			string uri = ContextNamespace;

			if (prefix != "") {
				uri = LookupNamespace (prefix);
				if (uri == null)
					throw new RelaxngException ("Undeclared prefix in name component: " + nameSpec);
			}
			RelaxngName n = new RelaxngName (local, uri);
			FillLocation (n);
			return n;
		}

		private RelaxngElement ReadElementPattern ()
		{
			RelaxngElement el = new RelaxngElement ();
			FillLocation (el);

			if (MoveToFirstAttribute ()) {
				do {
					if (NamespaceURI != String.Empty)
						continue;
					switch (LocalName) {
					case "datatypeLibrary":
					case  "name":
					case "ns":
						break;
					default:
						throw new RelaxngException ("Invalid attribute.");
					}
				} while (MoveToNextAttribute ());
				MoveToElement ();
			}

			// try to get name from attribute.
			if (MoveToAttribute ("name"))
				el.NameClass = resolvedName (XmlConvert.VerifyName (Value.Trim ()));
			MoveToElement ();
			Read ();

			// read nameClass from content.
			if (el.NameClass == null)
				el.NameClass = ReadNameClass ();

			// read patterns.
			this.ReadPatterns (el);

			expectEnd ("element");

			if (el.NameClass == null)
				throw new RelaxngException ("Name class was not specified.");
			return el;
		}

		private RelaxngAttribute ReadAttributePattern ()
		{
			RelaxngAttribute attr = new RelaxngAttribute ();
			FillLocation (attr);

			if (MoveToFirstAttribute ()) {
				do {
					if (NamespaceURI != String.Empty)
						continue;
					switch (LocalName) {
					case "datatypeLibrary":
					case "name":
					case "ns":
						break;
					default:
						throw new RelaxngException ("Invalid attribute.");
					}
				} while (MoveToNextAttribute ());
				MoveToElement ();
			}

			string ns = GetSpaceStrippedAttribute ("ns", String.Empty);

			// try to get name from attribute.
			if (MoveToAttribute ("name", String.Empty)) {
//				attr.NameClass = resolvedName (XmlConvert.VerifyName (Value.Trim ()), false);
				RelaxngName nc = new RelaxngName ();
				string name = XmlConvert.VerifyName (Value.Trim ());
				if (name.IndexOf (':') > 0)
					nc = resolvedName (name);
				else {
					nc.LocalName = name;
					nc.Namespace = ns == null ? String.Empty : ns;
				}
				attr.NameClass = nc;
			}

			MoveToElement ();
			if (!IsEmptyElement) {
				Read ();
				// read nameClass from content.
				if (attr.NameClass == null)
					attr.NameClass = ReadNameClass ();

				if (NodeType == XmlNodeType.Element)
					attr.Pattern = ReadPattern ();

				expectEnd ("attribute");
			} else
				Read ();

			if (attr.NameClass == null)
				throw new RelaxngException ("Name class was not specified.");
			return attr;
		}

		private RelaxngGrammar ReadGrammarPattern ()
		{
			RelaxngGrammar grammar = new RelaxngGrammar ();
			FillLocation (grammar);
			grammar.DefaultNamespace = Reader.GetAttribute ("ns");
			Read ();
			this.readGrammarIncludeContent (grammar.Starts, grammar.Defines, grammar.Divs, grammar.Includes);
			expectEnd ("grammar");

			return grammar;
		}

		private RelaxngRef ReadRefPattern ()
		{
			RelaxngRef r = new RelaxngRef ();
			FillLocation (r);
			expect ("ref");
			r.Name = GetNameAttribute ();
			if (!IsEmptyElement) {
				Read ();
				expectEnd ("ref");
			}
			else
				Read ();
			return r;
		}

		private RelaxngExternalRef ReadExternalRefPattern ()
		{
			RelaxngExternalRef r = new RelaxngExternalRef ();
			FillLocation (r);
			expect ("externalRef");
			string href = GetSpaceStrippedAttribute ("href", String.Empty);
			if (href == null)
				throw new RelaxngException ("Required attribute href was not found.");
			XmlResolver res = resolver != null ? resolver : new XmlUrlResolver ();
			r.Href = res.ResolveUri (BaseURI != null ? new Uri (BaseURI) : null, href).AbsoluteUri;
			r.NSContext = ContextNamespace;
			if (!IsEmptyElement) {
				Read ();
				expectEnd ("externalRef");
			}
			else
				Read ();
			return r;
		}

		private RelaxngParentRef ReadParentRefPattern ()
		{
			RelaxngParentRef r = new RelaxngParentRef ();
			FillLocation (r);
			expect ("parentRef");
			r.Name = GetNameAttribute ();
			if (!IsEmptyElement) {
				Read ();
				expectEnd ("parentRef");
			}
			else
				Read ();
			return r;
		}

		private RelaxngEmpty ReadEmptyPattern ()
		{
			expect ("empty");

			if (MoveToFirstAttribute ()) {
				do {
					if (NamespaceURI == String.Empty && LocalName != "datatypeLibrary")
						throw new RelaxngException ("Invalid attribute.");
				} while (MoveToNextAttribute ());
				MoveToElement ();
			}

			if (!IsEmptyElement) {
				Read ();
				expectEnd ("empty");
			}
			else
				Read ();

			RelaxngEmpty empty = new RelaxngEmpty ();
			FillLocation (empty);
			return empty;
		}

		private RelaxngText ReadTextPattern ()
		{
			expect ("text");

			if (MoveToFirstAttribute ()) {
				do {
					if (NamespaceURI == String.Empty && LocalName != "datatypeLibrary")
						throw new RelaxngException ("Invalid attribute.");
				} while (MoveToNextAttribute ());
				MoveToElement ();
			}

			if (!IsEmptyElement) {
				Read ();
				expectEnd ("text");
			}
			else
				Read ();

			RelaxngText t = new RelaxngText ();
			FillLocation (t);
			return t;
		}

		private RelaxngData ReadDataPattern ()
		{
			RelaxngData data = new RelaxngData ();
			FillLocation (data);

			expect ("data");
			data.Type = GetSpaceStrippedAttribute ("type", String.Empty);
			if (data.Type == null)
				throw new RelaxngException ("Attribute type is required.");
			data.DatatypeLibrary = DatatypeLibrary;

			if (MoveToFirstAttribute ()) {
				do {
					if (NamespaceURI != String.Empty)
						continue;
					switch (LocalName) {
					case "datatypeLibrary":
					case "type":
						break;
					default:
						throw new RelaxngException ("Invalid attribute.");
					}
				} while (MoveToNextAttribute ());
				MoveToElement ();
			}

			if (!IsEmptyElement) {
				Read ();
				while (LocalName == "param")
					data.ParamList.Add (ReadParam ());
				if (LocalName == "except")
					data.Except = ReadPatternExcept ();
				expectEnd ("data");
			} else
				Read ();

			return data;
		}

		private RelaxngValue ReadValuePattern ()
		{
			RelaxngValue v = new RelaxngValue ();
			FillLocation (v);
			expect ("value");

			if (MoveToFirstAttribute ()) {
				do {
					if (NamespaceURI != String.Empty)
						continue;
					switch (LocalName) {
					case "datatypeLibrary":
					case "type":
					case "ns":
						break;
					default:
						throw new RelaxngException ("Invalid attribute.");
					}
				} while (MoveToNextAttribute ());
				MoveToElement ();
			}

			if (MoveToAttribute ("type")) {
				v.Type = Value.Trim ();
				v.DatatypeLibrary = DatatypeLibrary;
			} else {
				v.Type = "token";
				v.DatatypeLibrary = "";
			}
//			v.Namespace = GetSpaceStrippedAttribute ("ns", String.Empty);
			MoveToElement ();
			if (IsEmptyElement) {
				v.Value = String.Empty;
				Read ();
			} else {
				v.Value = ReadString ();
				expectEnd ("value");
			}

			return v;
		}

		private RelaxngList ReadListPattern ()
		{
			RelaxngList list = new RelaxngList ();
			FillLocation (list);
			expect ("list");
			Read ();
			ReadPatterns (list);
			expectEnd ("list");
			return list;
		}

		private RelaxngOneOrMore ReadOneOrMorePattern ()
		{
			RelaxngOneOrMore o = new RelaxngOneOrMore ();
			FillLocation (o);
			expect ("oneOrMore");
			Read ();
			ReadPatterns (o);
			expectEnd ("oneOrMore");
			return o;
		}

		private RelaxngZeroOrMore ReadZeroOrMorePattern ()
		{
			RelaxngZeroOrMore o = new RelaxngZeroOrMore ();
			FillLocation (o);
			expect ("zeroOrMore");
			Read ();
			ReadPatterns (o);
			expectEnd ("zeroOrMore");
			return o;
		}

		private RelaxngOptional ReadOptionalPattern ()
		{
			RelaxngOptional o = new RelaxngOptional ();
			FillLocation (o);
			expect ("optional");
			Read ();
			ReadPatterns (o);
			expectEnd ("optional");
			return o;
		}

		private RelaxngMixed ReadMixedPattern ()
		{
			RelaxngMixed o = new RelaxngMixed ();
			FillLocation (o);
			expect ("mixed");
			Read ();
			ReadPatterns (o);
			expectEnd ("mixed");
			return o;
		}

		private RelaxngGroup ReadGroupPattern ()
		{
			RelaxngGroup g = new RelaxngGroup ();
			FillLocation (g);
			expect ("group");
			Read ();
			ReadPatterns (g);
			expectEnd ("group");
			return g;
		}

		private RelaxngInterleave ReadInterleavePattern ()
		{
			RelaxngInterleave i = new RelaxngInterleave ();
			FillLocation (i);
			expect ("interleave");
			Read ();
			ReadPatterns (i);
			expectEnd ("interleave");
			return i;
		}

		private RelaxngChoice ReadChoicePattern ()
		{
			RelaxngChoice c = new RelaxngChoice ();
			FillLocation (c);
			expect ("choice");
			Read ();
			ReadPatterns (c);
			expectEnd ("choice");
			return c;
		}

		private RelaxngNotAllowed ReadNotAllowedPattern ()
		{
			expect ("notAllowed");
			if (!IsEmptyElement) {
				Read ();
				expectEnd ("notAllowed");
			}
			else
				Read ();
			RelaxngNotAllowed na = new RelaxngNotAllowed ();
			FillLocation (na);
			return na;
		}

		public override string ReadString ()
		{
			skipExternal = false;
			string s = base.ReadString ();
			skipExternal = true;
			return s;
		}
	}
}