//
// XQueryTokenizer.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Xml;
using System.Xml.Query;
using System.Xml.Schema;
using System.Xml.XPath;
using Mono.Xml.XQuery;
using Mono.Xml.XPath2;

namespace Mono.Xml.XQuery.Parser
{
	// FIXME: make internal in the future
	public class XQueryTokenizer
		: Mono.Xml.XQuery.Parser.yyParser.yyInput, IXmlLineInfo
	{
		int line = 1;
		int column = 0;
		bool nextIncrementLine;

		// namespace resolver
		XmlNamespaceManager nsResolver;
		string defaultFunctionNamespace = XQueryFunction.Namespace;

		// input source
		TextReader source;
		int peekChar = -1;

		// token info
		int currentToken;
		string prefixName;
		object tokenValue;

		int lookAheadToken = -1;
		object lookAheadTokenValue;

		// state info
		WhitespaceHandling ws = WhitespaceHandling.Arbitrary;
		ParseState state = ParseState.Default;
		Stack stateStack;

		public XQueryTokenizer (TextReader reader)
		{
			this.source = reader;

			stateStack = new Stack ();

			nsResolver = new XmlNamespaceManager (new NameTable ());
			nsResolver.AddNamespace ("xs", XmlSchema.Namespace);
			nsResolver.AddNamespace ("xdt", XmlSchema.XdtNamespace);
			// FIXME: Are they really predefined?
			nsResolver.AddNamespace ("xsi", XmlSchema.InstanceNamespace);
			nsResolver.AddNamespace ("fn", "http://www.w3.org/2003/11/xpath-functions");
			nsResolver.AddNamespace ("local", "http://www.w3.org/2003/11/xquery-local-functions");
		}

		internal IXmlNamespaceResolver NSResolver {
			get { return nsResolver; }
		}

		internal string DefaultFunctionNamespace {
			get { return defaultFunctionNamespace; }
			set { defaultFunctionNamespace = value; }
		}

		public void AddNamespace (string prefix, string ns)
		{
			nsResolver.AddNamespace (prefix, ns);
		}

		public bool advance ()
		{
			if (currentToken < 0)
				return false;
			if (lookAheadToken >= 0) {
				tokenValue = lookAheadTokenValue;
				currentToken = lookAheadToken;
				lookAheadToken = -1;
			}
			else
				currentToken = ParseToken ();
			return currentToken >= 0;
		}

		public int token ()
		{
			return currentToken;
		}

		public object value ()
		{
			return tokenValue;
		}

		public bool HasLineInfo ()
		{
			return true;
		}

		public int LineNumber {
			get { return line; }
		}

		public int LinePosition {
			get { return column; }
		}

		internal WhitespaceHandling Space {
			get { return ws; }
			set { ws = value; }
		}

		internal ParseState State {
			get { return state; }
			set {
//				Console.Error.WriteLine ("**** eno **** state transition from {0} to {1}, stack count = {2}", state, value, stateStack.Count);
//foreach (ParseState ps in stateStack.ToArray ()) Console.Error.WriteLine ("***** eno ***** " + ps);
				state = value;
			}
		}

		internal void PushState (ParseState newState)
		{
			stateStack.Push (newState);
//			Console.Error.WriteLine ("**** eno **** state pushed {0}, added stack count = {1}", newState, stateStack.Count);
//foreach (ParseState ps in stateStack.ToArray ()) Console.Error.WriteLine ("***** eno ***** " + ps);
		}

		internal void PopState ()
		{
			if (stateStack.Count == 0)
				throw Error ("Internal state transition error. State stack is empty.");
			state = (ParseState) stateStack.Pop ();
//			Console.Error.WriteLine ("**** eno **** state pop, now as {0}, stack count = {1}", state, stateStack.Count);
//foreach (ParseState ps in stateStack.ToArray ()) Console.Error.WriteLine ("***** eno ***** " + ps);
		}

		private XmlQueryCompileException Error (string message)
		{
			return new XmlQueryCompileException (message, this, null, null);
		}

		private int ParseToken ()
		{
			switch (ws) {
			case WhitespaceHandling.Arbitrary:
				SkipWhitespaces ();
				break;
			case WhitespaceHandling.Explicit:
				if (!XmlChar.IsWhitespace (PeekChar ()))
					throw Error ("Whitespace is required.");
				goto case WhitespaceHandling.Arbitrary;
			}

			int c = PeekChar ();
			if (c < 0)
				return -1;

			// FIXME: consider DOUBLE_LITERAL
			if (Char.IsNumber ((char) c)) {
				tokenValue = ReadDecimal (false);
				return Token.DECIMAL_LITERAL;
			}

			switch (state) {
			case ParseState.OccurenceIndicator:
				return ParseOccurenceIndicator ();
			case ParseState.XmlPIContent:
				return ParseXmlPIContent ();
			default:
				return ParseDefault ();
			}
		}

		private int ParseXmlPIContent ()
		{
			// FIXME: handle ??> correctly
			while (true) {
				int c = PeekChar ();
				if (c < 0)
					throw Error ("Unexpected end of query text inside XML processing instruction content");
				if (c == '?') {
					ReadChar ();
					if (PeekChar () == '>') {
						ReadChar ();
						tokenValue = CreateValueString ();
						return Token.XML_PI_TO_END;
					}
					else
						AddValueChar ('?');
				}
				else
					AddValueChar ((char) c);
			}
		}

		private int ParseXmlCommentContent ()
		{
			// FIXME: handle ---> correctly
			while (true) {
				int c = PeekChar ();
				if (c < 0)
					throw Error ("Unexpected end of query text inside XML processing instruction content");
				if (c == '-') {
					ReadChar ();
					if (PeekChar () == '-') {
						ReadChar ();
						if (PeekChar () == '>') {
							tokenValue = CreateValueString ();
							return Token.XML_COMMENT_TO_END;
						} else {
							AddValueChar ('-');
							AddValueChar ('-');
						}
					}
					else
						AddValueChar ('-');
				}
				else
					AddValueChar ((char) c);
			}
		}

		private int ParseOccurenceIndicator ()
		{
			state = ParseState.Operator;
			switch (PeekChar ()) {
			case '?':
				ReadChar ();
				return Token.QUESTION;
			case '*':
				ReadChar ();
				return Token.ASTERISK;
			case '+':
				ReadChar ();
				return Token.PLUS;
			default:
				return ParseOperator ();
			}
		}

		private int ParseOperator ()
		{
			// TODO: implement
			return ParseDefault ();
		}

		private int ParseDefault ()
		{
			int c = ReadChar ();
			switch (c) {
			case '.':
				if (PeekChar () == '.') {
					ReadChar ();
					return Token.DOT2;
				}
				else if (Char.IsNumber ((char) PeekChar ())) {
					tokenValue = ReadDecimal (true);
				}
				return Token.DOT;
			case ',':
				return Token.COMMA;
			case ';':
				return Token.SEMICOLON;
			case '(':
				if (PeekChar () == ':') {
					ReadChar ();
					if (PeekChar () == ':') {
						ReadChar ();
						return Token.PRAGMA_OPEN;
					}
					return Token.OPEN_PAREN_COLON;
				}
				return Token.OPEN_PAREN;
			case ')':
				return Token.CLOSE_PAREN;
			case ':':
				switch (PeekChar ()) {
				case ':':
					ReadChar ();
					if (PeekChar () == ')') {
						ReadChar ();
						return Token.PRAGMA_CLOSE;
					}
					return Token.COLON2;
				case ')':
					ReadChar ();
					return Token.CLOSE_PAREN_COLON;
				case '=':
					ReadChar ();
					return Token.COLON_EQUAL;
				}
				return Token.COLON;
			case '[':
				return Token.OPEN_BRACKET;
			case ']':
				return Token.CLOSE_BRACKET;
			case '{':
				return Token.OPEN_CURLY;
			case '}':
				return Token.CLOSE_CURLY;
			case '$':
				return Token.DOLLAR;
			case '\'':
				// FIXME: consider in the future
/*
				if (state == ParseState.StartTag) {
					if (PeekChar () == '\'') {
						// FIXME: this code is VERY inefficient
						ReadChar ();
						tokenValue = "'";
						return Token.STRING_LITERAL;
					}
					return Token.APOS;
				}
*/
				tokenValue = ReadQuoted ('\'');
				return Token.STRING_LITERAL;
			case '"':
				// FIXME: consider in the future
/*
				if (state == ParseState.StartTag) {
					if (PeekChar () == '"') {
						// FIXME: this code is VERY inefficient
						ReadChar ();
						tokenValue = "\"";
						return Token.STRING_LITERAL;
					}
					return Token.QUOT;
				}
*/
				tokenValue = ReadQuoted ('"');
				return Token.STRING_LITERAL;
			case '=':
				return Token.EQUAL;
			case '<':
				// only happens when state is ElementContent 
				// (otherwise it might be "/foo</bar")
				if (state == ParseState.ElementContent) {
					switch ((char) PeekChar ()) {
					case '/':
						ReadChar ();
						return Token.END_TAG_START;
					case '!':
						ReadChar ();
						switch (PeekChar ()) {
						case '-':
							ReadChar ();
							if (ReadChar () != '-')
								throw Error ("Invalid sequence of characters '<!-'.");
							
							return Token.XML_COMMENT_START;
						case '[':
							ReadChar ();
							Expect ("CDATA[");
							return Token.XML_CDATA_START;
						}
						throw Error ("Invalid sequence of characters '<!'.");
					case '?':
						ReadChar ();
						return Token.XML_PI_START;
					default:
						return Token.LESSER;
					}
				}

				switch (PeekChar ()) {
				case '<':
					ReadChar ();
					return Token.LESSER2;
				case '=':
					ReadChar ();
					return Token.LESSER_EQUAL;
				}
				return Token.LESSER;
			case '>':
				switch (PeekChar ()) {
				case '>':
					ReadChar ();
					return Token.GREATER2;
				case '=':
					ReadChar ();
					return Token.GREATER_EQUAL;
				}
				return Token.GREATER;
			case '|':
				return Token.BAR;
			case '*':
				if (PeekChar () == ':') {
					ReadChar ();
					// FIXME: more check
					tokenValue = new XmlQualifiedName (ReadOneToken (), "*");
					return Token.WILD_PREFIX;
				}
				return Token.ASTERISK;
			case '+':
				return Token.PLUS;
			case '-':
				return Token.MINUS;
			case '/':
				// only happens when state is StartTag
				// (otherwise it might be "/>$extvar")
				if (state == ParseState.StartTag && PeekChar () == '>') {
					ReadChar ();
					return Token.EMPTY_TAG_CLOSE;
				}
				if (PeekChar () == '/') {
					ReadChar ();
					return Token.SLASH2;
				}
				return Token.SLASH;
			case '?':
				return Token.QUESTION;
			case '@':
				return Token.AT;
			}

			peekChar = c;
			prefixName = null;
			string name = ReadOneToken ();

			tokenValue = name;
			bool validKeyword = false;

			switch (state) {
			case ParseState.XmlSpaceDecl:
				switch (name) {
				case "preserve":
					return Token.PRESERVE;
				case "strip":
					return Token.STRIP;
				}
				break;
			case ParseState.CloseKindTest:
				if (name == "nillable")
					return Token.NILLABLE;
				break;
			case ParseState.ExtKey:
				switch (name) {
				case "pragma":
					return Token.PRAGMA;
				case "extension":
					return Token.EXTENSION;
				}
				break;
			case ParseState.KindTest:
				switch (name) {
				case "context":
					return Token.CONTEXT;
				case "element":
					return Token.ELEMENT;
				case "global":
					return Token.GLOBAL;
				case "type":
					return Token.TYPE;
				}
				break;
			case ParseState.ItemType:
				switch (name) {
				case "attribute":
					return Token.ATTRIBUTE;
				case "comment":
					return Token.COMMENT;
				case "document-node":
					return Token.DOCUMENT_NODE;
				case "element":
					return Token.ELEMENT;
				case "empty":
					return Token.EMPTY;
				case "item":
					return Token.ITEM;
				case "node":
					return Token.NODE;
				case "processing-instruction":
					return Token.PROCESSING_INSTRUCTION;
				case "text":
					return Token.TEXT;
				}
				break;
			case ParseState.NamespaceKeyword:
				switch (name) {
				case "declare":
					return Token.DECLARE;
				case "default":
					return Token.DEFAULT;
				case "element":
					return Token.ELEMENT;
				case "function":
					return Token.FUNCTION;
				case "namespace":
					return Token.NAMESPACE;
				}
				break;
			case ParseState.OccurenceIndicator:
			case ParseState.Operator:
				switch (name) {
				case "and":
				case "as":
				case "ascending":
				case "at":
				case "base-uri":
				case "by":
				case "case":
				case "cast":
				case "castable":
				case "collation":
				case "declare":
				case "default":
				case "descending":
				case "div":
				case "element":
				case "else":
				case "empty":
				case "eq":
				case "every":
				case "except":
				case "external":
				case "for":
				case "function":
				case "ge":
				case "global":
				case "greatest":
				case "gt":
				case "idiv":
				case "import":
				case "in":
				case "instance":
				case "intersect":
				case "is":
				case "lax":
				case "le":
				case "least":
				case "let":
				case "lt":
				case "mod":
				case "module":
				case "namespace":
				case "ne":
				case "of":
				case "or":
				case "order":
				case "ordered":
				case "ordering":
				case "return":
				case "satisfies":
				case "schema":
				case "skip":
				case "some":
				case "stable":
				case "strict":
				case "then":
				case "to":
				case "treat":
				case "typwswitch":
				case "union":
				case "unordered":
				case "variable":
				case "where":
				case "xmlspace":
					validKeyword = true;
					break;
				}
				break;
			case ParseState.Default:
				switch (name) {
				case "ancestor":
				case "ancestor-or-self":
				case "as":
				case "attribute":
				case "base-uri":
				case "child":
				case "collation":
				case "comment":
				case "construction":
				case "declare":
				case "default":
				case "descendant":
				case "descendant-or-self":
				case "document":
				case "document-node":
				case "element":
				case "every":
				case "following":
				case "following-sibling":
				case "for":
				case "function":
				case "global":
				case "if":
				case "import":
				case "lax":
				case "let":
				case "module":
				case "namespace":
				case "node":
				case "ordered":
				case "parent":
				case "preceding":
				case "preceding-sibling":
				case "processing-instruction":
				case "schema":
				case "self":
				case "some":
				case "strict":
				case "strip":
				case "text":
				case "typeswitch":
				case "unordered":
				case "validate":
				case "validation":
				case "version":
				case "xmlspace":
				case "xquery":
					validKeyword = true;
					break;
				}
				break;
			}

			if (validKeyword) {
				switch (name) {
				case "xquery":
					return Token.XQUERY;
				case "version":
					return Token.VERSION;
				case "pragma":
					return Token.PRAGMA;
				case "extension":
					return Token.EXTENSION;
				case "module":
					return Token.MODULE;
				case "namespace":
					return Token.NAMESPACE;
				case "declare":
					return Token.DECLARE;
				case "xmlspace":
					return Token.XMLSPACE;
				case "preserve":
					return Token.PRESERVE;
				case "strip":
					return Token.STRIP;
				case "default":
					return Token.DEFAULT;
				case "construction":
					return Token.CONSTRUCTION;
				case "ordering":
					return Token.ORDERING;
				case "ordered":
					return Token.ORDERED;
				case "unordered":
					return Token.UNORDERED;
				case "document-node":
					return Token.DOCUMENT_NODE;
				case "document":
					return Token.DOCUMENT;
				case "element":
					return Token.ELEMENT;
				case "attribute":
					return Token.ATTRIBUTE;
				case "processing-instruction":
					return Token.PROCESSING_INSTRUCTION;
				case "comment":
					return Token.COMMENT;
				case "text":
					return Token.TEXT;
				case "node":
					return Token.NODE;
				case "function":
					return Token.FUNCTION;
				case "collation":
					return Token.COLLATION;
				case "base-uri":
					return Token.BASEURI;
				case "import":
					return Token.IMPORT;
				case "schema":
					return Token.SCHEMA;
				case "at":
					return Token.AT;
				case "variable":
					return Token.VARIABLE;
				case "as":
					return Token.AS;
				case "external":
					return Token.EXTERNAL;
				case "validation":
					return Token.VALIDATION;
				case "lax":
					return Token.LAX;
				case "strict":
					return Token.STRICT;
				case "skip":
					return Token.SKIP;
				case "return":
					return Token.RETURN;
				case "for":
					return Token.FOR;
				case "let":
					return Token.LET;
				case "in":
					return Token.IN;
				case "where":
					return Token.WHERE;
				case "order":
					return Token.ORDER;
				case "by":
					return Token.BY;
				case "stable":
					return Token.STABLE;
				case "ascending":
					return Token.ASCENDING;
				case "descending":
					return Token.DESCENDING;
				case "empty":
					return Token.EMPTY;
				case "greatest":
					return Token.GREATEST;
				case "least":
					return Token.LEAST;
				case "some":
					return Token.SOME;
				case "every":
					return Token.EVERY;
				case "satisfies":
					return Token.SATISFIES;
				case "is":
					return Token.IS;
				case "to":
					return Token.TO;
				case "eq":
					return Token.EQ;
				case "ne":
					return Token.NE;
				case "lt":
					return Token.LT;
				case "le":
					return Token.LE;
				case "gt":
					return Token.GT;
				case "ge":
					return Token.GE;
				case "and":
					return Token.AND;
				case "or":
					return Token.OR;
				case "instance":
					return Token.INSTANCE;
				case "of":
					return Token.OF;
				case "if":
					return Token.IF;
				case "then":
					return Token.THEN;
				case "else":
					return Token.ELSE;
				case "typeswitch":
					return Token.TYPESWITCH;
				case "case":
					return Token.CASE;
				case "treat":
					return Token.TREAT;
				case "castable":
					return Token.CASTABLE;
				case "cast":
					return Token.CAST;
				case "div":
					return Token.DIV;
				case "idiv":
					return Token.IDIV;
				case "mod":
					return Token.MOD;
				case "union":
					return Token.UNION;
				case "intersect":
					return Token.INTERSECT;
				case "except":
					return Token.EXCEPT;
				case "validate":
					return Token.VALIDATE;
				case "context":
					return Token.CONTEXT;
				case "nillable":
					return Token.NILLABLE;
				case "item":
					return Token.ITEM;
				case "global":
					return Token.GLOBAL;
				case "type":
					return Token.TYPE;
				case "child":
					return Token.CHILD;
				case "descendant":
					return Token.DESCENDANT;
				case "self":
					return Token.SELF;
				case "descendant-or-self":
					return Token.DESCENDANT_OR_SELF;
				case "following-sibling":
					return Token.FOLLOWING_SIBLING;
				case "following":
					return Token.FOLLOWING;
				case "parent":
					return Token.PARENT;
				case "ancestor":
					return Token.ANCESTOR;
				case "preceding":
					return Token.PRECEDING;
				case "preceding-sibling":
					return Token.PRECEDING_SIBLING;
				case "ancestor-or-self":
					return Token.ANCESTOR_OR_SELF;
				}
			}

			switch (state) {
			case ParseState.NamespaceDecl:
			case ParseState.NamespaceKeyword:
			case ParseState.XmlSpaceDecl:
			case ParseState.KindTestForPI:
			case ParseState.XmlPI:
				return Token.NCNAME;
			}

			if (PeekChar () == ':') {
				ReadChar ();
				prefixName = name;
				switch (PeekChar ()) {
				case '*':
					ReadChar ();
					name = "*";
					break;
				case '=': // ex. let foo:= ...
					ReadChar ();
					tokenValue = new XmlQualifiedName (name, nsResolver.DefaultNamespace);
					lookAheadToken = Token.COLON_EQUAL;
					return Token.QNAME;
				default:
					name = ReadOneToken ();
					break;
				}

				string ns = nsResolver.LookupNamespace (prefixName);
				if (ns == null)
					throw Error (String.Format ("Prefix '{0}' is not mapped to any namespace URI.", prefixName));
				tokenValue = new XmlQualifiedName (name, ns);
				prefixName = null;
				return name == "*" ? Token.WILD_LOCALNAME : Token.QNAME;
			}
			tokenValue = new XmlQualifiedName (name);
			return Token.QNAME;
		}

		private int PeekChar ()
		{
			if (peekChar == -1)
				peekChar = source.Read ();
			return peekChar;
		}

		private int ReadChar ()
		{
			int ret;
			if (peekChar != -1) {
				ret = peekChar;
				peekChar = -1;
			}
			else
				ret = source.Read ();

			if (nextIncrementLine) {
				line++;
				column = 0;
				nextIncrementLine = false;
			}
			column++;
			switch (ret) {
			case '\r':
				break;
			case '\n':
				nextIncrementLine = true;
				goto default;
			default:
				break;
			}

			return ret;
		}

		private void SkipWhitespaces ()
		{
			while (true) {
				switch (PeekChar ()) {
				case ' ':
				case '\t':
				case '\r':
				case '\n':
					ReadChar ();
					continue;
				default:
					return;
				}
			}
		}

		char [] buffer = new char [30];
		int bufferIndex;

		private void AddValueChar (char c)
		{
			if (bufferIndex == buffer.Length) {
				char [] newBuf = new char [bufferIndex * 2];
				Array.Copy (buffer, newBuf, bufferIndex);
				buffer = newBuf;
			}
			buffer [bufferIndex++] = c;
		}

		private string CreateValueString ()
		{
			return new string (buffer, 0, bufferIndex);
		}

		private void Expect (string expected)
		{
			for (int i = 0; i < expected.Length; i++)
				if (ReadChar () != expected [i])
					throw Error (String.Format ("Expected token '{0}' did not appear.", expected));
		}

		// TODO: parse three quoted
		private string ReadQuoted (char quoteChar)
		{
			bufferIndex = 0;
			bool loop = true;
			do {
				int c = ReadChar ();
				switch (c) {
				case -1:
				case '"':
					if (quoteChar == '"')
						loop = false;
					break;
				case '\'':
					if (quoteChar == '\'')
						loop = false;
					break;
				default:
					AddValueChar ((char) c);
					break;
				}
			} while (loop);

			return CreateValueString ();
		}

		private decimal ReadDecimal (bool floatingPoint)
		{
			bufferIndex = 0;
			do {
				int c = PeekChar ();
				if (c < 0) {
					ReadChar ();
					break;
				}
				// FIXME: more complex
				if (Char.IsNumber ((char) c)) {
					ReadChar ();
					AddValueChar ((char) c);
					continue;
				}
				else
					break;
			} while (true);
			string s = (floatingPoint ? "" : ".") + CreateValueString ();
			return decimal.Parse (s);
		}

		private string ReadOneToken ()
		{
			bufferIndex = 0;
			bool loop = true;
			do {
				int c = PeekChar ();
				switch (c) {
				case -1:
				case ' ':
				case '\t':
				case '\r':
				case '\n':
					loop = false;
					break;
				default:
					if (!IsTokenContinuable (c)) {
						if (c == ':') {
							if (prefixName != null)
								throw new XmlQueryCompileException ("Invalid colon was found.");
							prefixName = CreateValueString ();
						}
						loop = false;
						break;
					}

					ReadChar ();
					AddValueChar ((char) c);
					break;
				}
			} while (loop);

			return CreateValueString ();
		}

		private bool IsTokenContinuable (int c)
		{
			switch (c) {
			case '-':
			case '_':
			case '.':
				return true;
			}
			return XmlChar.IsNCNameChar (c);
		}

	}

	public enum WhitespaceHandling {
		Arbitrary,
		Explicit,
		Significant
	}

	public enum ParseState {
		Default,
		Operator,
		NamespaceDecl,
		NamespaceKeyword,
		XmlSpaceDecl,
		ItemType,
		KindTest,
		KindTestForPI,
		CloseKindTest,
		OccurenceIndicator,
		SchemaContextStep,
		VarName,
		StartTag,
		ElementContent,
		EndTag,
		XmlComment,
		ExprComment,
		ExtKey,
		XmlPI,
		XmlPIContent,
		CDataSection,
		QuotAttributeContent,
		AposAttributeContent,
	}

}
#endif
