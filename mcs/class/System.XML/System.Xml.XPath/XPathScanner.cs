//
// System.Xml.XPath.XPathScanner
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.IO;
using System.Text;

// [28] ExprToken         ::= '(' | ')' | '[' | ']' | '.' | '..' | '@' | ',' | '::'
//                            | NameTest
//                            | NodeType
//                            | Operator
//                            | FunctionName
//                            | AxisName
//                            | Literal
//                            | Number
//                            | VariableReference
// [29] Literal           ::= '"' [^"]* '"'
//                            | "'" [^']* "'"
// [30] Number            ::= Digits ('.' Digits?)?
//                            | '.' Digits
// [31] Digits            ::= [0-9]+
// [32] Operator          ::= OperatorName
//                            | MultiplyOperator
//                            | '/' | '//' | '|' | '+' | '-' | '=' | '!=' | '<' | '<=' | '>' | '>='
// [33] OperatorName      ::= 'and' | 'or' | 'mod' | 'div'
// [34] MultiplyOperator  ::= '*'
// [35] FunctionName      ::= QName - NodeType
// [36] VariableReference ::= '$' QName
// [37] NameTest          ::= '*'
//                            | NCName ':' '*'
//                            | QName
// [38] NodeType          ::= 'comment'
//                            | 'text'
//                            | 'processing-instruction'
//                            | 'node'
// [39] ExprWhitespace    ::= S

namespace System.Xml.XPath
{
	public enum XPathTokenType
	{
		Start,
		End,
		Error,
		LeftParen,
		RightParen,
		LeftBracket,
		RightBracket,
		Dot,
		DotDot,
		At,
		Comma,
		ColonColon,
		NameTest,
		NodeType,
		Operator,
		FunctionName,
		AxisName,
		Literal,
		Number,
		VariableReference
	}

	public class XPathScanner
	{
		private string xpath;
		private int index;
		private XPathTokenType tokenType;
		private string value;
		private XPathTokenType precedingTokenType;

		public XPathScanner (string xpath)
		{
			this.xpath = xpath;
			index = 0;
			tokenType = XPathTokenType.Start;
		}

		public XPathTokenType TokenType {
			get {
				return tokenType;
			}
		}

		public string Value {
			get {
				return value;
			}
		}

		private int Read ()
		{
			int c = Peek ();
			if (c != -1)
				MoveNext ();
			return c;
		}

		private int Peek ()
		{
			if (index < xpath.Length)
				return xpath[index];
			return -1;
		}

		private int Peek2 ()
		{
			if (index + 1 < xpath.Length)
				return xpath[index + 1];
			return -1;
		}

		private void MoveNext ()
		{
			++index;
		}

		private void MovePrevious ()
		{
			if (index > 0)
				--index;
		}

		public XPathTokenType Scan ()
		{
			precedingTokenType = tokenType;

			int c = Read ();

			if (c == -1) {
				tokenType = XPathTokenType.End;
				value = null;
			} else if (c != ':' && XmlChar.IsFirstNameChar (c)) {
				StringBuilder builder = new StringBuilder ();
				builder.Append ((char) c);
				while (Peek () != ':' && XmlChar.IsNameChar (Peek ())) {
					builder.Append ((char) Read ());
				}
				if (Peek () == ':' && Peek2 () != ':') {
					Read();
					if (XmlChar.IsFirstNameChar (Peek ())) {
						builder.Append (':');
						builder.Append ((char) Read ());
						while (XmlChar.IsNameChar (Peek ())) {
							builder.Append ((char) Read ());
						}
						tokenType = XPathTokenType.NameTest;
					} else if (Peek () == '*') {
						builder.Append (':');
						builder.Append ((char) Read ());
						tokenType = XPathTokenType.NameTest;
						value = builder.ToString ();
						return tokenType;
					} else {
						tokenType = XPathTokenType.Error;
						return tokenType;
					}
				}
				value = builder.ToString ();
				if (precedingTokenType != XPathTokenType.Start &&
					precedingTokenType != XPathTokenType.At &&
					precedingTokenType != XPathTokenType.ColonColon &&
					precedingTokenType != XPathTokenType.LeftParen &&
					precedingTokenType != XPathTokenType.LeftBracket &&
					precedingTokenType != XPathTokenType.Operator)
					tokenType = XPathTokenType.Operator;
				else if (Peek () == '(') {
					if (value == "comment" || 
						value == "node" || 
						value == "processing-instruction" || 
						value == "text")
						tokenType = XPathTokenType.NodeType;
					else
						tokenType = XPathTokenType.FunctionName;
				} else {
					if (Peek () == ':' && Peek2 () == ':')
						tokenType = XPathTokenType.AxisName;
					else
						tokenType = XPathTokenType.NameTest;
				}
				value = builder.ToString ();
			} else {
				switch (c) {
				case '(':
					tokenType = XPathTokenType.LeftParen;
					value = "(";
					break;
				case ')':
					tokenType = XPathTokenType.RightParen;
					value = ")";
					break;
				case '[':
					tokenType = XPathTokenType.LeftBracket;
					break;
				case ']':
					tokenType = XPathTokenType.RightBracket;
					break;
				case '.':
					if (Peek () != '.') {
						tokenType = XPathTokenType.Dot;
						value = ".";
					} else {
						Read ();
						tokenType = XPathTokenType.DotDot;
						value = "..";
					}
					break;
				case '@':
					tokenType = XPathTokenType.At;
					value = "@";
					break;
				case ',':
					tokenType = XPathTokenType.Comma;
					break;
				case ':':
					if (Peek () == ':') {
						Read ();
						tokenType = XPathTokenType.ColonColon;
						value = "::";
					} else
						tokenType = XPathTokenType.Error;
					break;
				case '*':
					if (precedingTokenType != XPathTokenType.Start &&
						precedingTokenType != XPathTokenType.At &&
						precedingTokenType != XPathTokenType.ColonColon &&
						precedingTokenType != XPathTokenType.LeftParen &&
						precedingTokenType != XPathTokenType.LeftBracket &&
						precedingTokenType != XPathTokenType.Operator) {
						tokenType = XPathTokenType.Operator;
						value = "*";
					} else {
						tokenType = XPathTokenType.NameTest;
						value = "*";
					}
					break;
				default:
					if (c == '/') {
						tokenType = XPathTokenType.Operator;
						if (Peek () != '/')
							value = "/";
						else {
							Read ();
							value = "//";
						}
					}
					break;
				}
			}

			return tokenType;
		}
	}
}
