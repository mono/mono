//
// Commons.Xml.Relaxng.RelaxngReader.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//

using System;
using System.Collections;
using System.Xml;

namespace Commons.Xml.Relaxng
{
	public class RelaxngReader : XmlDefaultReader
	{
		// static members.
		static RngPattern relaxngGrammar;
		static XmlReader relaxngXmlReader;
		static RelaxngReader ()
		{
//			relaxngXmlReader = new XmlTextReader ("relaxng.rng");
//			relaxngGrammar = RngPattern.Read (relaxngXmlReader);
		}

		public static string RngNS = "http://relaxng.org/ns/structure/1.0";
//		public static RngPattern RelaxngGrammar {
//			get { return relaxngGrammar; }
//		}


		// fields
		Stack nsStack = new Stack ();
		Stack datatypeLibraryStack = new Stack ();

		// ctor
		public RelaxngReader (XmlReader reader)
			: this (reader, null)
		{
		}

		public RelaxngReader (XmlReader reader, string ns)
//			: base (reader == relaxngXmlReader ? reader : new RelaxngValidatingReader (reader, relaxngGrammar))
			: base (reader)
		{
			nsStack.Push (ns == null ? "" : ns);
			datatypeLibraryStack.Push ("");

			if (Reader.ReadState == ReadState.Initial)
				Read ();
			MoveToContent ();
		}

		// public
		public override bool Read ()
		{
			bool skipRead = false;
			bool b = false;
			bool loop = true;
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
				default:
					if (NamespaceURI != RngNS) {
						Reader.Skip ();
						skipRead = true;
					}
					else
						loop = false;
					break;
				}
			} while (b && loop);

			switch (NodeType) {
			case XmlNodeType.Element:
				if (!IsEmptyElement) {
					if (MoveToAttribute ("ns"))
						nsStack.Push (Value.Trim ());
					else
						nsStack.Push (nsStack.Peek ());

					if (MoveToAttribute ("datatypeLibrary"))
						datatypeLibraryStack.Push (Value.Trim ());
					else
						datatypeLibraryStack.Push (datatypeLibraryStack.Peek ());
					MoveToElement ();
				}
				break;
			case XmlNodeType.EndElement:
				nsStack.Pop ();
				datatypeLibraryStack.Pop ();
				break;
			}
			return b;
		}

		/*
		public override XmlNodeType MoveToContent ()
		{
			MoveToElement ();

			switch (NodeType) {
			case XmlNodeType.ProcessingInstruction:
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
			case XmlNodeType.Comment:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				Read ();
				break;
			default:
				break;
			}
			return NodeType;
		}
		*/

		// Properties

		public string NS {
			get { return nsStack.Peek () as string; }
		}

		public string DatatypeLibrary {
			get { return datatypeLibraryStack.Peek () as string; }
		}

		// Utility methods.
		private void expect (string name)
		{
			if (NamespaceURI != RngGrammar.NamespaceURI)
				throw new RngException (String.Format ("Invalid document: expected namespace {0} but found {1}", RngGrammar.NamespaceURI, NamespaceURI));
			else if (LocalName != name)
				throw new RngException (String.Format ("Invalid document: expected local name {0} but found {1}", name, LocalName));
		}

		private void expectEnd (string name)
		{
			if (NodeType != XmlNodeType.EndElement)
				throw new RngException (String.Format ("Expected EndElement but found {0}.", NodeType));
			expect (name);

			Read ();
		}

		// Other than name class and pattern.
		public RngStart ReadStart ()
		{
			RngStart s = new RngStart ();
			expect ("start");
			if (MoveToAttribute ("combine")) {
				s.Combine = Value.Trim ();
				if (s.Combine != "choice" && s.Combine != "interleave")
					throw new RngException ("Invalid combine attribute: " + s.Combine);
			}

			MoveToElement ();
			Read ();
			s.Pattern = ReadPattern ();
			expectEnd ("start");
			return s;
		}

		private string GetSpaceStrippedAttribute (string name)
		{
			string v = GetAttribute (name);
			return v != null ? v.Trim () : null;
		}

		public RngDefine ReadDefine ()
		{
			RngDefine def = new RngDefine ();
			expect ("define");
			def.Name = GetSpaceStrippedAttribute ("name");

			Read ();
			while (NodeType == XmlNodeType.Element)
				def.Patterns.Add (ReadPattern ());
			expectEnd ("define");
			return def;
		}

		public RngParam ReadParam ()
		{
			RngParam p = new RngParam ();
			expect ("param");
			p.Name = GetSpaceStrippedAttribute ("name");
			p.Value = ReadString ().Trim ();
			expectEnd ("param");
			return p;
		}

		// NameClass reader (only if it is element-style.)
		public RngNameClass ReadNameClass ()
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
			throw new RngException ("Invalid name class: " + LocalName);
		}

		public RngName ReadNameClassName ()
		{
			string name = ReadString ().Trim ();
			RngName rName = resolvedName (name);
			expectEnd ("name");
			return rName;
		}

		public RngAnyName ReadNameClassAnyName ()
		{
			RngAnyName an = new RngAnyName ();
			if (!IsEmptyElement) {
				Read ();
				if (NodeType == XmlNodeType.EndElement) {
				} else {
					// expect except
					expect ("except");
					Read ();
					an.Except = new RngExceptNameClass ();
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

		public RngNsName ReadNameClassNsName ()
		{
			RngNsName nn = new RngNsName (this.NS);
			if (!IsEmptyElement) {
				Read ();
				if (NodeType == XmlNodeType.EndElement) {
				} else {
					// expect except
					expect ("except");
					Read ();
					nn.Except = new RngExceptNameClass ();
					expectEnd ("except");
				}
				expectEnd ("nsName");
			} else
				Read ();
			return nn;
		}

		public RngNameChoice ReadNameClassChoice ()
		{
			RngNameChoice nc = new RngNameChoice ();
			if (IsEmptyElement)
				throw new RngException ("Name choice must have at least one name class.");

			Read ();
			while (NodeType != XmlNodeType.EndElement) {
				nc.Children.Add (ReadNameClass ());
			}
			if (nc.Children.Count == 0)
				throw new RngException ("Name choice must have at least one name class.");

			expectEnd ("choice");
			return nc;
		}

		public RngExceptNameClass ReadNameClassExcept ()
		{
			RngExceptNameClass x = new RngExceptNameClass ();
			if (IsEmptyElement)
				throw new RngException ("Name choice must have at least one name class.");

			Read ();
			while (NodeType != XmlNodeType.EndElement)
				x.Names.Add (ReadNameClass ());
			if (x.Names.Count == 0)
				throw new RngException ("Name choice must have at least one name class.");

			expectEnd ("except");
			return x;
		}

		// Pattern reader

		public RngPattern ReadPattern ()
		{
			while (NodeType != XmlNodeType.Element)
				if (!Read ())
					break;

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
			throw new RngException ("Non-supported pattern specification: " + LocalName);
		}

		private void ReadPatterns (RngSingleContentPattern el)
		{
			do {
				el.Patterns.Add (ReadPattern ());
			} while (NodeType == XmlNodeType.Element);
		}

		private void ReadPatterns (RngBinaryContentPattern el)
		{
			do {
				el.Patterns.Add (ReadPattern ());
			} while (NodeType == XmlNodeType.Element);
		}

		public RngExcept ReadPatternExcept ()
		{
			RngExcept x = new RngExcept ();
			if (IsEmptyElement)
				throw new RngException ("'except' must have at least one pattern.");
			Read ();
			while (NodeType != XmlNodeType.EndElement)
				x.Patterns.Add (ReadPattern ());
			if (x.Patterns.Count == 0)
				throw new RngException ("'except' must have at least one pattern.");

			expectEnd ("except");
			return x;
		}

		public RngInclude ReadInclude ()
		{
			RngInclude i = new RngInclude ();
			expect ("include");
			string href = GetSpaceStrippedAttribute ("href");
			i.Href = Util.ResolveUri (BaseURI, href);
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
						throw new RngException ("Unexpected content: " + Name);
					break;
				default:
					throw new RngException ("Unexpected content: " + Name);
				}
			}
		}

		public RngDiv ReadDiv (bool allowIncludes)
		{
			expect ("div");
			RngDiv div = new RngDiv ();
			if (!IsEmptyElement) {
				Read ();
				readGrammarIncludeContent (div.Starts, div.Defines, div.Divs, div.Includes);
				expectEnd ("div");
			}
			else
				Read ();
			return div;
		}

		private RngName resolvedName (string nameSpec)
		{
			int colonAt = nameSpec.IndexOf (':');
			string prefix = (colonAt < 0) ? "" : nameSpec.Substring (0, colonAt);
			string local = (colonAt < 0) ? nameSpec : nameSpec.Substring (colonAt + 1, nameSpec.Length - colonAt - 1);
			string uri = NS;

			if (prefix != "") {
				uri = LookupNamespace (prefix);
				if (uri == null)
					throw new RngException ("Undeclared prefix in name component: " + nameSpec);
			}
			return new RngName (local, uri);
		}

		public RngElement ReadElementPattern ()
		{
			RngElement el = new RngElement ();

			// try to get name from attribute.
			if (MoveToAttribute ("name")) {
				el.NameClass = resolvedName (Value.Trim ());
			}
			MoveToElement ();
			Read ();

			// read nameClass from content.
			if (el.NameClass == null)
				el.NameClass = ReadNameClass ();

			// read patterns.
			this.ReadPatterns (el);

			expectEnd ("element");

			return el;
		}

		public RngAttribute ReadAttributePattern ()
		{
			RngAttribute attr = new RngAttribute ();

			// try to get name from attribute.
			if (MoveToAttribute ("name"))
				attr.NameClass = resolvedName (Value.Trim ());

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
			return attr;
		}

		public RngGrammar ReadGrammarPattern ()
		{
			RngGrammar grammar = new RngGrammar ();
			Read ();
			this.readGrammarIncludeContent (grammar.Starts, grammar.Defines, grammar.Divs, grammar.Includes);
			expectEnd ("grammar");

			return grammar;
		}

		public RngRef ReadRefPattern ()
		{
			RngRef r = new RngRef ();
			expect ("ref");
			r.Name = GetSpaceStrippedAttribute ("name");
			if (!IsEmptyElement) {
				Read ();
				expectEnd ("ref");
			}
			else
				Read ();
			return r;
		}

		public RngExternalRef ReadExternalRefPattern ()
		{
			RngExternalRef r = new RngExternalRef ();
			expect ("externalRef");
			string href = GetSpaceStrippedAttribute ("href");
			r.Href = Util.ResolveUri (BaseURI, href);
			r.NSContext = NS;
			if (!IsEmptyElement) {
				Read ();
				expectEnd ("externalRef");
			}
			else
				Read ();
			return r;
		}

		public RngParentRef ReadParentRefPattern ()
		{
			RngParentRef r = new RngParentRef ();
			expect ("parentRef");
			r.Name = GetSpaceStrippedAttribute ("name");
			if (!IsEmptyElement) {
				Read ();
				expectEnd ("parentRef");
			}
			else
				Read ();
			return r;
		}

		public RngEmpty ReadEmptyPattern ()
		{
			expect ("empty");
			if (!IsEmptyElement) {
				Read ();
				expectEnd ("empty");
			}
			else
				Read ();

			return RngEmpty.Instance;
		}

		public RngText ReadTextPattern ()
		{
			expect ("text");
			if (!IsEmptyElement) {
				Read ();
				expectEnd ("text");
			}
			else
				Read ();

			return RngText.Instance;
		}

		public RngData ReadDataPattern ()
		{
			RngData data = new RngData ();

			expect ("data");
			data.Type = GetSpaceStrippedAttribute ("type");
			data.DatatypeLibrary = DatatypeLibrary;

			if (!IsEmptyElement) {
				Read ();
				while (Name == "param") {
					data.ParamList.Add (ReadParam ());
				}
				if (LocalName == "except")
					data.Except = ReadPatternExcept ();
				expectEnd ("data");
			} else
				Read ();

			return data;
		}

		public RngValue ReadValuePattern ()
		{
			RngValue v = new RngValue ();
			expect ("value");
			if (MoveToAttribute ("type")) {
				v.Type = Value.Trim ();
				v.DatatypeLibrary = DatatypeLibrary;
			} else {
				v.Type = "token";
				v.DatatypeLibrary = "";
			}
			v.Namespace = GetSpaceStrippedAttribute ("ns");
			MoveToElement ();
			v.Value = ReadString ().Trim ();
			expectEnd ("value");

			return v;
		}

		public RngList ReadListPattern ()
		{
			RngList list = new RngList ();
			expect ("list");
			Read ();
			ReadPatterns (list);
			expectEnd ("list");
			return list;
		}

		public RngOneOrMore ReadOneOrMorePattern ()
		{
			RngOneOrMore o = new RngOneOrMore ();
			expect ("oneOrMore");
			Read ();
			ReadPatterns (o);
			expectEnd ("oneOrMore");
			return o;
		}

		public RngZeroOrMore ReadZeroOrMorePattern ()
		{
			RngZeroOrMore o = new RngZeroOrMore ();
			expect ("zeroOrMore");
			Read ();
			ReadPatterns (o);
			expectEnd ("zeroOrMore");
			return o;
		}

		public RngOptional ReadOptionalPattern ()
		{
			RngOptional o = new RngOptional ();
			expect ("optional");
			Read ();
			ReadPatterns (o);
			expectEnd ("optional");
			return o;
		}

		public RngMixed ReadMixedPattern ()
		{
			RngMixed o = new RngMixed ();
			expect ("mixed");
			Read ();
			ReadPatterns (o);
			expectEnd ("mixed");
			return o;
		}

		public RngGroup ReadGroupPattern ()
		{
			RngGroup g = new RngGroup ();
			expect ("group");
			Read ();
			ReadPatterns (g);
			expectEnd ("group");
			return g;
		}

		public RngInterleave ReadInterleavePattern ()
		{
			RngInterleave i = new RngInterleave ();
			expect ("interleave");
			Read ();
			ReadPatterns (i);
			expectEnd ("interleave");
			return i;
		}

		public RngChoice ReadChoicePattern ()
		{
			RngChoice c = new RngChoice ();
			expect ("choice");
			Read ();
			ReadPatterns (c);
			expectEnd ("choice");
			return c;
		}

		public RngNotAllowed ReadNotAllowedPattern ()
		{
			expect ("notAllowed");
			if (!IsEmptyElement) {
				Read ();
				expectEnd ("notAllowed");
			}
			else
				Read ();
			return RngNotAllowed.Instance;
		}
	}
}